#nullable enable
using System;
using FluentAssertions;
using Playdux.src.Store;

namespace Playdux.test
{
    public class StoreErrorHandlingTests
    {
        private static SimpleTestState MethodThatThrows(SimpleTestState _) => throw new Exception();

        [Fact]
        public void ErrorInSubscribeDoesNotBreakOtherObservers()
        {
            SimpleTestState init = new(42);
            var simpleStore = new Store<SimpleTestState>(init, TestReducers.IncrementNSimpleTestStateReducer);

            int notified = 0;
            int errorSeen = 0;
            simpleStore.ObservableFor(state => state)
                .Subscribe(
                    onNext: _ => throw new Exception(),
                    onError: _ => { });
            simpleStore.ObservableFor(state => state)
                .Subscribe(
                    onNext: _ => notified++,
                    onError: _ => errorSeen++);

            simpleStore.Dispatch(new EmptyAction());
            simpleStore.Dispatch(new EmptyAction());

            (notified, errorSeen).Should().Be((2, 0));
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
}