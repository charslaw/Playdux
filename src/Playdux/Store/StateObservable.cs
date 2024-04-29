using System;
using System.Collections.Generic;
using Playdux.Utils;

namespace Playdux.Store;

internal class StateObservable<TRootState, TSelectedState>(
    Func<TRootState, TSelectedState> selector,
    bool notifyImmediately,
    TRootState initial
) : IObservable<TSelectedState>, IStateObservable<TRootState>
    where TRootState : class, IEquatable<TRootState>
    where TSelectedState : IEquatable<TSelectedState>
{
    private readonly List<IObserver<TSelectedState>> _observers = [];
    private TSelectedState _lastObservedValue = selector(initial);

    public void OnStateChanged(TRootState newState)
    {
        if (_observers.Count <= 0) return;

        var newSelectedValue = selector(newState);

        if (EquatableUtils.NullableEquals<TSelectedState, TSelectedState>(_lastObservedValue, newSelectedValue))
        {
            return;
        }

        _lastObservedValue = newSelectedValue;
        foreach (var observer in _observers)
        {
            observer.OnNext(_lastObservedValue);
        }
    }

    public IDisposable Subscribe(IObserver<TSelectedState> observer)
    {
        _observers.Add(observer);
        if (notifyImmediately) observer.OnNext(_lastObservedValue);
        return new StateSubscriptionHandle(this, observer);
    }

    private class StateSubscriptionHandle(
        StateObservable<TRootState, TSelectedState> observable,
        IObserver<TSelectedState> observer
    ) : IDisposable
    {
        public void Dispose()
        {
            observable._observers.Remove(observer);
        }
    }
}