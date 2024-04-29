using System.Collections.Generic;
using Playdux.Store;
using Shared;

namespace Playdux.Tests;

public class SideEffectorTests
{
    [Fact]
    public void IdentitySideEffectorHasNoEffect()
    {
            SimpleTestState init = new(0);
            var simpleStore = new Store<SimpleTestState>(init, TestReducers.IdentitySimpleTestStateReducer);
            simpleStore.RegisterSideEffector(new TestSideEffectors.DoesNothingSideEffector<SimpleTestState>());

            SimpleTestState newState = new(10);
            simpleStore.Dispatch(new InitializeAction<SimpleTestState>(newState));

            simpleStore.State.Should().BeEquivalentTo(newState);
        }

    [Fact]
    public void PreventativeSideEffectorPreventsStateChange()
    {
            SimpleTestState init = new(0);
            var simpleStore = new Store<SimpleTestState>(init, TestReducers.IdentitySimpleTestStateReducer);
            simpleStore.RegisterSideEffector(new TestSideEffectors.PreventsAllActionsSideEffector<SimpleTestState>());

            SimpleTestState newState = new(10);
            simpleStore.Dispatch(new InitializeAction<SimpleTestState>(newState));

            simpleStore.State.Should().BeEquivalentTo(init);
        }

    [Fact]
    public void PostSideEffectorGetsUpdatedStateFromAction()
    {
            SimpleTestState init = new(0);
            var simpleStore = new Store<SimpleTestState>(init, TestReducers.IdentitySimpleTestStateReducer);

            int? actualValue = null;
            simpleStore.RegisterSideEffector(new TestSideEffectors.FakeSideEffector<SimpleTestState>(post: (_, store) => { actualValue = store.State.N; }));

            SimpleTestState newState = new(10);
            simpleStore.Dispatch(new InitializeAction<SimpleTestState>(newState));

            actualValue.Should().Be(10);
        }

    [Fact]
    public void PreSideEffectorCanProduceSideEffects()
    {
            SimpleTestState init = new(0);
            var simpleStore = new Store<SimpleTestState>(init, TestReducers.IdentitySimpleTestStateReducer);

            var executeCount = 0;
            simpleStore.RegisterSideEffector(new TestSideEffectors.FakeSideEffector<SimpleTestState>((_, _) =>
            {
                executeCount++;
                return true;
            }));

            SimpleTestState newState = new(10);
            simpleStore.Dispatch(new InitializeAction<SimpleTestState>(newState));

            executeCount.Should().Be(1);
        }

    [Fact]
    public void PostSideEffectorCanProduceSideEffects()
    {
            SimpleTestState init = new(0);
            var simpleStore = new Store<SimpleTestState>(init, TestReducers.IdentitySimpleTestStateReducer);

            var executeCount = 0;
            simpleStore.RegisterSideEffector(new TestSideEffectors.FakeSideEffector<SimpleTestState>(post: (_, _) => executeCount++));

            SimpleTestState newState = new(10);
            simpleStore.Dispatch(new InitializeAction<SimpleTestState>(newState));

            executeCount.Should().Be(1);
        }

    [Fact]
    public void PreSideEffectorCanInterceptAndInjectSeparateAction()
    {
            var init = new SimpleTestState(0);
            var simpleStore = new Store<SimpleTestState>(init, TestReducers.AcceptAddSimpleTestStateReducer);

            simpleStore.RegisterSideEffector(new TestSideEffectors.FakeSideEffector<SimpleTestState>(pre: (dispatchedAction, dispatcher) =>
                {
                    var action = dispatchedAction.Action;
                    if (action is SimpleStateAddAction (var value))
                    {
                        dispatcher.Dispatch(new BetterSimpleStateAddAction(value));
                        return false;
                    }

                    return true;
                }
            ));

            simpleStore.Dispatch(new SimpleStateAddAction(5));

            simpleStore.State.N.Should().Be(6);
        }

