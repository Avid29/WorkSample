// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

//
//                                    Excerpt Summary
// ----------------------------------------------------------------------------
// 
//      This is a continuation of the MarqueeText class.
//
// ----------------------------------------------------------------------------
// 
// This is an excerpt from the Windows Community Toolkit Labs.

// The full project can be found here:
// https://github.com/CommunityToolkit/Labs-Windows
//
// A link to the active file is available here:
// https://github.com/CommunityToolkit/Labs-Windows/blob/main/components/MarqueeText/src/MarqueeText.Events.cs
//
// And a permalink to when this excerpt was taken is available here:
// https://github.com/CommunityToolkit/Labs-Windows/blob/073e60f871e1861f33feefe52d8a5bbe101bedcd/components/MarqueeText/src/MarqueeText.Events.cs
// 

namespace WorkSample.MarqueeText;

/// <summary>
/// A Control that displays Text in a Marquee style.
/// </summary>
public partial class MarqueeText
{
    /// <summary>
    /// Event raised when the Marquee begins scrolling.
    /// </summary>
    public event EventHandler? MarqueeBegan;

    /// <summary>
    /// Event raised when the Marquee stops scrolling for any reason.
    /// </summary>
    public event EventHandler? MarqueeStopped;

    /// <summary>
    /// Event raised when the Marquee completes scrolling.
    /// </summary>
    public event EventHandler? MarqueeCompleted;

    private void MarqueeText_Unloaded(object sender, RoutedEventArgs e)
    {
        this.Unloaded -= MarqueeText_Unloaded;

        if (_marqueeContainer is not null)
        {
            _marqueeContainer.SizeChanged -= Container_SizeChanged;
        }

        if (_marqueeStoryboard is not null)
        {
            _marqueeStoryboard.Completed -= StoryBoard_Completed;
        }
    }

    private void Container_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (_marqueeContainer is not null)
        {
            // Clip the marquee within its bounds
            _marqueeContainer.Clip = new RectangleGeometry
            {
                Rect = new Rect(0, 0, e.NewSize.Width, e.NewSize.Height)
            };
        }

        // The marquee should run when the size changes in case the text gets cutoff
        StartMarquee();
    }

    private void StoryBoard_Completed(object? sender, object e)
    {
        StopMarquee(true);
        MarqueeCompleted?.Invoke(this, EventArgs.Empty);
    }
}