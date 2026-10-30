using System.Collections.Generic;
using System.Threading.Tasks;

using FuiCs.Hosting;

using TUnit.Assertions;
using TUnit.Core;

namespace FuiCs.Tests;

public sealed class PersistenceTests
{
    private static PersistedValue<Text, string> TextState() =>
        new("text", text => text.Text, (text, value) => text.Text = value, PersistedCodecs.Text);

    [Test]
    public async Task APageCapturesAndRestoresItsPersistedValues()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var name = new Text("Ada").NodeId("name").PersistState(TextState());
        Application.Mount(FlexBox.Column(name));

        HostEvents.CapturePersistedUiState();
        name.Text = "changed";
        HostEvents.RestorePersistedUiState();

        await Assert.That(abi.PersistedText[("name", "text")]).IsEqualTo((1u, "Ada"));
        await Assert.That(name.Text).IsEqualTo("Ada");
    }

    [Test]
    public async Task NodesWithoutAnIdOrDecliningToCaptureSaveNothing()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var anonymous = new Text("a").PersistState(TextState());
        var declining = new Text("b").NodeId("declining").PersistState(
            new PersistedValue<Text, string>("text", text => text.Text, (text, value) => text.Text = value, PersistedCodecs.Text)
            {
                ShouldCapture = _ => false,
            });
        Application.Mount(FlexBox.Column(anonymous, declining));

        HostEvents.CapturePersistedUiState();

        await Assert.That(abi.PersistedText.Count).IsEqualTo(0);
    }

    [Test]
    public async Task AnAdapterOfTheSameKindReplacesTheEarlierOne()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var count = new Text("3").NodeId("count")
            .PersistState(TextState())
            .PersistState(new PersistedValue<Text, int>("text", text => int.Parse(text.Text) * 2, (text, value) => text.Text = value.ToString(),
                PersistedCodecs.Int32, version: 2));
        Application.Mount(FlexBox.Column(count));

        HostEvents.CapturePersistedUiState();

        await Assert.That(abi.PersistedText[("count", "text")]).IsEqualTo((2u, "6"));
    }

    [Test]
    public async Task CodecsRoundTripInTheInvariantCulture()
    {
        TestRuntime.Start(new LayoutAbi());

        await Assert.That(PersistedCodecs.Single.Decode(PersistedCodecs.Single.Encode(1.25f), 1)).IsEqualTo(1.25f);
        await Assert.That(PersistedCodecs.Single.Encode(1.25f)).IsEqualTo("1.25");
        await Assert.That(PersistedCodecs.Int32.Decode("-42", 1)).IsEqualTo(-42);
        await Assert.That(PersistedCodecs.Boolean.Decode(PersistedCodecs.Boolean.Encode(true), 1)).IsTrue();
    }

    [Test]
    public async Task ChildrenRestoreBeforeTheirParents()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var order = new List<string>();
        PersistedValue<FlexBox, string> Logged(string id) =>
            new("order", _ => id, (_, value) => order.Add(value), PersistedCodecs.Text);
        var child = new FlexBox().NodeId("child").PersistState(Logged("child"));
        var parent = new FlexBox().NodeId("parent").PersistState(Logged("parent")).Child(child);
        Application.Mount(parent);
        HostEvents.CapturePersistedUiState();

        HostEvents.RestorePersistedUiState();

        await Assert.That(string.Join(",", order)).IsEqualTo("child,parent");
        await Assert.That(abi.PersistedText.Count).IsEqualTo(2);
    }

    [Test]
    public async Task AScrollViewRestoresItsOffsetOnceTheHostReportsEnoughContent()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var scroll = new ScrollView().NodeId("list").HorizontalScrollEnabled(false);
        Application.Mount(scroll);
        abi.PersistedScroll["list"] = (0.0f, 900.0f);
        abi.Calls.Clear();

        HostEvents.RestorePersistedUiState();
        var restoredBeforeMetrics = abi.CountOf("ui_set_scroll_offset");
        HostEvents.OnScroll(scroll.Handle, 0.0f, 0.0f, 200.0f, 1000.0f, 200.0f, 200.0f);

        await Assert.That(restoredBeforeMetrics).IsEqualTo(0);
        await Assert.That(abi.HasCall("ui_set_scroll_offset", scroll.Handle, 0.0f, 800.0f)).IsTrue();
        await Assert.That(abi.CountOf("ui_clear_momentum_scroll")).IsEqualTo(0);
        await Assert.That(scroll.VerticalOffset).IsEqualTo(800.0f);
    }

    [Test]
    public async Task AScrollViewSavesItsOffsetUnlessPersistenceIsOff()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var saved = new ScrollView().NodeId("saved").ScrollOffset(0.0f, 40.0f);
        var unsaved = new ScrollView().NodeId("unsaved").ScrollOffset(0.0f, 60.0f).PersistScroll(false);
        Application.Mount(FlexBox.Column(saved, unsaved));

        HostEvents.CapturePersistedUiState();

        await Assert.That(abi.PersistedScroll["saved"]).IsEqualTo((0.0f, 40.0f));
        await Assert.That(abi.PersistedScroll.ContainsKey("unsaved")).IsFalse();
    }
}