    [Fact]
    public void PreSideEffectorInjectedActionWaitsForInitialActionCompletion()
    {
            var init = new SimpleTestState(0);
            var simpleStore = new Store<SimpleTestState>(init, TestReducers.IdentitySimpleTestStateReducer);

            var firstRun = true;
            simpleStore.RegisterSideEffector(new TestSideEffectors.FakeSideEffector<SimpleTestState>(pre: (_, store) =>
                {
                    if (!firstRun) return true;

                    firstRun = false;
                    store.Dispatch(new SimpleStateAddAction(7));

                    return true;
                }
            ));

            var order = new List<int>();
            simpleStore.RegisterSideEffector(new TestSideEffectors.FakeSideEffector<SimpleTestState>(post: (dispatchedAction, _) =>
                {
                    var actionAsSimpleAdd = dispatchedAction.Action as SimpleStateAddAction;

                    order.Add(actionAsSimpleAdd!.Value);
                }
            ));

            simpleStore.Dispatch(new SimpleStateAddAction(13));

            order.Should().BeEquivalentTo([13, 7]);
        }

    [Fact]
    public void UnregisteredSideEffectorDoesNotGetCalled()
    {
            var init = new SimpleTestState(0);
            var simpleStore = new Store<SimpleTestState>(init, TestReducers.IdentitySimpleTestStateReducer);

            var executeCount = 0;
            var sideEffectorId = simpleStore.RegisterSideEffector(new TestSideEffectors.FakeSideEffector<SimpleTestState>(pre: (_, _) =>
                {
                    executeCount++;
                    return true;
                }
            ));

            simpleStore.UnregisterSideEffector(sideEffectorId);

            simpleStore.Dispatch(new EmptyAction());

            executeCount.Should().Be(0);
        }

    [Fact]
    public void PreSideEffectorCantCancelOtherPreSideEffectors()
    {
            var init = new SimpleTestState(0);
            var simpleStore = new Store<SimpleTestState>(init, TestReducers.IdentitySimpleTestStateReducer);

            var secondCalled = false;

            simpleStore.RegisterSideEffector(new TestSideEffectors.FakeSideEffector<SimpleTestState>(pre: (_, _) => false));

            simpleStore.RegisterSideEffector(new TestSideEffectors.FakeSideEffector<SimpleTestState>(pre: (_, _) =>
                {
                    secondCalled = true;
                    return true;
                }
            ));

            simpleStore.Dispatch(new EmptyAction());

            secondCalled.Should().BeTrue();
        }

    [Fact]
    public void SideEffectorsWithSamePriorityActivatedInOrderOfAddition()
    {
            var init = new SimpleTestState(0);
            var simpleStore = new Store<SimpleTestState>(init, TestReducers.IdentitySimpleTestStateReducer);

            var order = new List<int>();

            simpleStore.RegisterSideEffector(new TestSideEffectors.FakeSideEffector<SimpleTestState>(pre: (_, _) =>
                {
                    order.Add(0);
                    return true;
                },
                priority: 0
            ));

            simpleStore.RegisterSideEffector(new TestSideEffectors.FakeSideEffector<SimpleTestState>(pre: (_, _) =>
                {
                    order.Add(1);
                    return true;
                },
                priority: 0
            ));

            simpleStore.RegisterSideEffector(new TestSideEffectors.FakeSideEffector<SimpleTestState>(pre: (_, _) =>
                {
                    order.Add(2);
                    return true;
                },
                priority: 0
            ));

            simpleStore.Dispatch(new EmptyAction());

            order.Should().BeEquivalentTo([0, 1, 2]);
        }

