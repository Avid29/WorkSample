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
// This is an optional argument though, to allow for tests on the InstructionParser
// without creating an object module. The expressions simply won't contain symbols
// or macros.
//
//      At this stage, it throws exceptions in place of logging errors. However, there
// are notes through out the code dropping reminders of problems that may arise once errors
// are implemented.
//
// ----------------------------------------------------------------------------
// 
// This is an excerpt from my MIPS.Interpreter project on GitHub.

// The full project can be found here:
// https://github.com/Avid29/MIPS.Interpreter
//
// A link to the active file is available here:
// https://github.com/Avid29/MIPS.Interpreter/blob/master/src/MIPS.Assembler/Parsers/InstructionParser.cs
//
// And a permalink to when this excerpt was taken is available here:
// https://github.com/Avid29/MIPS.Interpreter/blob/0d9faabcd562a5a939110c67bbc870940426d6d0/src/MIPS.Assembler/Parsers/InstructionParser.cs
// 

namespace WorkSample.MIPS;

/// <summary>
/// A struct for parsing instructions.
/// </summary>
public struct InstructionParser
{
    // Definition not included in excerpt
    private ExpressionParser _expParser;
    
    // Definition not included in excerpt
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
    public InstructionParser() : this(null)
    {

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InstructionParser"/> struct.
    /// </summary>
    public InstructionParser(ObjectModuleConstructor? obj)
    {
        _expParser = new ExpressionParser(obj);
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
    /// Parses an instruction from a name and a list of arguments.
    /// </summary>
    /// <param name="name">The instruction name.</param>
    /// <param name="args">The instruction arguments.</param>
    /// <returns>An <see cref="Instruction"/>.</returns>
    public Instruction Parse(string name, string[] args)
    {
        // Get instruction metadata from name
        if (!ConstantTables.TryGetInstruction(name, out _meta))
        {
            // TODO: Log error
            ThrowHelper.ThrowInvalidDataException($"Instruction named '{name}' could not be found.");
        }

        // Assert proper argument count for instruction
        if (args.Length != _meta.ArgumentPattern.Length)
        {
            // TODO: Log error
            ThrowHelper.ThrowArgumentException($"Instruction '{name}' had {args.Length} arguments instead of {_meta.ArgumentPattern.Length}.");
        }

        // Assign op code and function code
        _opCode = _meta.OpCode;
        _funcCode = _meta.FuncCode;

        // Parse argument data according to pattern
        var pattern = _meta.ArgumentPattern;
        for (int i = 0; i < args.Length; i++)
        {
            ParseArg(args[i], pattern[i]);
        }

        // Create an instruction from its components based on the instruction type
        var instruction = _meta.Type switch
        {
            InstructionType.R => Instruction.Create(_funcCode, _rs, _rt, _rd, _shift),
            InstructionType.I => Instruction.Create(_opCode, _rs, _rt, _immediate),
            InstructionType.J => Instruction.Create(_opCode, _address),
            _ => ThrowHelper.ThrowInvalidDataException<Instruction>(""),
        };

        return instruction;
    }

    private void ParseArg(string arg, Argument type)
    {
        switch (type)
        {
            // Register type argument
            case Argument.RS:
            case Argument.RT:
            case Argument.RD:
                ParseRegisterArg(arg, type);
                break;
            // Immediate type argument
            case Argument.Shift:
            case Argument.Immediate:
            case Argument.Address:
                ParseExpressionArg(arg, type);
                break;
            // Address offset type argument
            case Argument.AddressOffset:
                ParseAddressOffsetArg(arg);
                break;
            default:
                ThrowHelper.ThrowArgumentOutOfRangeException($"Argument type '{type}' is not in range.");
                break;
        }
    }

    /// <summary>
    /// Parses an argument as a register and assigns it to the target component.
    /// </summary>
    private void ParseRegisterArg(string arg, Argument target)
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

            default:
                ThrowHelper.ThrowArgumentException($"Argument '{arg}' of type '{target}' attempted to parse as an expression.");
                break;
        }

        if (!TryParseRegister(arg, out var register))
        {
            // TODO: Log error
            // NOTE: This condition is currently not hit because any branch of
            // TryParseRegister that would return false also throws an exception.
            // When error messages are added the exceptions will be removed.
        }

        // Cache register as appropriate argument type
        reg = register;
    }

    /// <summary>
    /// Parses an argument as an expression and assigns it to the target component
    /// </summary>
    private void ParseExpressionArg(string arg, Argument target)
    {
        if (!_expParser.TryParse(arg, out var value))
        {
            // TODO: Log error and remove exception

            ThrowHelper.ThrowArgumentException($"Argument '{arg}' of type '{target}' was not a valid expression.");
        }

        // Assign to appropriate instruction argument
        switch (target)
        {
            case Argument.Shift:
                _shift = (byte)value;
                break;
            case Argument.Immediate:
                _immediate = (short)value;
                break;
            case Argument.Address:
                _address = (uint)value;
                break;
            default:
                ThrowHelper.ThrowArgumentOutOfRangeException($"Argument '{arg}' of type '{target}' attempted to parse as an expression.");
                break;
        }
    }

    /// <summary>
    /// Parses an argument as an address offset, assigning its components to immediate and $rs.
    /// </summary>
    private void ParseAddressOffsetArg(string arg)
    {
        // Split the string into an offset and a register 
        if (!TokenizeAddressOffset(arg, out var offsetStr, out var regStr))
        {
            // NOTE: This condition is currently not hit because any branch of
            // TokenizeAddressOffset that would return false also throws an exception.
            // When error messages are added the exceptions will be removed.
            return;
        }
        
        // NOTE: Be careful about forwards to other parse functions when implementing 
        // error logging. $rs and immediate errors might be inappropriately logged for
        // address offset arguments.


        // Parse offset component into immediate
        ParseExpressionArg(offsetStr, Argument.Immediate);

        // Parse register component into $rs
        ParseRegisterArg(regStr, Argument.RS);
    }

    private static bool TryParseRegister(string name, out Register register)
    {
        // Trim whitespace from register string
        name = name.Trim();

        // Check that argument is register argument
        if (name[0] != '$')
        {
            // TODO: Log error and remove exception.

            ThrowHelper.ThrowArgumentException($"Expected register argument. Found '{name}'");
            register = Register.Zero;
            return false;
        }

        // Get register from table
        if (!ConstantTables.TryGetRegister(name[1..], out register))
        {
            // TODO: Log error and remove exception.

            ThrowHelper.ThrowInvalidDataException($"{name} is not a valid register.");
            return false;
        }

        return true;
    }

    private bool TokenizeAddressOffset(string arg, out string offset, out string register)
    {
        // Find parenthesis start and end
        // Parenthesis wrap the register
        int regStart = arg.IndexOf('(');
        int regEnd = arg.IndexOf(')');

        // Either end of the parenthesis were not found
        if (regStart == -1 || regEnd == -1)
        {
            offset = string.Empty;
            register = string.Empty;

            ThrowHelper.ThrowArgumentException($"Argument '{arg}' is not a valid address offset.");
            return false;
        }

        // Split argument into offset and register components
        offset = arg[..regStart];
        register = arg[(regStart + 1)..regEnd];

        return true;
    }
}
