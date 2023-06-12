// Adam Dernis 2023

//
//                                    Excerpt Summary
// ----------------------------------------------------------------------------
// 
//      This is an enum of mips instruction tyoes. It is included for
// clarity in understanding InstructionParser.cs.
//
// ----------------------------------------------------------------------------
// 
// This is an excerpt from my MIPS.Interpreter project on GitHub.

// The full project can be found here:
// https://github.com/Avid29/MIPS.Interpreter
//
// A link to the active file is available here:
// https://github.com/Avid29/MIPS.Interpreter/blob/master/src/MIPS/Models/Instructions/Enums/InstructionType.cs
//
// And a permalink to when this excerpt was taken is available here:
// https://github.com/Avid29/MIPS.Interpreter/blob/0d9faabcd562a5a939110c67bbc870940426d6d0/src/MIPS/Models/Instructions/Enums/InstructionType.cs
// 

namespace WorkSample.MIPS;

/// <summary>
/// An enum for the mips instruction types
/// </summary>
public enum InstructionType
{
#pragma warning disable CS1591
    R,
    I,
    J,
#pragma warning restore CS1591
}
