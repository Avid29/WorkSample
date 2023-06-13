// Adam Dernis 2023

//
//                                    Excerpt Summary
// ----------------------------------------------------------------------------
// 
//      This is an excerpt of a MIPS assmebler that I wrote out of self interest.
// The instruction parser will identify register components and immediate components.
// The immediate components are handled by an ExpressionParser (not included) and
// the registers are parsed in TryParseRegister, then read from a table (included).
//
//      The ExpressionParser also needs access to the symbol and macro table, so the
// ObjectModuleConstructor is passed through the constructor into the ExpressionParser.
// This is an optional argument though, to allow for testing on the InstructionParser
// without creating an object module. The expressions simply won't be able to contain
// symbols or macros.
//
// ----------------------------------------------------------------------------
// 
// This is an excerpt from my MIPS.Interpreter project on GitHub.
//
// The full project can be found here:
// https://github.com/Avid29/MIPS.Interpreter
//
// A link to the active file is available here:
// https://github.com/Avid29/MIPS.Interpreter/blob/master/src/MIPS.Assembler/Parsers/InstructionParser.cs
//
// And a permalink to when this excerpt was taken is available here:
// https://github.com/Avid29/MIPS.Interpreter/blob/99e84f2b37eb0c64a145bde79f31144b40f63c67/src/MIPS.Assembler/Parsers/InstructionParser.cs
// 

namespace WorkSample.MIPS;

/// <summary>
/// A struct for parsing instructions.
/// </summary>
public struct InstructionParser
{
    private ILogger? _logger;
    private ExpressionParser _expParser;

    private InstructionMetadata _meta;