    [Fact]
    public void SideEffectorsOccurInPriorityOrder()
    {
            var init = new SimpleTestState(0);
            var simpleStore = new Store<SimpleTestState>(init, TestReducers.IdentitySimpleTestStateReducer);

            var order = new List<int>();

            simpleStore.RegisterSideEffector(new TestSideEffectors.FakeSideEffector<SimpleTestState>(pre: (_, _) =>
                {
                    order.Add(1);
                    return true;
                },
                priority: 0
            ));

            simpleStore.RegisterSideEffector(new TestSideEffectors.FakeSideEffector<SimpleTestState>(pre: (_, _) =>
                {
                    order.Add(0);
                    return true;
                },
                priority: 1
            ));

            simpleStore.Dispatch(new EmptyAction());

            order.Should().BeEquivalentTo([0, 1]);
        }

    [Fact]
    public void SideEffectorInsertOrderIsCorrect()
    {
            var init = new SimpleTestState(0);
            var simpleStore = new Store<SimpleTestState>(init, TestReducers.IdentitySimpleTestStateReducer);

            var order = new List<int>();

            simpleStore.RegisterSideEffector(new TestSideEffectors.FakeSideEffector<SimpleTestState>(pre: (_, _) =>
                {
                    order.Add(0);
                    return true;
                },
                priority: 0
            ));

            simpleStore.RegisterSideEffector(new TestSideEffectors.FakeSideEffector<SimpleTestState>(pre: (_, _) =>
                {
                    order.Add(3);
                    return true;
                },
                priority: -1
            ));

            simpleStore.RegisterSideEffector(new TestSideEffectors.FakeSideEffector<SimpleTestState>(pre: (_, _) =>
                {
                    order.Add(1);
                    return true;
                },
                priority: 0
            ));

            simpleStore.RegisterSideEffector(new TestSideEffectors.FakeSideEffector<SimpleTestState>(pre: (_, _) =>
                {
                    order.Add(2);
                    return true;
                },
                priority: 0
            ));

            simpleStore.Dispatch(new EmptyAction());

            order.Should().BeEquivalentTo([0, 1, 2, 3]);
        }

    [Fact]
    public void UnregisteringSideEffectorRemovesCorrectSideEffector()
    {
            var init = new SimpleTestState(0);
            var simpleStore = new Store<SimpleTestState>(init, TestReducers.IdentitySimpleTestStateReducer);

            var firstCalled = false;
            simpleStore.RegisterSideEffector(new TestSideEffectors.FakeSideEffector<SimpleTestState>((_, _) => {
                firstCalled = true;
                return true;
            }, priority: 0));
            
            var secondCalled = false;
            var secondID = simpleStore.RegisterSideEffector(new TestSideEffectors.FakeSideEffector<SimpleTestState>((_, _) => {
                secondCalled = true;
                return true;
            }, priority: 0));
            
            simpleStore.UnregisterSideEffector(secondID);
            
            simpleStore.Dispatch(new EmptyAction());

            (firstCalled, secondCalled).Should().Be((true, false));
        }

    [Fact]
    public void SideEffectorThrowingDoesNotPreventExecutionOfOtherSideEffectors()
    {
            var init = new SimpleTestState(0);
            var simpleStore = new Store<SimpleTestState>(init, TestReducers.IdentitySimpleTestStateReducer);
            
            simpleStore.RegisterSideEffector(new TestSideEffectors.FakeSideEffector<SimpleTestState>((_, _) => throw new Exception()));
            
            var secondCalled = false;
            simpleStore.RegisterSideEffector(new TestSideEffectors.FakeSideEffector<SimpleTestState>((_, _) => {
                secondCalled = true;
                return true;
            }));
            
            simpleStore.Dispatch(new EmptyAction());

            secondCalled.Should().BeTrue();
        }

    [Fact]
    public void UnregisteringNonexistantSideEffectorThrows()
    {
            var init = new SimpleTestState(0);
            var simpleStore = new Store<SimpleTestState>(init, TestReducers.IdentitySimpleTestStateReducer);

            simpleStore.Invoking(store => store.UnregisterSideEffector(Guid.NewGuid()))
                .Should().Throw<ArgumentException>();
        }
}