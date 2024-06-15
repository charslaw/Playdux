using Playdux.Actions;
using Playdux.Store;

namespace Playdux.Tests.Store;

public class DispatchTests
{
    [Fact]
    public void Dispatch_ShouldModifyStateWithRootReducer()
    {
        var initialState = new BasicState(10);
        var store = new Store<BasicState>(initialState, (_, _) => new BasicState(20));

        store.Dispatch(new EmptyAction());

        store.State.Should().BeEquivalentTo(new BasicState(20));
    }

    [Fact]
    public void Dispatch_ShouldPassDispatchedActionToReducer()
    {
        var initialState = new BasicState(10);
        var store = new Store<BasicState>(initialState,
            (state, action) => action is NAction a
                ? new BasicState(a.N)
                : state
        );

        store.Dispatch(new NAction(20));

        store.State.Should().BeEquivalentTo(new BasicState(20));
    }
}

file record BasicState(int N);

file record EmptyAction : IAction<BasicState>;

file record NAction(int N) : IAction<BasicState>;