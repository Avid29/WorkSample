// Avishai Dernis 2025
//
//                                    Excerpt Summary
// ----------------------------------------------------------------------------
// 
//      This is an excerpt of a MIPS assmebler that I wrote out of self
// interest. It is an instruction parser which takes tokenized lines of
// assembly, and parses them into encoded instruction structs. The instruction
// parser will grab the instruction metadata from an instruction table, then
// parse the instruction arguments according to the metadata argument pattern.
// Immediate values are parsed with help from the ExpressionParser, which is
// not included in this excerpt.
//
//      Finally all the parsed components are combined
// with the, remaining encoding metadata to create an Instruction or
// PseudoInstruction struct, which is then wrapped in a ParsedInstruction
// object for abstraction.
//
//      Since the ExpressionParser also needs access to the symbol and macro
// table, the ObjectModuleConstructor is passed through the constructor into
// the ExpressionParser. This is an optional argument though to allow for
// testing on the InstructionParser without creating an object module. The
// expressions simply won't be able to contain symbols or macros.
//
// ----------------------------------------------------------------------------
// 
// This is an excerpt from my MIPSer project on GitHub. It is not the complete file
// (some functions have been removed for brevity), and it may not be update to date.
//
// The full project can be found here:
// https://github.com/Avid29/MIPSer
//
// A link to the active file is available here:
// https://github.com/Avid29/MIPSer/blob/main/src/MIPS.Assembler/Parsers/InstructionParser.cs
//
// A permalink to the full file from when this excerpt was taken is available here:
// https://github.com/Avid29/MIPSer/blob/main/src/MIPS.Assembler/Parsers/InstructionParser.cs
// 

/// <summary>
/// A struct for parsing instructions.
/// </summary>
public struct InstructionParser
{
    private readonly AssemblerContext? _context;
    private readonly InstructionTable _instructionTable;
    private readonly ILogger? _logger;

    private InstructionMetadata _meta;

    private GPRegister _rs;
    private GPRegister _rt;
    private GPRegister _rd;
    private FloatFormat _format;
    private byte _shift;
    private int _immediate;
    private uint _address;
    
    /// <summary>
    /// Attempts to parse an instruction from a name and a list of arguments.
    /// </summary>
    /// <param name="line">The assembly line to parse.</param>
    /// <param name="parsedInstruction">The resulting <see cref="ParsedInstruction"/>.</param>
    /// <returns>Whether or not an instruction was parsed.</returns>
    public bool TryParse(AssemblyLine line, [NotNullWhen(true)] out ParsedInstruction? parsedInstruction)
    {
        ReferenceEntry? reference = null;
        parsedInstruction = null;

        // Attempt to load the instruction
        // If successful, this will set the _meta and _format
        if (!TryParseInstruction(line, out var name))
            return false;

        // Applies provided values
        _rs = (GPRegister)(_meta.RS ?? default);
        _rt = (GPRegister)(_meta.RT ?? default);
        _rd = (GPRegister)(_meta.RD ?? default);

        // Parse argument data according to pattern
        Argument[] pattern = _meta.ArgumentPattern;
        for (int i = 0; i < line.Args.Count; i++)
        {
            // Split out next arg
            var arg = line.Args[i];
            TryParseArg(arg, pattern[i], out reference);
        }

        // It's a psuedo instruction.
        // Create a pseudo-instruction and return with reference
        // as parsed instruction.
        if (_meta.IsPseudoInstruction)
        {
            Guard.IsTrue(_meta.PseudoOp.HasValue);

            var pseudo = new PseudoInstruction
            {
                PseudoOp = _meta.PseudoOp.Value,
                RS = _rs,
                RT = _rt,
                RD = _rd,
                Immediate = _immediate,
                Address = _address,
            };

            parsedInstruction = new ParsedInstruction(pseudo, reference);
            return true;
        }

        // Build an instruction using the information from
        // _meta and all the parsed arguments
        var instruction = BuildInstruction();

        // Check for write back to zero register
        // Give a warning if not an explicit nop operation
        // TODO: Check on pseudo-instructions
        if (instruction.GetWritebackRegister() is GPRegister.Zero && name != "nop")
        {
            _logger?.Log(Severity.Message, LogId.ZeroRegWriteBack, "This instruction writes to $zero.");
        }

        parsedInstruction = new ParsedInstruction(instruction, reference);
        return true;
    }

