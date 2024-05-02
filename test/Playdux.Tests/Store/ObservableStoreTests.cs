using Playdux.Store;
using Shared;

namespace Playdux.Tests.Store;

public class ObservableStoreTests
{
    [Fact]
    public void CanUnsubscribeWithoutBreakingOtherSubscribers()
    {
        Point init = new(4, 2);
        var pointStore = new Store<Point>(init, TestReducers.IncrementYPointReducer);

        var notified = 0;
        var disposable = pointStore.ObservableFor(state => state.Y).Subscribe(_ => { });
        pointStore.ObservableFor(state => state.Y)
            .Subscribe(
                onNext: _ => notified++,
                onError: e => { Console.Error.WriteLine(e); });
        disposable.Dispose();

        pointStore.Dispatch(new EmptyAction());

        notified.Should().Be(1);
    }

    [Fact]
    public void CanUnsubscribeWithoutBreakingEverything()
    {
        Point init = new(4, 2);
        var pointStore = new Store<Point>(init, TestReducers.IncrementYPointReducer);

        var disposable = pointStore.ObservableFor(state => state.Y).Subscribe(_ => { });
        disposable.Dispose();

        pointStore.Invoking(store => store.Dispatch(new EmptyAction()))
            .Should().NotThrow();
    }
}