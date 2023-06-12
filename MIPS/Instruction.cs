// Adam Dernis 2023

//
//                                    Excerpt Summary
// ----------------------------------------------------------------------------
// 
//      This is a struct representing an instruction in MIPS. It is stored as a
// bit field on a 4-byte unsigned integer. It contains properties that use bit
// shifting and masking to directly get and set the instruction components from
// the bit field.
//
// ----------------------------------------------------------------------------
// 
// This is an excerpt from my MIPS.Interpreter project on GitHub.
//
// The full project can be found here:
// https://github.com/Avid29/MIPS.Interpreter
//
// A link to the active file is available here:
// https://github.com/Avid29/MIPS.Interpreter/blob/master/src/MIPS/Models/Instructions/Instruction.cs
//
// And a permalink to when this excerpt was taken is available here:
// https://github.com/Avid29/MIPS.Interpreter/blob/1aafed49d45c5ffd40fd9fb21ad3b80ed2ea7ff8/src/MIPS/Models/Instructions/Instruction.cs
// 

namespace WorkSample.MIPS;

/// <summary>
/// A struct representing an instruction.
/// </summary>
public struct Instruction
{
    // Universal
    private const int OPCODE_BIT_SIZE = 6;
    private const int REGISTER_ADDRESS_BIT_SIZE = 5;
    private const int OPCODE_BIT_OFFSET = ADDRESS_BIT_SIZE;

    // R Type
    private const int SHIFT_AMOUNT_BIT_SIZE = 5;
    private const int FUNCTION_BIT_SIZE = 6;

    private const int RS_BIT_OFFSET = REGISTER_ADDRESS_BIT_SIZE + RT_BIT_OFFSET;
    private const int RT_BIT_OFFSET = (REGISTER_ADDRESS_BIT_SIZE + RD_BIT_OFFSET);
    private const int RD_BIT_OFFSET = (SHIFT_AMOUNT_BIT_SIZE + SHIFT_AMOUNT_BIT_OFFSET);

    private const int SHIFT_AMOUNT_BIT_OFFSET = (FUNCTION_BIT_SIZE + FUNCTION_BIT_OFFSET);
    private const int FUNCTION_BIT_OFFSET = 0;

    // I Type
    private const int IMMEDIATE_BIT_SIZE = 16;
    private const int IMMEDIATE_BIT_OFFSET = 0;

    // J Type
    private const int ADDRESS_BIT_SIZE = 26;
    private const int ADDRESS_BIT_OFFSET = 0;

    private uint _inst;

    /// <summary>
    /// Creates a new r-type instruction.
    /// </summary>
    public static Instruction Create(FunctionCode funcCode, Register rs, Register rt, Register rd, byte shiftAmount = 0)
    {
        Instruction value = default;
        value.OpCode = OperationCode.RType;
        value.RS = rs;
        value.RT = rt;
        value.RD = rd;
        value.ShiftAmount = shiftAmount;
        value.FuncCode = funcCode;
        return value;
    }

    /// <summary>
    /// Creates a new i-type instruction.
    /// </summary>
    public static Instruction Create(OperationCode opCode, Register rs, Register rt, short immediate)
    {
        Instruction value = default;
        value.OpCode = opCode;
        value.RS = rs;
        value.RT = rt;
        value.ImmediateValue = immediate;
        return value;
    }

    /// <summary>
    /// Creates a new j-type instruction.
    /// </summary>
    public static Instruction Create(OperationCode opCode, uint address)
    {
        Instruction value = default;
        value.OpCode = opCode;
        value.Address = address;
        return value;
    }

    /// <summary>
    /// Gets the instruction type.
    /// </summary>
    public InstructionType Type => InstructionTypeHelper.GetInstructionType(OpCode);

    /// <summary>
    /// Gets the instruction's operation code.
    /// </summary>
    public OperationCode OpCode
    {
        get => (OperationCode)GetShiftMask(OPCODE_BIT_SIZE, OPCODE_BIT_OFFSET);
        private set => SetShiftMask(OPCODE_BIT_SIZE, OPCODE_BIT_OFFSET, (uint)value);
    }

