using System.Collections.Generic;
using FluentAssertions.Execution;
using Playdux.Actions;
using Playdux.SideEffectors;
using Playdux.Store;

namespace Playdux.Tests.Store;

public class SideEffectorTests
{
    [Fact]
    public void Dispatch_ShouldCallPreAndPostEffectorWithDispatchedAction()
    {
        var initialState = new BasicState(default);
        var sideEffector = new SpySideEffector();
        var store = new Store<BasicState>(initialState, (state, _) => state);
        store.RegisterSideEffector(sideEffector);

        store.Dispatch(new NAction(1));

        using (new AssertionScope())
        {
            sideEffector.PreEffectCalls.Should().BeEquivalentTo([new NAction(1)]);
            sideEffector.PostEffectCalls.Should().BeEquivalentTo([new NAction(1)]);
        }
    }

    [Fact]
    public void Dispatch_ShouldPreventStateChange_WhenPreEffectReturnsFalse()
    {
        var initialState = new BasicState(1);
        var sideEffector = new SpySideEffector(false);
        var store = new Store<BasicState>(initialState, (_, _) => new BasicState(2));
        store.RegisterSideEffector(sideEffector);

        store.Dispatch(new EmptyAction());

        store.State.Should().BeEquivalentTo(new BasicState(1));
    }

    [Fact]
    public void Dispatch_ShouldPreventPostEffectExecution_WhenPreEffectReturnsFalse()
    {
        var initialState = new BasicState(1);
        var sideEffector = new SpySideEffector(false);
        var store = new Store<BasicState>(initialState, (state, _) => state);
        store.RegisterSideEffector(sideEffector);

        store.Dispatch(new EmptyAction());

        sideEffector.PostEffectCalls.Should().BeEmpty();
    }
}

file record BasicState(int N);
file record EmptyAction : IAction<BasicState>;
file record NAction(int N) : IAction<BasicState>;

file class SpySideEffector(bool preEffectReturn = true) : ISideEffector<BasicState>
{
    public List<IAction<BasicState>> PreEffectCalls { get; } = [];
    public List<IAction<BasicState>> PostEffectCalls { get; } = [];
        
    public bool PreEffect(DispatchedAction<BasicState> dispatchedAction, IStore<BasicState> store)
    {
        PreEffectCalls.Add(dispatchedAction.Action);
        return preEffectReturn;
    }

    public void PostEffect(DispatchedAction<BasicState> dispatchedAction, IStore<BasicState> store)
    {
        PostEffectCalls.Add(dispatchedAction.Action);
    }
}
