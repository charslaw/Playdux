using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Playdux.Actions;
using Playdux.DataStructures;

namespace Playdux.Tests.DataStructures;

public class ActionQueueTests
{
    private abstract record EmptyState;
    
    private record NumberAction(int Num) : IAction<EmptyState>;

    private record EmptyAction : IAction<EmptyState>;
    
    [Fact]
    public async Task Dispatch_ShouldExecuteAllDispatchedActionsInOrderOfReceipt()
    {
        var nums = new List<int>();
        var queue = new ActionQueue<EmptyState>(a => nums.Add((a.Action as NumberAction)!.Num));
        
        await Task.WhenAll(
            Task.Delay(2).ContinueWith(_ => queue.Dispatch(new DispatchedAction<EmptyState>(new NumberAction(2)))),
            Task.Delay(3).ContinueWith(_ => queue.Dispatch(new DispatchedAction<EmptyState>(new NumberAction(3)))),
            Task.Delay(1).ContinueWith(_ => queue.Dispatch(new DispatchedAction<EmptyState>(new NumberAction(1))))
        );

        nums.Should().BeEquivalentTo([1, 2, 3]);
    }

    [Fact]
    public async Task Dispatch_ShouldExecuteActionsDispatchedAtSameTimeOnSameThread()
    {
        var nums = new List<int>();
        var queue = new ActionQueue<EmptyState>(_ => nums.Add(Environment.CurrentManagedThreadId));
        
        // When multiple actions are dispatched at the same time, they should be handled on the same thread
        // We don't want two separate threads attempting to read from the queue at the same time and modifying the
        // state at the same time.
        await Task.WhenAll(
            Task.Delay(0).ContinueWith(_ => queue.Dispatch(new DispatchedAction<EmptyState>(new EmptyAction()))),
            Task.Delay(0).ContinueWith(_ => queue.Dispatch(new DispatchedAction<EmptyState>(new EmptyAction())))
        );

        nums.Distinct().Count().Should().Be(1);
    }
}