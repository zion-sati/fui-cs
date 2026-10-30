using System.Threading.Tasks;

using TUnit.Assertions;
using TUnit.Core;

namespace FuiCs.Tests;

public sealed class NodeTreeTests
{
    [Test]
    public async Task ConstructionAndConfigurationDoNoNativeWork()
    {
        var abi = TestRuntime.Start();

        var root = new TestNode { NodeId = "root", SemanticLabel = "Root", IsEnabled = false, Cursor = CursorStyle.Pointer };
        root.AddChild(new TestNode { Visibility = Visibility.Hidden });
        root.MakeInteractive();

        await Assert.That(abi.Calls.Count).IsEqualTo(0);
        await Assert.That(root.IsBuilt).IsFalse();
    }

    [Test]
    public async Task BuildCreatesTheParentBeforeItsChildrenAndAttachesThemInOrder()
    {
        var abi = TestRuntime.Start();
        var root = new TestNode();
        var first = new TestNode();
        var second = new TestNode();
        root.AddChild(first);
        root.AddChild(second);

        var handle = root.Build();

        await Assert.That(handle).IsEqualTo(1UL);
        await Assert.That(abi.Sequence("ui_create_node", "ui_node_add_child"))
            .IsEqualTo("ui_create_node,ui_create_node,ui_node_add_child,ui_create_node,ui_node_add_child");
        await Assert.That(abi.Arguments("ui_node_add_child", 1)).IsEqualTo("2,3");
        await Assert.That(root.Build()).IsEqualTo(1UL);
        await Assert.That(abi.CountOf("ui_create_node")).IsEqualTo(3);
    }

    [Test]
    public async Task AddingAfterBuildBuildsTheChildAndRequestsRender()
    {
        var abi = TestRuntime.Start();
        var root = new TestNode();
        root.Build();
        abi.Calls.Clear();

        var child = new TestNode();
        root.AddChild(child);

        await Assert.That(child.IsBuilt).IsTrue();
        await Assert.That(abi.Sequence("ui_create_node", "ui_node_add_child", "request_render"))
            .IsEqualTo("ui_create_node,ui_node_add_child,request_render");
    }

    [Test]
    public async Task RemovingAfterBuildDetachesWithoutDeletingSoTheChildCanReturn()
    {
        var abi = TestRuntime.Start();
        var root = new TestNode();
        var child = new TestNode();
        root.AddChild(child);
        root.Build();
        abi.Calls.Clear();

        root.RemoveChild(child);

        await Assert.That(abi.CountOf("ui_node_remove_child")).IsEqualTo(1);
        await Assert.That(abi.CountOf("ui_delete_node")).IsEqualTo(0);
        await Assert.That(child.IsBuilt).IsTrue();
        await Assert.That(child.Parent).IsNull();
        await Assert.That(root.Children.Count).IsEqualTo(0);

        root.AddChild(child);

        await Assert.That(abi.CountOf("ui_create_node")).IsEqualTo(0);
        await Assert.That(abi.Arguments("ui_node_add_child", 1)).IsEqualTo("2");
    }

    [Test]
    public async Task AddingANodeToAnotherParentMovesIt()
    {
        var abi = TestRuntime.Start();
        var left = new TestNode();
        var right = new TestNode();
        var child = new TestNode();
        left.AddChild(child);
        left.Build();
        right.Build();
        abi.Calls.Clear();

        right.AddChild(child);

        await Assert.That(abi.Sequence("ui_node_remove_child", "ui_node_add_child")).IsEqualTo("ui_node_remove_child,ui_node_add_child");
        await Assert.That(left.Children.Count).IsEqualTo(0);
        await Assert.That(ReferenceEquals(right.Children[0], child)).IsTrue();
        await Assert.That(ReferenceEquals(child.Parent, right)).IsTrue();
        await Assert.That(child.ParentChanges).IsEqualTo(3);
    }

    [Test]
    public async Task AddingItselfOrAnExistingChildDoesNothing()
    {
        TestRuntime.Start();
        var root = new TestNode();
        var child = new TestNode();
        root.AddChild(child);

        root.AddChild(root);
        root.AddChild(child);

        await Assert.That(root.Children.Count).IsEqualTo(1);
        await Assert.That(root.Parent).IsNull();
    }

    [Test]
    public async Task MutationAfterBuildUpdatesNativeStateAndSchedulesOneCommit()
    {
        var abi = TestRuntime.Start();
        var root = new TestNode();
        root.Build();
        abi.Calls.Clear();

        root.NodeId = "main";
        root.SemanticLabel = "Main";

        await Assert.That(abi.Sequence("ui_set_node_id", "ui_set_semantic_label", "request_render"))
            .IsEqualTo("ui_set_node_id,request_render,ui_set_semantic_label");
        await Assert.That(FrameScheduler.FlushCommit()).IsTrue();
        await Assert.That(abi.CountOf("ui_commit_frame")).IsEqualTo(1);
        await Assert.That(FrameScheduler.FlushCommit()).IsFalse();
    }

    [Test]
    public async Task DisposeDeletesEachNativeNodeOnceChildrenFirst()
    {
        var abi = TestRuntime.Start();
        var root = new TestNode();
        var branch = new TestNode();
        var leaf = new TestNode();
        var sibling = new TestNode();
        branch.AddChild(leaf);
        root.AddChild(branch);
        root.AddChild(sibling);
        root.Build();
        abi.Calls.Clear();

        root.Dispose();
        root.Dispose();

        await Assert.That(abi.Arguments("ui_delete_node", 0)).IsEqualTo("4,3,2,1");
        await Assert.That(root.IsDisposed).IsTrue();
        await Assert.That(leaf.IsDisposed).IsTrue();
        await Assert.That(root.IsBuilt).IsFalse();
        await Assert.That(root.Children.Count).IsEqualTo(0);
    }

    [Test]
    public async Task TheRouterResolvesLiveHandlesOnly()
    {
        TestRuntime.Start();
        var root = new TestNode();
        var handle = root.Build();

        await Assert.That(ReferenceEquals(EventRouter.GetRegisteredNode(handle), root)).IsTrue();
        await Assert.That(EventRouter.GetRegisteredNode(handle | (1UL << 32))).IsNull();

        root.Dispose();

        await Assert.That(EventRouter.GetRegisteredNode(handle)).IsNull();
    }
}
