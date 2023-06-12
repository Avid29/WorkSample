// Adam Dernis 2023

//
//                                    Excerpt Summary
// ----------------------------------------------------------------------------
// 
//      This is an enum of mips registers. It is included for clarity in
// understanding InstructionParser.cs.
//
// ----------------------------------------------------------------------------
// 
// This is an excerpt from my MIPS.Interpreter project on GitHub.

// The full project can be found here:
// https://github.com/Avid29/MIPS.Interpreter
//
// A link to the active file is available here:
// https://github.com/Avid29/MIPS.Interpreter/blob/master/src/MIPS/Models/Instructions/Enums/Register.cs
//
// And a permalink to when this excerpt was taken is available here:
// https://github.com/Avid29/MIPS.Interpreter/blob/0d9faabcd562a5a939110c67bbc870940426d6d0/src/MIPS/Models/Instructions/Enums/Register.cs
// 

namespace MIPS.Models.Instructions.Enums;

/// <summary>
/// An enum for addressable registers
/// </summary>
public enum Register : byte
{
#pragma warning disable CS1591

    Zero = 0,
    AssemblerTemporary = 1,
    ReturnValue0 = 2,
    ReturnValue1 = 3,
    Argument0 = 4,
    Argument1 = 5,
    Argument2 = 6,
    Argument3 = 7,
    Temporary0 = 8,
    Temporary1 = 9,
    Temporary2 = 10,
    Temporary3 = 11,
    Temporary4 = 12,
    Temporary5 = 13,
    Temporary6 = 14,
    Temporary7 = 15,
    Saved0 = 16,
    Saved1 = 17,
    Saved2 = 18,
    Saved3 = 19,
    Saved4 = 20,
    Saved5 = 21,
    Saved6 = 22,
    Saved7 = 23,
    Temporary8 = 24,
    Temporary9 = 25,
    Kernel0 = 26,
    Kernel1 = 27,
    GlobalPointer = 28,
    StackPointer = 29,
    FramePointer = 30,
    ReturnAddress = 31,

    // Non-indexable registers
    High,
    Low,

#pragma warning restore CS1591
}