    private bool TryParseInstruction(AssemblyLine line, [NotNullWhen(true)] out string? name)
    {
        name = line.Instruction?.Source;
        Guard.IsNotNull(name);

        // Parse out format from instruction name if present
        if (FloatFormatTable.TryGetFloatFormat(name, out _format, out var formattedName))
            name = formattedName;

        if (!_instructionTable.TryGetInstruction(name, out var metas, out var version))
        {
            // Select error message
            (LogId id, string message) = version switch
            {
                // The instruction requires a higher MIPS version
                not null when _context is null || version > _context?.Config.MipsVersion =>
                    (LogId.NotInVersion, $"The instruction '{name}' requires mips version {version:d}."),

                // The instruction is deprecated
                not null => (LogId.NotInVersion, $"The instruction '{name}' is deprecated. Last supported in mips version {version:d}."),

                // The instruction does not exist.
                null => (LogId.InvalidInstructionName, $"No instruction named '{name}'.")
            };

            // Log the error
            _logger?.Log(Severity.Error, id, message);
            return false;
        }

        // Assert instruction metadata with proper argument count exists
        if (!metas.Any(x => x.ArgumentPattern.Length == line.Args.Count))
        {
            // TODO: Improve messaging
            var message = $"Instruction '{name}' does not have the appropriate number of arguments.";
            //var message = line.Args.Count < pattern.Length
            //    ? $"Instruction '{name}' doesn't have enough arguments. Found {line.Args.Count} arguments when expecting {_meta.ArgumentPattern.Length}."
            //    : $"Instruction '{name}' has too many arguments! Found {line.Args.Count} arguments when expecting {_meta.ArgumentPattern.Length}.";

            _logger?.Log(Severity.Error, LogId.InvalidInstructionArgCount, message);
            return false;
        }

        // Find instruction pattern with matching argument count
        _meta = metas.FirstOrDefault(x => x.ArgumentPattern.Length == line.Args.Count);

        // Check that the float format is supported valid with the instruction, if applicable
        if (_meta.FloatFormats is not null && !_meta.FloatFormats.Contains(_format))
        {
            _logger?.Log(Severity.Error, LogId.InvalidFloatFormat, $"Instruction '{name}' does not support float format '{_format}'.");
            return false;
        }

        return true;
    }

    private bool TryParseArg(ReadOnlySpan<Token> arg, Argument type, out ReferenceEntry? reference)
    {
        reference = null;

        return type switch
        {
            // Register arguments
            (>= Argument.RS and <= Argument.RD) or
            (>= Argument.FS and <= Argument.FD) or
            Argument.RT_Numbered => TryParseRegisterArg(arg[0], type),

            // Expression arguments
            Argument.Shift or Argument.Immediate or
            Argument.FullImmediate or Argument.Offset
            or Argument.Address => TryParseExpressionArg(arg, type, out reference),

            // Address offset arguments
            Argument.AddressBase => TryParseAddressOffsetArg(arg, out reference),

            _ => ThrowHelper.ThrowArgumentOutOfRangeException<bool>($"Argument of type '{type}' is not within parsable type range."),
        };
    }

    /// <summary>
    /// Parses an argument as a register and assigns it to the target component.
    /// </summary>
    private unsafe bool TryParseRegisterArg(Token arg, Argument target)
    {
        // Get reference to selected register argument
        RefTuple<Ref<GPRegister>, RegisterSet> pair = target switch
        {
            // General Purpose Registers
            Argument.RS => new(new(ref _rs), RegisterSet.GeneralPurpose),
            Argument.RT => new(new(ref _rt), RegisterSet.GeneralPurpose),
            Argument.RD => new(new(ref _rd), RegisterSet.GeneralPurpose),
            // Float Registers
            Argument.FS => new(new(ref _rs), RegisterSet.FloatingPoints),
            Argument.FT => new(new(ref _rt), RegisterSet.FloatingPoints),
            Argument.FD => new(new(ref _rd), RegisterSet.FloatingPoints),
            // RT Register for coprocessors
            Argument.RT_Numbered => new(new(ref _rt), RegisterSet.Numbered),
            // Invalid target type
            _ => throw new ArgumentOutOfRangeException($"Argument of type '{target}' attempted to parse as a register.")
        };

        (Ref<GPRegister> regRef, RegisterSet set) = pair;
        ref GPRegister reg = ref regRef.Value;

        if (!TryParseRegister(arg, out var register, set))
        {
            // Register could not be parsed.
            // Error already logged.

            return false;
        }

        // Cache register as appropriate argument type
        reg = register;

        return true;
    }

