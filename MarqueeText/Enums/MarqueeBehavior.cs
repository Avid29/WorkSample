// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

//
//                                    Excerpt Summary
// ----------------------------------------------------------------------------
// 
//      This is an enum of behaviors for a marquee. It is included for
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
/// How the Marquee moves.
/// </summary>
public enum MarqueeBehavior
{
    /// <summary>
    /// The text flows across the screen from start to finish.
    /// </summary>
    Ticker,

    /// <summary>
    /// As the text flows across the screen a duplicate follows.
    /// </summary>
    /// <remarks>
    /// Looping text won't move if all the text already fits on the screen.
    /// </remarks>
    Looping,

#if !HAS_UNO
    /// <summary>
    /// The text bounces back and forth across the screen.
    /// </summary>
    Bouncing,
#endif
}