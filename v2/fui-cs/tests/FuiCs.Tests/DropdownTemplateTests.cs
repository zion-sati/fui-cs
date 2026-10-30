using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using TUnit.Assertions;
using TUnit.Core;

namespace FuiCs.Tests;

public sealed class DropdownTemplateTests
{
    private static Theme Light => Theme.DefaultLight;

    private static DropdownFieldVisualState Field(bool enabled = true, bool pressed = false) =>
        new(IsOpen: false, IsFocused: false, IsEnabled: enabled, IsPressed: pressed, SelectedLabel: "Alpha");

    [Test]
    public async Task TheDefaultFieldHoldsTheValueThenTheChevronAndGrowsToFitItsSizing()
    {
        var field = DropdownFieldTemplate.Default.CreatePresenter(null);
        var grown = DropdownFieldTemplate.Default.CreatePresenter(new DropdownSizing { FieldFontSize = 20.0f, ChevronBoxSize = 40.0f });
        var fixedHeight = DropdownFieldTemplate.Default.CreatePresenter(new DropdownSizing { FieldHeight = 50.0f, ChevronBoxSize = 40.0f });

        await Assert.That(ReferenceEquals(field.Root.Children[0], field.ValueHost)).IsTrue();
        await Assert.That(ReferenceEquals(field.Root.Children[1], field.ChevronHost)).IsTrue();
        await Assert.That(ReferenceEquals(field.ValueHost.Children[0], field.ValueNode)).IsTrue();
        await Assert.That(field.Metrics).IsEqualTo(new DropdownFieldMetrics(32.0f, 16.0f, 16.0f, 16.0f, 0.0f, 16.0f, 0.0f));
        await Assert.That((grown.Metrics.Height, grown.Metrics.FontSize, grown.Metrics.ChevronBoxSize)).IsEqualTo((40.0f, 20.0f, 40.0f));
        await Assert.That(fixedHeight.Metrics.Height).IsEqualTo(50.0f);
        await Assert.That(grown.ChevronHost.Width).IsEqualTo(new Length(40.0f));
    }

    [Test]
    public async Task TheDefaultFieldFollowsItsStateTheThemeAndItsColors()
    {
        var palette = Light.Colors;
        var field = DropdownFieldTemplate.Default.CreatePresenter(null);

        field.Apply(Light, Field(), null);
        var resting = (field.Root.Background, field.Root.Border, field.Root.Height, field.ValueNode.TextColor, field.ValueNode.LineHeight);
        field.Apply(Light, Field(pressed: true), null);
        var pressed = field.Root.Background;
        field.Apply(Light, Field(enabled: false, pressed: true), null);
        var disabled = (field.Root.Background, field.ValueNode.TextColor);
        field.Apply(Light, Field(pressed: true), new DropdownColors { Background = 0x112233FFu, Border = 0x445566FFu, TextPrimary = 0x778899FFu });
        var colored = (field.Root.Background, field.Root.Border.Color, field.ValueNode.TextColor);

        await Assert.That(resting).IsEqualTo((palette.Surface, new Border(2.0f, palette.Border), new Length(32.0f), (Color?)palette.TextPrimary, (float?)32.0f));
        await Assert.That(pressed).IsEqualTo(palette.Background);
        await Assert.That(disabled).IsEqualTo((palette.Surface, (Color?)palette.TextMuted));
        await Assert.That(colored).IsEqualTo(((Color)0x112233FFu, (Color)0x445566FFu, (Color?)(Color)0x778899FFu));
    }

