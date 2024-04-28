#nullable enable
using System;
using FluentAssertions;
using Playdux.src.Store;

namespace Playdux.test
{
    public class StoreTests
    {

        [Fact]
        public void GetStateOnNewStoreReturnsInitialState()
        {
            SimpleTestState init = new(42);
            var simpleStore = new Store<SimpleTestState>(init, TestReducers.IdentitySimpleTestStateReducer);

            simpleStore.State.Should().BeEquivalentTo(init);
        }

        [Fact]
        public void StateNotChangedAfterDispatchWhenReducerDoesNotChangeState()
        {
            SimpleTestState init = new(42);
            var simpleStore = new Store<SimpleTestState>(init, TestReducers.IdentitySimpleTestStateReducer);

            simpleStore.Dispatch(new EmptyAction());

            simpleStore.State.Should().BeEquivalentTo(init);
        }

        [Fact]
        public void StateChangedAfterDispatchWhenReducerDoesChangeState()
        {
            SimpleTestState init = new(42);
            var simpleStore = new Store<SimpleTestState>(init, TestReducers.GenerateSetNSimpleTestStateReducer(243));

            simpleStore.Dispatch(new EmptyAction());

            simpleStore.State.N.Should().Be(243);
        }

        [Fact]
        public void InitializeActionSetsState()
        {
            Point init = new(0, 1);
            var pointStore = new Store<Point>(init, TestReducers.IdentityPointReducer);

            Point newState = new(10, 11);
            pointStore.Dispatch(new InitializeAction<Point>(newState));

            pointStore.State.Should().BeEquivalentTo(newState);
        }

        [Fact]
        public void InitializeActionWithWrongStateTypeThrows()
        {
            Point init = new(default, default);
            var pointStore = new Store<Point>(init, TestReducers.IdentityPointReducer);

            pointStore.Invoking(store =>
                    store.Dispatch(new InitializeAction<SimpleTestState>(new SimpleTestState(default))))
                .Should().Throw<InitializeTypeMismatchException>();
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
    }
}