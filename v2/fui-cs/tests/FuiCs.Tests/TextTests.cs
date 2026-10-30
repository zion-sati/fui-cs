using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using FuiCs.Hosting;

using TUnit.Assertions;
using TUnit.Core;

namespace FuiCs.Tests;

public sealed class TextTests
{
    private static readonly string[] TextCalls =
    [
        "ui_create_node", "ui_set_text", "ui_set_text_style_runs", "ui_set_font", "ui_set_line_height", "ui_set_text_color",
        "ui_set_text_align", "ui_set_text_limits", "ui_set_text_wrapping", "ui_set_text_overflow", "ui_set_editable",
        "ui_set_selectable",
    ];

    [Test]
    public async Task BuildSendsContentFontAndThemeDefaults()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var text = new Text("hello").FontSize(18.0f).TextAlign(TextAlign.Center).MaxLines(2);

        var handle = text.Build();

        await Assert.That(abi.Sequence(TextCalls)).IsEqualTo(
            "ui_create_node,ui_set_text,ui_set_font,ui_set_text_color,ui_set_text_align,ui_set_text_limits,ui_set_selectable");
        await Assert.That(abi.HasCall("ui_set_text", handle, "hello")).IsTrue();
        await Assert.That(abi.HasCall("ui_set_font", handle, 1u, 18.0f)).IsTrue();
        await Assert.That(abi.HasCall("ui_set_text_color", handle, Theme.DefaultDark.Colors.TextPrimary.Rgba)).IsTrue();
        await Assert.That(abi.HasCall("ui_set_selectable", handle, true, Theme.DefaultDark.Colors.Selection.Rgba)).IsTrue();
        await Assert.That(text.Cursor).IsEqualTo(CursorStyle.Text);
    }

    [Test]
    public async Task AnUnsizedFontLeavesTheHostDefault()
    {
        var abi = TestRuntime.Start(new LayoutAbi());

        new TextCore("plain").Build();

        await Assert.That(abi.CountOf("ui_set_font")).IsEqualTo(0);
        await Assert.That(abi.CountOf("ui_set_selectable")).IsEqualTo(0);
    }

    [Test]
    public async Task WeightAndStyleResolveTheClosestFaceOfTheFamily()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var bold = new Text("b").FontSize(16.0f).FontWeight(FontWeight.Bold);
        var italic = new Text("i").FontSize(16.0f).FontStyle(FontStyle.Italic);
        var boldItalic = new Text("bi").FontSize(16.0f).FontWeight(FontWeight.Semibold).FontStyle(FontStyle.Italic);

        bold.Build();
        italic.Build();
        boldItalic.Build();

        await Assert.That(abi.HasCall("ui_set_font", bold.Handle, 2u)).IsTrue();
        await Assert.That(abi.HasCall("ui_set_font", italic.Handle, 5u)).IsTrue();
        await Assert.That(abi.HasCall("ui_set_font", boldItalic.Handle, 6u)).IsTrue();
    }

    [Test]
    public async Task PropertiesUpdateTheBuiltNodeWithOneCommit()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var text = new Text("a");
        var handle = text.Build();
        abi.Calls.Clear();

        text.Text("b").TextColor(0x112233FFu).FontSize(20.0f).Wrapping(false);

        await Assert.That(abi.HasCall("ui_set_text", handle, "b")).IsTrue();
        await Assert.That(abi.HasCall("ui_set_text_color", handle, 0x112233FFu)).IsTrue();
        await Assert.That(abi.HasCall("ui_set_font", handle, 1u, 20.0f)).IsTrue();
        await Assert.That(abi.HasCall("ui_set_text_wrapping", handle, false)).IsTrue();
        await Assert.That(abi.CountOf("request_render")).IsEqualTo(1);
        await Assert.That(abi.CountOf("ui_create_node")).IsEqualTo(0);
    }

    [Test]
    public async Task ThemeChangesRecolorTextWithoutAnExplicitColor()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var themed = new Text("themed");
        var explicitColor = new Text("fixed").TextColor(0xFF0000FFu);
        themed.Build();
        explicitColor.Build();
        abi.Calls.Clear();

        Theme.UseCustom(Theme.DefaultLight);

        await Assert.That(abi.HasCall("ui_set_text_color", themed.Handle, Theme.DefaultLight.Colors.TextPrimary.Rgba)).IsTrue();
        await Assert.That(abi.HasCall("ui_set_text_color", explicitColor.Handle)).IsFalse();
        await Assert.That(Theme.IsDarkMode).IsFalse();
        await Assert.That(Theme.IsUsingSystemTheme).IsFalse();
    }

    [Test]
    public async Task TextIsStaticTextLabelledByItsContentUnlessAnAncestorHasARole()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var standalone = new Text("Hello");
        var inButton = new Text("Inside");
        var button = new FlexBox().SemanticRole(SemanticRole.Button).Child(inButton);

        standalone.Build();
        button.Build();

        await Assert.That(abi.HasCall("ui_set_semantic_role", standalone.Handle, SemanticRole.StaticText)).IsTrue();
        await Assert.That(abi.HasCall("ui_set_semantic_label", standalone.Handle, "Hello")).IsTrue();
        await Assert.That(abi.HasCall("ui_set_semantic_role", inButton.Handle)).IsFalse();

        abi.Calls.Clear();
        standalone.Text = "Changed";

        await Assert.That(abi.HasCall("ui_set_semantic_label", standalone.Handle, "Changed")).IsTrue();
    }

    [Test]
    public async Task EditableTextIsSelectableUnlessSelectionWasSet()
    {
        TestRuntime.Start(new LayoutAbi());
        var editor = new TextCore("e").Editable();
        var readOnly = new TextCore("r").Selectable(false).Editable();

        await Assert.That(editor.Selectable).IsTrue();
        await Assert.That(editor.IsEditableText).IsTrue();
        await Assert.That(readOnly.Selectable).IsFalse();
        await Assert.That(readOnly.Cursor).IsEqualTo(CursorStyle.Default);
    }

    [Test]
    public async Task HostEditsUseUtf8OffsetsAndRaiseTextChanged()
    {
        TestRuntime.Start(new LayoutAbi());
        var changes = new List<string>();
        var text = new Text("héllo wörld").OnTextChanged(e => changes.Add(e.Text));
        var handle = text.Build();

        HostEvents.OnTextReplaced(handle, 1, 3, "e");
        HostEvents.OnTextChanged(handle, "whole");

        await Assert.That(string.Join("|", changes)).IsEqualTo("hello wörld|whole");
        await Assert.That(text.Text).IsEqualTo("whole");
    }

    [Test]
    public async Task SelectionChangesAreTrackedAndRaised()
    {
        TestRuntime.Start(new LayoutAbi());
        var ranges = new List<string>();
        var text = new Text("select me").OnSelectionChanged(e => ranges.Add(e.Start + "-" + e.End));
        var handle = text.Build();

        HostEvents.OnSelectionChanged(handle, 2, 5);

        await Assert.That(text.SelectionStart).IsEqualTo(2u);
        await Assert.That(text.SelectionEnd).IsEqualTo(5u);
        await Assert.That(string.Join(",", ranges)).IsEqualTo("2-5");
    }

    [Test]
    public async Task RichTextSendsMergedTextWithStyleRuns()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var rich = new RichText(new Span("ab").Bold(), new Span("ç").Color(0xFF0000FFu).Underline());

        var handle = rich.Build();

        await Assert.That(abi.HasCall("ui_set_text", handle, "abç")).IsTrue();
        var runs = abi.Named("ui_set_text_style_runs")[0].Arguments;
        var size = BitConverter.SingleToUInt32Bits(16.0f);
        var primary = Theme.DefaultDark.Colors.TextPrimary.Rgba;
        await Assert.That(runs[1]).IsEqualTo((object)2u);
        await Assert.That(string.Join(",", (uint[])runs[2]!)).IsEqualTo(string.Join(",",
            0u, 2u, 2u, size, primary, 0u, 0u,
            2u, 4u, 1u, size, 0xFF0000FFu, 0u, 1u));
    }

    [Test]
    public async Task RichTextRunsFollowThemeChangesForSpansWithoutTheirOwnColor()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var rich = new RichText(new Span("plain"), new Span("red").Color(0xFF0000FFu));
        var handle = rich.Build();
        abi.Calls.Clear();

        Theme.UseCustom(Theme.DefaultLight);

        var runs = (uint[])abi.Named("ui_set_text_style_runs")[^1].Arguments[2]!;
        await Assert.That(abi.Named("ui_set_text_style_runs")[^1].Arguments[0]).IsEqualTo((object)handle);
        await Assert.That(runs[4]).IsEqualTo(Theme.DefaultLight.Colors.TextPrimary.Rgba);
        await Assert.That(runs[11]).IsEqualTo(0xFF0000FFu);
    }

    [Test]
    public async Task ARichTextThemeChangeResendsOnlyWhatTheThemeChanged()
    {
        // Resending unchanged text and runs would discard the shaping a detached node was prepared with for a bitmap
        // render. These spans have their own colors, so the new theme changes only the base color and selection color.
        var abi = TestRuntime.Start(new LayoutAbi());
        var rich = new RichText(
                new Span("Draw ").Color(0xEBEEF5D2u).Italic(),
                new Span("here").FontFamily(Theme.Active.Value.Fonts.MonoFamily).Bold().Color(0x3AC56CFFu))
            .FontFamily(Theme.Active.Value.Fonts.BodyFamily)
            .FontSize(24.0f);
        var handle = rich.Build();
        abi.Calls.Clear();

        Theme.UseCustom(Theme.DefaultLight);

        var sent = abi.Calls.FindAll(call => call.Arguments.Length > 0 && Equals(call.Arguments[0], handle)).ConvertAll(call => call.Name);
        await Assert.That(string.Join(",", sent)).IsEqualTo("ui_set_selectable,ui_set_text_color");
        await Assert.That(abi.HasCall("ui_set_text_color", handle, Theme.DefaultLight.Colors.TextPrimary.Rgba)).IsTrue();
    }

    [Test]
    public async Task FontsLoadOnceAndWaitersRunWhenAllAreReady()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var log = new List<string>();
        var face = FontFace.Load("https://fonts.example/a.woff2");
        var other = FontFace.Load("https://fonts.example/b.woff2");
        face.OnLoaded(e => log.Add("face " + e.Font.Id));
        FontFace.WhenFontsLoaded([face.Id, other.Id], e => log.Add("both " + e.FontIds.Count));
        FontFace.WhenLoaded(1, _ => log.Add("built-in"));

        HostEvents.OnFontLoaded(face.Id);
        HostEvents.OnFontLoaded(face.Id);
        var afterFirst = string.Join("|", log);
        HostEvents.OnFontLoaded(other.Id);

        await Assert.That(face.Id).IsEqualTo(1024u);
        await Assert.That(abi.HasCall("fui_load_font", 1024u, "https://fonts.example/a.woff2")).IsTrue();
        await Assert.That(afterFirst).IsEqualTo("built-in|face 1024");
        await Assert.That(string.Join("|", log)).IsEqualTo("built-in|face 1024|both 2");
        await Assert.That(face.IsLoaded).IsTrue();
    }

    [Test]
    public async Task StacksRegisterFallbacksAndReportTheirFonts()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var stack = FontStack.Load("https://fonts.example/main.woff2").Fallback(new FontFace(3)).Fallback(new FontFace(3));

        await Assert.That(abi.CountOf("ui_register_font_fallback")).IsEqualTo(1);
        await Assert.That(string.Join(",", stack.RequiredFontIds())).IsEqualTo(stack.Id + ",3");
        await Assert.That(stack.IsLoaded).IsFalse();
    }

    [Test]
    public async Task GeneratedThemesMatchTheirLightAndAccentInputs()
    {
        TestRuntime.Start(new LayoutAbi());
        var dark = Theme.Generate(isDark: true);
        var light = Theme.Generate(isDark: false, 0xFFCC00FFu);

        await Assert.That(dark.Colors.Background.Rgba).IsEqualTo(0x040A14FFu);
        await Assert.That(dark.Colors.Accent.Rgba).IsEqualTo(0x2563EBFFu);
        await Assert.That(dark.Colors.TextOnAccent.Rgba).IsEqualTo(0xFFFFFFFFu);
        await Assert.That(dark.Colors.Selection.Rgba).IsEqualTo(0x2563EB40u);
        await Assert.That(light.Colors.TextOnAccent.Rgba).IsEqualTo(0x000000FFu);
        await Assert.That(light.Colors.Selection.Rgba).IsEqualTo(0xFFCC0033u);
        await Assert.That(dark.Colors.Accent == Theme.Generate(isDark: true).Colors.Accent).IsTrue();
        await Assert.That(dark == Theme.Generate(isDark: true)).IsFalse();
    }

    [Test]
    public async Task TheSystemThemeFollowsTheHostOnlyWhileInUse()
    {
        TestRuntime.Start(new LayoutAbi());

        var system = Theme.UseSystem();
        var followed = ApplyHostDarkMode(true);
        Theme.UseCustom(Theme.DefaultLight);
        var ignored = ApplyHostDarkMode(true);

        await Assert.That(system.Colors.Background.Rgba).IsEqualTo(Theme.DefaultLight.Colors.Background.Rgba);
        await Assert.That(followed.Colors.Background.Rgba).IsEqualTo(Theme.DefaultDark.Colors.Background.Rgba);
        await Assert.That(ReferenceEquals(ignored, Theme.Active.Value)).IsTrue();
        await Assert.That(Theme.Active.Value.Colors.Background.Rgba).IsEqualTo(Theme.DefaultLight.Colors.Background.Rgba);
    }

    [Test]
    public async Task TheSystemThemeFollowsTheHostAccentColorUntilACustomThemeIsUsed()
    {
        TestRuntime.Start(new LayoutAbi());
        var green = new Color(0x16A34AFF);
        var red = new Color(0xDC2626FF);

        HostEvents.OnSystemAccentColorChanged(green.Rgba);
        var followed = Theme.Active.Value.Colors.Accent;
        Theme.UseCustom(Theme.DefaultLight);
        HostEvents.OnSystemAccentColorChanged(red.Rgba);

        await Assert.That(followed).IsEqualTo(Theme.Generate(isDark: true, green).Colors.Accent);
        await Assert.That(Theme.Active.Value.Colors.Accent).IsEqualTo(Theme.DefaultLight.Colors.Accent);
    }

    private static Theme ApplyHostDarkMode(bool isDark)
    {
        HostEvents.OnSystemDarkModeChanged(isDark);
        return Theme.Active.Value;
    }
}
