using Playdux.Store;
using Shared;

namespace Playdux.Tests.Store;

public class StoreNotificationTests
{
    [Fact]
    public void ObserverNotNotifiedOnDispatchWhenReducerDoesNotChangeState()
    {
        SimpleTestState init = new(42);
        var simpleStore = new Store<SimpleTestState>(init, TestReducers.IdentitySimpleTestStateReducer);

        var subscriber = new EventCounterSubscriber<SimpleTestState>();
        simpleStore.ObservableFor(state => state).Subscribe(subscriber);

        simpleStore.Dispatch(new EmptyAction());

        subscriber.Notified.Should().Be(0);
    }

    [Fact]
    public void ObserverNotifiedOnDispatchWhenReducerDoesChangeState()
    {
        SimpleTestState init = new(42);
        var simpleStore = new Store<SimpleTestState>(init, TestReducers.IncrementNSimpleTestStateReducer);

        var subscriber = new EventCounterSubscriber<SimpleTestState>();
        simpleStore.ObservableFor(state => state).Subscribe(subscriber);

        simpleStore.Dispatch(new EmptyAction());

        subscriber.Notified.Should().Be(1);
    }


    [Fact]
    public void ObserverNotNotifiedForChangeOutsideOfSelector()
    {
        Point init = new(4, 2);
        var pointStore = new Store<Point>(init, TestReducers.IncrementYPointReducer);

        var subscriber = new EventCounterSubscriber<float>();
        pointStore.ObservableFor(state => state.X).Subscribe(subscriber);

        pointStore.Dispatch(new EmptyAction());

        subscriber.Notified.Should().Be(0);
    }

    [Fact]
    public void ObserverNotifiedForChangeInsideOfSelector()
    {
        Point init = new(4, 2);
        var pointStore = new Store<Point>(init, TestReducers.IncrementYPointReducer);

        var subscriber = new EventCounterSubscriber<float>();
        pointStore.ObservableFor(state => state.Y).Subscribe(subscriber);

        pointStore.Dispatch(new EmptyAction());

        subscriber.Notified.Should().Be(1);
    }

    [Fact]
    public void ErrorInSelectorDoesNotBreakOtherObservers()
    {
        SimpleTestState init = new(42);
        var simpleStore = new Store<SimpleTestState>(init, TestReducers.IncrementNSimpleTestStateReducer);

        int notified = 0;
        int errors = 0;
        simpleStore.ObservableFor(state => state)
            .Subscribe(
                onNext: _ => notified++,
                onError: _ => errors++);
        simpleStore.ObservableFor(TestSelectors.ErrorSimpleTestStateSelector)
            .Subscribe(
                onNext: _ => { },
                onError: _ => { });

        simpleStore.Dispatch(new EmptyAction());
        simpleStore.Dispatch(new EmptyAction());

        (notified, errors).Should().Be((2, 0));
    }
}

internal class EventCounterSubscriber<T> : IObserver<T>
{
    public int Notified { get; private set; }
    public void OnNext(T value) => Notified++;
    public void OnCompleted() => throw new NotImplementedException();
    public void OnError(Exception error) => throw new NotImplementedException();
}