// Adam Dernis 2023

//
//                                    Excerpt Summary
// ----------------------------------------------------------------------------
// 
//      This is an enum of mips instruction arguments. It is included for
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
// https://github.com/Avid29/MIPS.Interpreter/blob/master/src/MIPS/Models/Instructions/Enums/Argument.cs
//
// And a permalink to when this excerpt was taken is available here:
// https://github.com/Avid29/MIPS.Interpreter/blob/9a036730b5ad2a5a97787e5bc82bc33218e26711/src/MIPS/Models/Instructions/Enums/Argument.cs
// 

namespace WorkSample.MIPS;

/// <summary>
/// An enum for potential argument types.
/// </summary>
public enum Argument
{
#pragma warning disable CS1591

    // Registers
    RS,
    RT,
    RD,

    Shift,
    Immediate,
    Address,
    AddressOffset,

#pragma warning restore CS1591
}