    /// <summary>
    /// Parses an argument as an expression and assigns it to the target component
    /// </summary>
    private bool TryParseExpressionArg(ReadOnlySpan<Token> arg, Argument target, out ReferenceEntry? relocation)
    {
        relocation = null;
        var parser = new ExpressionParser(_context, _logger);

        // Attempt to parse expression
        if (!parser.TryParse(arg, out var address, out SymbolEntry? refSymbol))
            return false;

        // NOTE: Casting might truncate the value to fit the bit size.
        // This is the desired behavior, but when logging errors this
        // should be handled explicitly and drop an assembler warning.

        if (!address.IsFixed && target is Argument.Shift)
        {
            _logger?.Log(Severity.Error, LogId.RelocatableReferenceInShift, "Shift amount argument cannot reference relocatable symbols.");
            return false;
        }

        // TODO: Can branches make external references?

        if (!address.IsFixed && target is not Argument.Offset && _context is not null)
        {
            Guard.IsNotNull(refSymbol);

            var type = target switch
            {
                Argument.Address => ReferenceType.Address,
                Argument.Immediate => ReferenceType.Lower,
                _ => ThrowHelper.ThrowArgumentOutOfRangeException<ReferenceType>($"Argument of type '{target}' cannot reference relocatable symbols."),
            };

            var method = ReferenceMethod.Relocate;
            if (address.IsExternal)
            {
                // TODO: When is it replace or subtract?
                method = ReferenceMethod.Add;
            }

            relocation = new ReferenceEntry(refSymbol.Value.Name, _context.CurrentAddress, type, method);
        }

        long value = address.Value;

        // Truncates the value to fit the target argument
        CleanInteger(ref value, arg, target);

        // Assign to appropriate instruction argument
        switch (target)
        {
            case Argument.Shift:
                _shift = (byte)value;
                return true;
            case Argument.Immediate:
                _immediate = (short)value;
                return true;
            case Argument.FullImmediate:
                _immediate = (int)value;
                return true;
            case Argument.Address:
                _address = (uint)value;
                return true;
            case Argument.Offset:
                if (address.IsRelocatable)
                {
                    Guard.IsNotNull(_context);

                    var @base = _context.CurrentAddress + 4;
                    if (@base.Section != address.Section)
                    {
                        _logger?.Log(Severity.Error, LogId.BranchBetweenSections, $"Cannot branch between section.");
                        return false;
                    }

                    // Adjust realtive to current position
                    value -= @base.Value;
                }

                _immediate = (int)value;
                return true;

            // Invalid target type
            default:
                return ThrowHelper.ThrowArgumentOutOfRangeException<bool>($"Argument '{arg.Print()}' of type '{target}' attempted to parse as an expression.");
        }
    }

    /// <summary>
    /// Parses an argument as an address offset, assigning its components to immediate and $rs.
    /// </summary>
    private bool TryParseAddressOffsetArg(ReadOnlySpan<Token> arg, out ReferenceEntry? relSymbol)
    {
        relSymbol = null;

        // NOTE: Be careful about forwards to other parse functions with regards to 
        // error logging. Address offset argument errors might be inappropriately logged.

        // Split the string into an offset and a register, return false if failed
        if (!SplitAddressOffset(arg, out var offsetStr, out var regStr))
            return false;

        // Try parse offset component into immediate, return false if failed
        if (!TryParseExpressionArg(offsetStr, Argument.Immediate, out relSymbol))
            return false;

        // Parse register component into $rs, return false if failed
        if (!TryParseRegisterArg(regStr, Argument.RS))
            return false;

        return true;
    }

