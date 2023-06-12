// Adam Dernis 2023

//
//                                    Excerpt Summary
// ----------------------------------------------------------------------------
// 
//      This is an enum of mips instruction function codes. It is included for
// clarity in understanding InstructionParser.cs.
//
// ----------------------------------------------------------------------------
// 
// This is an excerpt from my MIPS.Interpreter project on GitHub.
//
// The full project can be found here:
// https://github.com/Avid29/MIPS.Interpreter
//
// A link to the active file is available here:
// https://github.com/Avid29/MIPS.Interpreter/blob/master/src/MIPS/Models/Instructions/Enums/FunctionCode.cs
//
// And a permalink to when this excerpt was taken is available here:
// https://github.com/Avid29/MIPS.Interpreter/blob/0d9faabcd562a5a939110c67bbc870940426d6d0/src/MIPS/Models/Instructions/Enums/FunctionCode.cs
// 

namespace WorkSample.MIPS;

/// <summary>
/// An enum for r-type instruction function codes.
/// </summary>
public enum FunctionCode : byte
{
#pragma warning disable CS1591

    None = 0,

    // Question: What is 0x01?

    ShiftLeftLogical = 0x00,
    ShiftRightLogical = 0x02,
    ShiftRightArithmetic = 0x03,

    ShiftLeftLogicalVariable = 0x04,
    ShiftRightLogicalVariable = 0x06,
    ShiftRightArithmeticVariable = 0x07,

    JumpRegister = 0x08,
    JumpAndLinkRegister = 0x09,

    SystemCall = 0x0c,

    MoveFromHigh = 0x10,
    MoveToHigh = 0x11,
    MoveFromLow = 0x12,
    MoveToLow = 0x13,

    Multiply = 0x18,
    MultiplyUnsigned = 0x19,
    Divide = 0x1a,
    DivideUnsigned = 0x1b,

    Add = 0x20,
    AddUnsigned = 0x21,
    Subtract = 0x22,
    SubtractUnsigned = 0x23,

    And = 0x24,
    Or = 0x25,
    ExclusiveOr = 0x26,
    Nor = 0x27,

    SetLessThan = 0x2a,
    SetLessThanUnsigned = 0x2b,

#pragma warning restore CS1591
}
