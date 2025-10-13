// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//
//                                    Excerpt Summary
// ----------------------------------------------------------------------------
// 
//      This is an excerpt from the Windows Community Toolkit Labs. 
//      This file contains the property definitions and property changed
//      callback definitions for the ColorPaletteSampler. 
//
// ----------------------------------------------------------------------------
// 
//
// The full component can be found here:
// https://github.com/CommunityToolkit/Labs-Windows/tree/main/components/ColorAnalyzer/src/ColorPaletteSampler
//
// A link to the active file is available here:
// https://github.com/CommunityToolkit/Labs-Windows/blob/main/components/ColorAnalyzer/src/ColorPaletteSampler/ColorPaletteSampler.Properties.cs
//
// A permalink to the full file from when this excerpt was taken is available here:
// https://github.com/CommunityToolkit/Labs-Windows/blob/acda1c3df1b71dbf77bc80a0544eae7955398d49/components/ColorAnalyzer/src/ColorPaletteSampler/ColorPaletteSampler.Properties.cs
// 

namespace CommunityToolkit.WinUI.Helpers;

public partial class ColorPaletteSampler
{
    /// <summary>
    /// Gets the <see cref="DependencyProperty"/> for the <see cref="Source"/> property.
    /// </summary>
    public static readonly DependencyProperty SourceProperty =
        DependencyProperty.Register(nameof(Source), typeof(UIElement), typeof(ColorPaletteSampler), new PropertyMetadata(null, OnSourceChanged));

    /// <summary>
    /// An event fired when the <see cref="Palette"/> and <see cref="PaletteSelectors"/> are updated.
    /// </summary>
    public event EventHandler? PaletteUpdated;

    /// <summary>
    /// Gets or sets the <see cref="UIElement"/> source sampled for a color palette.
    /// </summary>
    public UIElement? Source
    {
        get => (UIElement)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    /// <summary>
    /// The list of <see cref="ColorPaletteSelector"/> to update when the <see cref="Source"/> is set or changed.
    /// </summary>
    public IList<ColorPaletteSelector> PaletteSelectors { get; set; }

    /// <summary>
    /// Gets the set of <see cref="PaletteColor"/> extracted on last update.
    /// </summary>
    /// <remarks>
    /// The palette is the set of colors extracted from the <see cref="Source"/> element, and
    /// the fraction of the image that each <see cref="PaletteColor"/> covers.
    /// </remarks>
    public IReadOnlyList<PaletteColor>? Palette { get; private set; }

    private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ColorPaletteSampler analyzer)
            return;

        _ = analyzer.UpdatePaletteAsync();
    }
}