    private readonly bool TryParseRegister(Token arg, out GPRegister register, RegisterSet set = RegisterSet.GeneralPurpose)
    {
        register = GPRegister.Zero;

        // Check that argument is register argument
        var regStr = arg.Source;
        if (regStr[0] != '$')
        {
            _logger?.Log(Severity.Error, LogId.InvalidRegisterArgument, $"'{arg}' is not a valid register argument.");
            return false;
        }

        // Get named register from table
        if (!RegistersTable.TryGetRegister(regStr, out register, out RegisterSet parsedSet))
        {
            // Register does not exist in table
            _logger?.Log(Severity.Error, LogId.InvalidRegisterArgument, $"No register '{arg}' exists.");
            return false;
        }

        // Match register set
        if (parsedSet != RegisterSet.Numbered && parsedSet != set)
        {
            _logger?.Log(Severity.Error, LogId.InvalidRegisterArgument, $"Register '{arg}' is not parse of register set '{set}'.");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Splits an address offset argument into a token span for the offset and the address register token.
    /// </summary>
    /// <remarks>
    /// Upon return offset and register do not need to be valid offset and register strings.
    /// The register is just the component in parenthesis. The offset is just the component before the parenthesis.
    /// Nothing may follow the parenthesis.
    /// </remarks>
    private readonly bool SplitAddressOffset(ReadOnlySpan<Token> arg, out ReadOnlySpan<Token> offset, [NotNullWhen(true)] out Token? register)
    {
        var original = arg;
        register = null;
        offset = arg;

        // Find parenthesis start and end
        // Parenthsis must be matched and contain a single token
        var parIndex = arg.FindNext(TokenType.OpenParenthesis);
        var closeIndex = arg.FindNext(TokenType.CloseParenthesis);
        if (parIndex is -1 || closeIndex is -1 || closeIndex - parIndex != 2)
        {
            _logger?.Log(Severity.Error, LogId.InvalidAddressOffsetArgument, $"Argument '{arg.Print()}' is not a valid address offset.");
            return false;
        }

        // Offset is everything before the parenthesis
        offset = arg[..parIndex];
        arg = arg[(parIndex + 1)..];

        // Register is everything between the parenthesis,
        // and must be a single token.
        register = arg[0];
        arg = arg[1..];

        // Parenthesis pair was not found
        // Or contains both an opening and closing parenthesis, but they are not matched.
        // Or there was content following the parenthesis.
        // Or the token inside the parenthesis is not a register.
        if (arg.IsEmpty || register.Type is not TokenType.Register)
        {
            _logger?.Log(Severity.Error, LogId.InvalidAddressOffsetArgument, $"Argument '{original.Print()}' is not a valid address offset.");
            return false;
        }

        return true;
    }

    private void CleanInteger(ref long value, ReadOnlySpan<Token> arg, Argument target)
    {
        // Determine casting details for the argument
        (int bitCount, int shiftAmount, bool signed) = target switch
        {
            Argument.Shift => (5, 0, false),
            Argument.Offset => (16, 2, false),
            Argument.Immediate => (16, 0, true),
            Argument.Address => (26, 2, false),
            Argument.FullImmediate => (32, 0, true),
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<(byte, byte, bool)>($"Argument of type '{target}' attempted to parse as an expression."),
        };

        // Clean integer to fit within argument bit size and match signs.
        long original = value;
        var cleanStatus = CastInteger(ref value, bitCount, shiftAmount, signed);

        // Generate a message if any changes made to the value when casting.
        var message = cleanStatus switch
        {
            // Sign changed
            CastingChanges.SignChanged =>
            $"Expression '{arg.Print()}' evaluated to signed value {original}" +
            $"but was cast to an unsigned value, resulting in {value}.",

            // Truncated
            CastingChanges.Truncated =>
            $"Expression '{arg.Print()}' evaluated to {original}, but was truncated to " +
            $"{bitCount}-bits dropping the lower {shiftAmount} bits, resulting in {value}.",

            // Truncated and sign changed
            CastingChanges.TruncatedAndSignChanged =>
            $"Expression '{arg.Print()}' evaluated to {original}, but was truncated to an" +
            $"unsigned value with {bitCount}-bits and dropping the lower {shiftAmount} bits," +
            $"resulting in {value}.",

            // No changes
            _ => null,
        };

        // If a message was generated, log it
        if (message is not null)
        {
            _logger?.Log(Severity.Warning, LogId.IntegerTruncated, message);
        }
    }

    /// <remarks>
    /// This does not apply the <paramref name="shiftAmount"/>! It only masks the lower bits.
    /// </remarks>
    /// <param name="integer">A reference to the integer to modify.</param>
    /// <param name="bitCount">The number of bits after casting.</param>
    /// <param name="shiftAmount">The number of bits that will drop from the bottom.</param>
    /// <param name="signed">Whether or not the new value should be signed.</param>
    /// <returns>The changes made to the integer.</returns>
    private static CastingChanges CastInteger(ref long integer, int bitCount, int shiftAmount, bool signed = false)
    {
        var original = integer;

        Guard.IsGreaterThan(bitCount, 1);
        Guard.IsLessThanOrEqualTo(bitCount + shiftAmount, 64);

        // Create a masks for the high and low truncating bits,
        // as well as an overall remaining bits map.
        var upperMask = bitCount == 64 ? -1L : (1L << (bitCount + shiftAmount)) - 1;
        var lowerMask = ~((1L << shiftAmount) - 1);
        var mask = (upperMask & lowerMask);

        // Truncate mask upper and lower bits
        long truncated = integer & mask;

        // Sign extend if signed and not full width
        if (signed && bitCount < 64)
        {
            long signBit = 1L << (bitCount - 1);
            if ((truncated & signBit) != 0)
                truncated |= ~upperMask; // Sign extend
        }

        integer = truncated;

        // Compute changes
        var changes = CastingChanges.None;

        // Check if the sign changed
        if ((original < 0) != (truncated < 0))
            changes |= CastingChanges.SignChanged;

        // Check for upper truncation
        long upperBits = original & ~upperMask;
        if (upperBits != 0 && upperBits != ~upperMask)
            changes |= CastingChanges.Truncated;

        // Check for lower truncation
        if ((original & ~lowerMask) != 0)
        {
            changes |= CastingChanges.Truncated;
        }

        // Return combined code
        return changes;
    }
}
