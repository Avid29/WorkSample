// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

//
//                                    Excerpt Summary
// ----------------------------------------------------------------------------
// 
//      This is an enum of directions for a marquee to flow. It is included for
// clarity in understanding MarqueeText.cs.
//
// ----------------------------------------------------------------------------
// 
// This is an excerpt from the Windows Community Toolkit Labs.
//
// The full project can be found here:
// https://github.com/CommunityToolkit/Labs-Windows
//
// A link to the active file is available here:
// https://github.com/CommunityToolkit/Labs-Windows/blob/main/components/MarqueeText/src/MarqueeDirection.cs
//
// And a permalink to when this excerpt was taken is available here:
// https://github.com/CommunityToolkit/Labs-Windows/blob/073e60f871e1861f33feefe52d8a5bbe101bedcd/components/MarqueeText/src/MarqueeDirection.cs
// 

namespace WorkSample.MarqueeText;

/// <summary>
/// The direction a Marquee moves.
/// </summary>
public enum MarqueeDirection
{
    /// <summary>
    /// The text will flow from left to right.
    /// </summary>
    Left,

    /// <summary>
    /// The text will flow from right to left.
    /// </summary>
    Right,

    /// <summary>
    /// The text will flow from bottom to top.
    /// </summary>
    Up,

    /// <summary>
    /// The text will flow from top to bottom.
    /// </summary>
    Down,
}