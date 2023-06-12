// Adam Dernis 2023

//
//                                    Excerpt Summary
// ----------------------------------------------------------------------------
// 
//      This is an enum of mips instruction operation codes. It is included for
// clarity in understanding InstructionParser.cs.
//
// ----------------------------------------------------------------------------
// 
// This is an excerpt from my MIPS.Interpreter project on GitHub.

// The full project can be found here:
// https://github.com/Avid29/MIPS.Interpreter
//
// A link to the active file is available here:
// https://github.com/Avid29/MIPS.Interpreter/blob/master/src/MIPS/Models/Instructions/Enums/OperationCode.cs
//
// And a permalink to when this excerpt was taken is available here:
// https://github.com/Avid29/MIPS.Interpreter/blob/0d9faabcd562a5a939110c67bbc870940426d6d0/src/MIPS/Models/Instructions/Enums/OperationCode.cs
// 

namespace WorkSample.MIPS;

/// <summary>
/// An enum for instruction op codes.
/// </summary>
public enum OperationCode : byte
{
    /// <summary>
    /// Marks any r-type instruction, each one shares an op-code of 0x00.
    /// </summary>
    /// <remarks>
    /// r-type instructions are distinguished with <see cref="FunctionCode"/>.
    /// </remarks>
    RType = 0x00,

#pragma warning disable CS1591

    Jump = 0x02,
    JumpAndLink = 0x03,
    
    BranchOnEquals = 0x04,
    BranchOnNotEquals = 0x05,
    BranchOnLessThanOrEqualToZero = 0x06,
    BranchGreaterThanZero = 0x07,

    AddImmediate = 0x08,
    AddImmediateUnsigned = 0x09,

    SetLessThanImmediate = 0x0a,
    SetLessThanImmediateUnsigned = 0x0b,

    AndImmediate = 0x0c,
    OrImmediate = 0x0d,
    ExclusiveOrImmediate = 0x0e,

    LoadUpperImmediate = 0x0f,

    MoveFromCoprocessor = 0x10,

    LoadByte = 0x20,
    LoadHalfWord = 0x21,
    LoadWord = 0x23,
    LoadByteUnsigned = 0x24,
    LoadHalfWordUnsigned = 0x25,

    StoreByte = 0x28,
    StoreHalfWord = 0x29,
    StoreWord = 0x2b,

#pragma warning restore CS1591
}