    private OperationCode _opCode;
    private FunctionCode _funcCode;
    private Register _rs;
    private Register _rt;
    private Register _rd;
    private byte _shift;
    private short _immediate;
    private uint _address;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="InstructionParser"/> struct.
    /// </summary>
    public InstructionParser() : this(null, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InstructionParser"/> struct.
    /// </summary>
    public InstructionParser(ObjectModuleConstructor? obj, ILogger? logger)
    {
        _logger = logger;
        _expParser = new ExpressionParser(obj, logger);
        _meta = default;
        _opCode = default;
        _funcCode = default;
        _rs = default;
        _rt = default;
        _rd = default;
        _shift = default;
        _immediate = default;
        _address = default;
    }

    /// <summary>
    /// Attempts to parse an instruction from a name and a list of arguments.
    /// </summary>
    /// <param name="name">The instruction name.</param>
    /// <param name="args">The instruction arguments.</param>
    /// <param name="instruction">The <see cref="Instruction"/>.</param>
    /// <returns>Whether or not an instruction was parsed.</returns>
    public bool TryParse(string name, string[] args, out Instruction instruction)
    {
        instruction = default;

        // Get instruction metadata from name
        if (!ConstantTables.TryGetInstruction(name, out _meta))
        {
            _logger?.Log(Severity.Error, LogId.InvalidInstructionName, $"Instruction named '{name}' could not be found.");
            return false;
        }

        // Assert proper argument count for instruction
        if (args.Length != _meta.ArgumentPattern.Length)
        {
            _logger?.Log(Severity.Error, LogId.InvalidInstructionArgCount, $"Instruction '{name}' had {args.Length} arguments instead of {_meta.ArgumentPattern.Length}.");
            return false;
        }

        // Assign op code and function code
        _opCode = _meta.OpCode;
        _funcCode = _meta.FuncCode;

        // Parse argument data according to pattern
        Argument[] pattern = _meta.ArgumentPattern;
        for (int i = 0; i < args.Length; i++)
            TryParseArg(args[i], pattern[i]);

        // Create the instruction from its components based on the instruction type
        instruction = _meta.Type switch
        {
            InstructionType.R => Instruction.Create(_funcCode, _rs, _rt, _rd, _shift),
            InstructionType.I => Instruction.Create(_opCode, _rs, _rt, _immediate),
            InstructionType.J => Instruction.Create(_opCode, _address),
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<Instruction>($"Invalid instruction type '{_meta.Type}'."),
        };

        return true;
    }

    private bool TryParseArg(string arg, Argument type)
    {
        // Trim whitespace from argument
        arg = arg.Trim();

        switch (type)
        {
            // Register type argument
            case Argument.RS:
            case Argument.RT:
            case Argument.RD:
                return TryParseRegisterArg(arg, type);
            // Immediate type argument
            case Argument.Shift:
            case Argument.Immediate:
            case Argument.Address:
                return TryParseExpressionArg(arg, type);
            // Address offset type argument
            case Argument.AddressOffset:
                return TryParseAddressOffsetArg(arg);

            // Invalid type
            default:
                return ThrowHelper.ThrowArgumentOutOfRangeException<bool>($"Argument '{arg}' of type '{type}' is not within parsable type range.");
        }
    }

    /// <summary>
    /// Parses an argument as a register and assigns it to the target component.
    /// </summary>
    private bool TryParseRegisterArg(string arg, Argument target)
    {
        // Get reference to selected register argument
        ref Register reg = ref _rs;
        switch (target)
        {
            case Argument.RS:
                reg = ref _rs;
                break;
            case Argument.RT:
                reg = ref _rt;
                break;
            case Argument.RD:
                reg = ref _rd;
                break;

            // Invalid target type
            default:
                return ThrowHelper.ThrowArgumentOutOfRangeException<bool>($"Argument '{arg}' of type '{target}' attempted to parse as a register.");
        }

        if (!TryParseRegister(arg, out var register))
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
    private bool TryParseExpressionArg(string arg, Argument target)
    {
        // Attempt to parse expression
        if (!_expParser.TryParse(arg, out var value))
            return false;

        // NOTE: Casting might truncate the value to fit the bit size.
        // This is the desired behavior, but when logging errors this
        // should be handled explicitly and drop an assembler warning.
        
        // Determine the bits allowed by the 
        int bitCount = target switch
        {
            Argument.Shift => 5,
            Argument.Immediate => 16,
            Argument.Address => 26,
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<int>($"Argument '{arg}' of type '{target}' attempted to parse as an expression."),
        };

        // Shift and Address are unsigned. Immediate is the only signed argument
        bool signed = target is Argument.Immediate;

        // Clean integer to fit within argument bit size and match signs.
        switch (CleanInteger(ref value, bitCount, signed, out var original))
        {
            case 0:
                // Integer was already clean
                break;

            case 1:
                // Integer was negative, but needs to be unsigned.
                // Also may have been truncated.
                _logger?.Log(Severity.Warning, LogId.IntegerTruncated, $"Expression '{arg}' evaluated to signed value {original}," +
                                                                       $" but was cast to unsigned value and truncated to {bitCount}-bits, resulting in {value}.");
                break;
            case 2:
                // Integer was truncated.
                _logger?.Log(Severity.Warning, LogId.IntegerTruncated, $"Expression '{arg}' evaluated to {original}," +
                                                                       $" but was truncated to {bitCount}-bits, resulting in {value}.");
                break;
        }

        // Assign to appropriate instruction argument
        switch (target)
        {
            case Argument.Shift:
                _shift = (byte)value;
                return true;
            case Argument.Immediate:
                _immediate = (short)value;
                return true;
            case Argument.Address:
                _address = (uint)value;
                return true;
                
            // Invalid target type
            default:
                return ThrowHelper.ThrowArgumentOutOfRangeException<bool>($"Argument '{arg}' of type '{target}' attempted to parse as an expression.");
        }
    }

    /// <summary>
    /// Parses an argument as an address offset, assigning its components to immediate and $rs.
    /// </summary>
    private bool TryParseAddressOffsetArg(string arg)
    {
        // NOTE: Be careful about forwards to other parse functions with regards to 
        // error logging. Address offset argument errors might be inappropriately logged.


        // Split the string into an offset and a register, return false if failed
        if (!TokenizeAddressOffset(arg, out var offsetStr, out var regStr))
            return false;
        
        // Try parse offset component into immediate, return false if failed
        if (!TryParseExpressionArg(offsetStr, Argument.Immediate))
            return false;

        // Parse register component into $rs, return false if failed
        if(!TryParseRegisterArg(regStr, Argument.RS))
            return false;

        return true;
    }

    private bool TryParseRegister(string arg, out Register register)
    {
        register = Register.Zero;

        // Check that argument is register argument
        if (arg[0] != '$')
        {
            _logger?.Log(Severity.Error, LogId.InvalidRegisterArgument, $"Expected register argument. Found '{arg}'");
            return false;
        }

        // Get register from table
        if (!ConstantTables.TryGetRegister(arg[1..], out register))
        {
            // Register does not exist in table
            _logger?.Log(Severity.Error, LogId.InvalidRegisterArgument, $"{arg} is not a valid register.");
            return false;
        }

        return true;
    }

    /// <remarks>
    /// Upon return offset and register do not need to be valid offset and register strings.
    /// The register is just the component in parenthesis. The offset is just the component before the parenthesis.
    /// Nothing may follow the parenthesis.
    /// </remarks>
    private bool TokenizeAddressOffset(string arg, out string offset, out string register)
    {
        offset = string.Empty;
        register = string.Empty;

        // Find parenthesis start and end
        // Parenthesis wrap the register
        int regStart = arg.IndexOf('(');
        int regEnd = arg.IndexOf(')');

        // Parenthesis pair was not found
        // Or contains both an opening and closing parenthesis, but they are not matched.
        // Or there was content following the parenthesis 
        if (regStart == -1 || regEnd == -1 || regStart > regEnd || regEnd != arg.Length - 1)
        {
            _logger?.Log(Severity.Error, LogId.InvalidAddressOffsetArgument, $"Argument '{arg}' is not a valid address offset.");
            return false;
        }

        // Split argument into offset and register components
        // Argument and offset validity will be assessed outside of tokenization
        offset = arg[..regStart];
        register = arg[(regStart + 1)..regEnd];

        return true;
    }
    
    /// <returns>
    /// 0 if unchanged, 1 if signChanged (maybe also have been truncated), and 2 if truncated.
    /// </returns>
    private static int CleanInteger(ref long integer, int bitCount, bool signed, out long original)
    {
        original = integer;

        // Truncate integer to bit count
        long mask = (1 << bitCount) - 1;
        integer &= mask;

        // Check for sign change
        if (!signed && original < 0)
        {
            // Remove sign from truncated integer
            // NOTE: This assumes bitCount is less than 32

            integer = (uint)integer;
            return 1;
        }

        // Check if truncated
        if (integer != original)
            return 2;

        return 0;
    }
}