    [Test]
    public async Task TheDefaultChevronTurnsUpWhileOpenAndBrightensWhileHovered()
    {
        var palette = Light.Colors;
        var chevron = DropdownChevronTemplate.Default.CreatePresenter(new DropdownSizing { ChevronIconSize = 10.0f });
        var icon = (Svg)chevron.Root.Children[0];

        chevron.Apply(Light, new DropdownChevronVisualState(IsOpen: false, IsHovered: false, IsEnabled: true));
        var closed = (icon.Source, icon.Tint);
        chevron.Apply(Light, new DropdownChevronVisualState(IsOpen: true, IsHovered: true, IsEnabled: true));
        var open = (icon.Source, icon.Tint);
        chevron.Apply(Light, new DropdownChevronVisualState(IsOpen: true, IsHovered: true, IsEnabled: false));

        await Assert.That(closed.Source!.Contains("M3 4.5 6 7.5 9 4.5", StringComparison.Ordinal)).IsTrue();
        await Assert.That(closed.Tint).IsEqualTo((Color?)palette.TextMuted);
        await Assert.That(open.Source!.Contains("M3 7.5 6 4.5 9 7.5", StringComparison.Ordinal)).IsTrue();
        await Assert.That(open.Tint).IsEqualTo((Color?)palette.TextPrimary);
        await Assert.That(icon.Tint).IsEqualTo((Color?)palette.TextMuted);
        await Assert.That(icon.Width).IsEqualTo(new Length(10.0f));
    }

    [Test]
    public async Task TheDefaultOptionRowHighlightsAndShowsTheSelectedOptionInTheAccent()
    {
        var palette = Light.Colors;
        var row = DropdownOptionRowTemplate.Default.CreatePresenter(new DropdownSizing { OptionHeight = 40.0f });
        var colors = new DropdownColors { Accent = 0x16A34AFFu, TextPrimary = 0x334155FFu };

        row.Apply(Light, new DropdownOptionRowVisualState(IsHighlighted: true, IsSelected: false, IsEnabled: true), null);
        var highlighted = (row.Root.Background, row.Label.TextColor);
        row.Apply(Light, new DropdownOptionRowVisualState(IsHighlighted: false, IsSelected: true, IsEnabled: true), null);
        var selected = (row.Root.Background, row.Label.TextColor);
        row.Apply(Light, new DropdownOptionRowVisualState(IsHighlighted: false, IsSelected: true, IsEnabled: true), colors);
        var coloredSelected = row.Label.TextColor;
        row.Apply(Light, new DropdownOptionRowVisualState(IsHighlighted: false, IsSelected: false, IsEnabled: true), colors);
        var coloredPlain = row.Label.TextColor;
        row.Apply(Light, new DropdownOptionRowVisualState(IsHighlighted: false, IsSelected: true, IsEnabled: false), colors);

        await Assert.That(row.Metrics).IsEqualTo(new DropdownOptionRowMetrics(40.0f, 10.0f, 10.0f, 16.0f));
        await Assert.That(highlighted).IsEqualTo((Light.ContextMenu.Item.HoverBackground, (Color?)palette.TextPrimary));
        await Assert.That(selected).IsEqualTo((Color.Transparent, (Color?)palette.Accent));
        await Assert.That(coloredSelected).IsEqualTo((Color?)(Color)0x16A34AFFu);
        await Assert.That(coloredPlain).IsEqualTo((Color?)(Color)0x334155FFu);
        await Assert.That(row.Label.TextColor).IsEqualTo((Color?)palette.TextMuted);
    }

    [Test]
    public async Task ACustomTemplateReceivesTheSizingAndSizesRejectNonPositiveValues()
    {
        var sizings = new List<DropdownSizing?>();
        var template = new DropdownOptionRowTemplate(sizing =>
        {
            sizings.Add(sizing);
            return new PlainRow();
        });
        var sizing = new DropdownSizing { OptionFontSize = 18.0f };

        var row = template.CreatePresenter(sizing);

        await Assert.That(row is PlainRow).IsTrue();
        await Assert.That(ReferenceEquals(sizings[0], sizing)).IsTrue();
        await Assert.That(Rejects(() => new DropdownSizing { FieldHeight = 0.0f })).IsTrue();
        await Assert.That(Rejects(() => new DropdownSizing { ChevronIconSize = -1.0f })).IsTrue();
    }

    private static bool Rejects(Func<DropdownSizing> create)
    {
        try
        {
            create();
            return false;
        }
        catch (ArgumentOutOfRangeException)
        {
            return true;
        }
    }

    private sealed class PlainRow() : DropdownOptionRowPresenter(new FlexBox(), new Text(), new DropdownOptionRowMetrics(20.0f))
    {
        public override void Apply(Theme theme, DropdownOptionRowVisualState state, DropdownColors? colors)
        {
        }
    }
}
