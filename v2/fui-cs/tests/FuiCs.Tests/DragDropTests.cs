using System.Collections.Generic;
using System.Threading.Tasks;

using TUnit.Assertions;
using TUnit.Core;

namespace FuiCs.Tests;

public sealed class DragDropTests
{
    private static (TestNode Root, TestNode Source, TestNode Target) BuildScene()
    {
        var root = new TestNode();
        var source = new TestNode
        {
            DragAllowedEffects = DragDropEffects.Copy | DragDropEffects.Move,
            DragDataProvider = () => new DragDataObject().SetText("payload"),
        };
        var target = new TestNode();
        root.AddChild(source);
        root.AddChild(target);
        root.Build();
        return (root, source, target);
    }

    [Test]
    public async Task DraggingASourceOntoATargetDropsTheDataAndReportsTheEffect()
    {
        var abi = TestRuntime.Start();
        var (_, source, target) = BuildScene();
        var log = new List<string>();
        target.DragEnter += _ => log.Add("enter");
        target.DragOver += e => e.Proposal = new DropProposal(DragDropEffects.Move);
        target.Drop += e => log.Add("drop " + e.Session.Data.GetText());
        target.DragLeave += _ => log.Add("leave");
        source.DragCompleted += e => log.Add("completed " + e.Effect);

        HostInput.Pointer(PointerEventType.Down, source.Handle, 0.0f, 0.0f);
        HostInput.Pointer(PointerEventType.Move, target.Handle, 10.0f, 0.0f);
        HostInput.Pointer(PointerEventType.Up, target.Handle, 10.0f, 0.0f);

        await Assert.That(string.Join("|", log)).IsEqualTo("enter|drop payload|leave|completed Move");
        await Assert.That(abi.Arguments("fui_set_cursor", 0)).IsEqualTo("Move,Default");
    }

    [Test]
    public async Task ARefusedDropLeavesTheTargetWithoutDropping()
    {
        TestRuntime.Start();
        var (_, source, target) = BuildScene();
        var log = new List<string>();
        target.DragEnter += _ => log.Add("enter");
        target.DragOver += e => e.Proposal = DropProposal.None;
        target.Drop += _ => log.Add("drop");
        target.DragLeave += _ => log.Add("leave");
        source.DragCompleted += e => log.Add("completed " + e.Effect);

        HostInput.Pointer(PointerEventType.Down, source.Handle, 0.0f, 0.0f);
        HostInput.Pointer(PointerEventType.Move, target.Handle, 10.0f, 0.0f);
        HostInput.Pointer(PointerEventType.Up, target.Handle, 10.0f, 0.0f);

        await Assert.That(string.Join("|", log)).IsEqualTo("enter|leave|completed None");
    }

    [Test]
    public async Task APressWithoutMovementStillClicksTheSource()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var (_, source, _) = BuildScene();
        abi.Bounds[source.Handle] = new Rect(0.0f, 0.0f, 40.0f, 20.0f);
        var clicks = 0;
        var completed = 0;
        source.PointerClick += _ => clicks++;
        source.DragCompleted += _ => completed++;

        HostInput.Click(source.Handle, 10.0f, 10.0f);

        await Assert.That(clicks).IsEqualTo(1);
        await Assert.That(completed).IsEqualTo(0);
    }

    [Test]
    public async Task ATargetThatProposesNothingReceivesNoDrop()
    {
        TestRuntime.Start();
        var (_, source, target) = BuildScene();
        target.AllowDrop = true;
        var drops = 0;
        var effects = new List<DragDropEffects>();
        target.Drop += _ => drops++;
        source.DragCompleted += e => effects.Add(e.Effect);

        HostInput.Pointer(PointerEventType.Down, source.Handle, 0.0f, 0.0f);
        HostInput.Pointer(PointerEventType.Move, target.Handle, 10.0f, 0.0f);
        HostInput.Pointer(PointerEventType.Up, target.Handle, 10.0f, 0.0f);

        await Assert.That(drops).IsEqualTo(0);
        await Assert.That(string.Join(",", effects)).IsEqualTo("None");
    }

    [Test]
    public async Task DisposingTheSourceEndsItsDrag()
    {
        TestRuntime.Start();
        var (_, source, target) = BuildScene();
        var effects = new List<DragDropEffects>();
        target.DragOver += e => e.Proposal = new DropProposal(DragDropEffects.Copy);
        source.DragCompleted += e => effects.Add(e.Effect);

        HostInput.Pointer(PointerEventType.Down, source.Handle, 0.0f, 0.0f);
        HostInput.Pointer(PointerEventType.Move, target.Handle, 10.0f, 0.0f);
        source.Dispose();
        HostInput.Pointer(PointerEventType.Up, target.Handle, 10.0f, 0.0f);

        await Assert.That(string.Join(",", effects)).IsEqualTo("None");
    }
}
