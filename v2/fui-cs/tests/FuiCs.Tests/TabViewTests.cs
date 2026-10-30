using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using FuiCs.Hosting;

using TUnit.Assertions;
using TUnit.Core;

namespace FuiCs.Tests;

public sealed class TabViewTests
{
    private sealed class Counts
    {
        public int Created { get; set; }

        public int Activated { get; set; }

        public int Deactivated { get; set; }

        public Func<RetainedView> Factory() => () =>
        {
            ++Created;
            return new RetainedView(new FlexBox().FillSize())
                .OnActivate(_ => ++Activated)
                .OnDeactivate(_ => ++Deactivated);
        };
    }

    [Test]
    public async Task EachTabsContentIsCreatedOnceAndDeactivatedBeforeItIsDetached()
    {
        TestRuntime.Start();
        var first = new Counts();
        var second = new Counts();
        var tabs = new TabView(new TabItem("First", first.Factory()), new TabItem("Second", second.Factory()));
        tabs.Build();
        var initial = (tabs.SelectedIndex, first.Created, first.Activated, second.Created, tabs.Children.Count, tabs.ContentPresenter.Children.Count);

        tabs.SelectedIndex = 1;
        var afterSecond = (first.Deactivated, second.Created, tabs.ContentPresenter.Children.Count);
        tabs.SelectedIndex = 0;

        await Assert.That(initial).IsEqualTo((0, 1, 1, 0, 1, 1));
        await Assert.That(afterSecond).IsEqualTo((1, 1, 1));
        await Assert.That((first.Created, first.Activated)).IsEqualTo((1, 2));
        await Assert.That(tabs.Items[0].IsMaterialized && tabs.Items[1].IsMaterialized).IsTrue();
        tabs.Dispose();
    }

    [Test]
    public async Task OnlyTheSelectedTabsPanelIsShownWithNoTabStrip()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var tabs = new TabView(new TabItem("First"), new TabItem("Second"));
        tabs.Build();

        await Assert.That(tabs.Children.Count).IsEqualTo(1);
        await Assert.That(abi.HasCall("ui_set_semantic_role", (object)tabs.ContentPresenter.Handle, SemanticRole.TabPanel)).IsTrue();
        await Assert.That(abi.Named("ui_set_semantic_role").Exists(call => call.Arguments[1] is SemanticRole.Tab or SemanticRole.TabList)).IsFalse();
        await Assert.That(tabs.ContentPresenter.SemanticLabel).IsEqualTo("First tab panel");
        tabs.Items[0].Label = "Overview";
        await Assert.That(tabs.ContentPresenter.SemanticLabel).IsEqualTo("Overview tab panel");
        tabs.Dispose();
    }

    [Test]
    public async Task DisablingOrRemovingTheSelectedTabSelectsTheNearestEnabledOne()
    {
        TestRuntime.Start();
        var second = new TabItem("Second");
        var log = new List<string>();
        var tabs = new TabView(new TabItem("First"), second, new TabItem("Third"))
            .OnSelectionChanged(e => log.Add($"{e.PreviousIndex}>{e.SelectedIndex}"));
        tabs.Build();

        tabs.SelectedIndex = 1;
        second.IsEnabled = false;
        var afterDisable = tabs.SelectedIndex;
        tabs.RemoveItemAt(2);
        var afterRemove = (tabs.SelectedIndex, tabs.Items.Count);
        tabs.SelectedIndex = 1;
        var disabledIgnored = tabs.SelectedIndex;
        tabs.ClearItems();

        await Assert.That(afterDisable).IsEqualTo(2);
        await Assert.That(afterRemove).IsEqualTo((0, 2));
        await Assert.That(disabledIgnored).IsEqualTo(0);
        await Assert.That(string.Join(",", log)).IsEqualTo("0>1,1>2,2>0,0>-1");
        await Assert.That(tabs.SelectedItem).IsNull();
        tabs.Dispose();
    }

    [Test]
    public async Task ASelectionChangedFromTheHandlerTakesEffectAfterTheCurrentOne()
    {
        TestRuntime.Start();
        var changes = 0;
        TabView? tabs = null;
        tabs = new TabView(new TabItem("First"), new TabItem("Second"), new TabItem("Third"))
            .OnSelectionChanged(e =>
            {
                ++changes;
                if (e.SelectedIndex == 1)
                {
                    tabs!.SelectedIndex = 2;
                }
            });
        tabs.Build();

        tabs.SelectedIndex = 1;

        await Assert.That(tabs.SelectedIndex).IsEqualTo(2);
        await Assert.That(changes).IsEqualTo(2);
        tabs.Dispose();
    }

    [Test]
    public async Task MisuseThrowsAndRemovingATabDisposesItsContent()
    {
        TestRuntime.Start();
        var disposed = 0;
        var item = new TabItem("Only", () => new RetainedView(new FlexBox()).OnDispose(_ => ++disposed));
        var tabs = new TabView(item);
        tabs.Build();

        var outOfRange = Throws<ArgumentOutOfRangeException>(() => tabs.SelectedIndex = 1);
        var replaceContent = Throws<InvalidOperationException>(() => item.Content(() => new RetainedView(new FlexBox())));
        var removeOutOfRange = Throws<ArgumentOutOfRangeException>(() => tabs.RemoveItemAt(3));
        tabs.RemoveItem(item);

        await Assert.That(outOfRange && replaceContent && removeOutOfRange).IsTrue();
        await Assert.That(disposed).IsEqualTo(1);
        await Assert.That(tabs.SelectedIndex).IsEqualTo(-1);
        tabs.Dispose();
    }

    private static bool Throws<TException>(Action action)
        where TException : Exception
    {
        try
        {
            action();
            return false;
        }
        catch (TException)
        {
            return true;
        }
    }
}