    /// <summary>
    /// Gets the instruction's RS Register 
    /// </summary>
    public Register RS
    {
        get => (Register)GetShiftMask(REGISTER_ADDRESS_BIT_SIZE, RS_BIT_OFFSET);
        private set => SetShiftMask(REGISTER_ADDRESS_BIT_SIZE, RS_BIT_OFFSET, (uint)value);
    }

    /// <summary>
    /// Gets the instruction's RS Register 
    /// </summary>
    public Register RT
    {
        get => (Register)GetShiftMask(REGISTER_ADDRESS_BIT_SIZE, RT_BIT_OFFSET);
        private set => SetShiftMask(REGISTER_ADDRESS_BIT_SIZE, RT_BIT_OFFSET, (uint)value);
    }

    /// <summary>
    /// Gets the instruction's RS Register 
    /// </summary>
    public Register RD
    {
        get => (Register)GetShiftMask(REGISTER_ADDRESS_BIT_SIZE, RD_BIT_OFFSET);
        private set => SetShiftMask(REGISTER_ADDRESS_BIT_SIZE, RD_BIT_OFFSET, (uint)value);
    }

    /// <summary>
    /// Gets the instruction's RS Register 
    /// </summary>
    public byte ShiftAmount
    {
        get => (byte)GetShiftMask(SHIFT_AMOUNT_BIT_SIZE, SHIFT_AMOUNT_BIT_OFFSET);
        private set => SetShiftMask(SHIFT_AMOUNT_BIT_SIZE, SHIFT_AMOUNT_BIT_OFFSET, (uint)value);
    }
    
    /// <summary>
    /// Gets the instruction's function code.
    /// </summary>
    /// <remarks>
    /// Instruction may or may not have function code.
    /// </remarks>
    public FunctionCode FuncCode
    {
        get => (FunctionCode)GetShiftMask(FUNCTION_BIT_SIZE, FUNCTION_BIT_OFFSET);
        private set => SetShiftMask(FUNCTION_BIT_SIZE, FUNCTION_BIT_OFFSET, (uint)value);
    }

    /// <summary>
    /// Gets the instruction's immediate value.
    /// </summary>
    public short ImmediateValue
    {
        get => (short)GetShiftMask(IMMEDIATE_BIT_SIZE, IMMEDIATE_BIT_OFFSET);
        private set => SetShiftMask(IMMEDIATE_BIT_SIZE, IMMEDIATE_BIT_OFFSET, (ushort)value);
    }

    /// <summary>
    /// Gets the instruction's immediate value.
    /// </summary>
    public uint Address
    {
        get => GetShiftMask(ADDRESS_BIT_SIZE, ADDRESS_BIT_OFFSET);
        private set => SetShiftMask(ADDRESS_BIT_SIZE, ADDRESS_BIT_OFFSET, value);
    }

    /// <summary>
    /// Casts a <see cref="uint"/> to a <see cref="Instruction"/>.
    /// </summary>
    public static unsafe explicit operator Instruction(uint value) => *(Instruction*)&value;

    /// <summary>
    /// Casts a <see cref="uint"/> to a <see cref="Instruction"/>.
    /// </summary>
    public static unsafe explicit operator uint(Instruction value) => *(uint*)&value;

    private readonly uint GetShiftMask(int size, int offset)
    {
        // Generate the mask by taking 2^(size) and subtracting one
        uint mask = (uint)(1 << size) - 1;

        // Shift right by the offset then mask off the size
        return mask & (_inst >> offset);
    }

    private void SetShiftMask(int size, int offset, uint value)
    {
        // Generate the mask by taking 2^(size) and subtracting one
        // Then shifting and inverting
        uint mask = (uint)(1 << size) - 1;
        mask = ~(mask << offset);

        // Clear masked section
        uint masked = _inst & mask;

        // Shift value and assign to masked section
        _inst = masked | (value << offset);
    }
}
