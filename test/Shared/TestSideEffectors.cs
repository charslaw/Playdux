using System;
using Playdux.Actions;
using Playdux.SideEffectors;
using Playdux.Store;

namespace Shared;

public static class TestSideEffectors
{
    public class FakeSideEffector<T> : ISideEffector<T>
        where T : class, IEquatable<T>
    {
        public int Priority { get; }

        private readonly Func<DispatchedAction, IStore<T>, bool> pre;
        private readonly Action<DispatchedAction, IStore<T>> post;

        public FakeSideEffector(Func<DispatchedAction, IStore<T>, bool>? pre = null, Action<DispatchedAction, IStore<T>>? post = null, int priority = 0)
        {
            this.pre = pre ?? ((_, _) => true);
            this.post = post ?? ((_, _) => { });
            Priority = priority;
        }

        public bool PreEffect(DispatchedAction dispatchedAction, IStore<T> store) => pre(dispatchedAction, store);
        public void PostEffect(DispatchedAction dispatchedAction, IStore<T> store) => post(dispatchedAction, store);
    }

    public class DoesNothingSideEffector<T> : FakeSideEffector<T>
        where T : class, IEquatable<T>
    {
        public DoesNothingSideEffector() : base((_, _) => true) { }
    }

    public class PreventsAllActionsSideEffector<T> : FakeSideEffector<T>
        where T : class, IEquatable<T>
    {
        public PreventsAllActionsSideEffector() : base((_, _) => false) { }
    }
}