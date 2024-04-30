using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Playdux.Utils;

namespace Playdux.Store;

internal interface IStateObservable<in TRootState>
    where TRootState : class, IEquatable<TRootState>
{
    void OnStateChanged(TRootState prevState, TRootState newState);
}

public class StateObservable<TRootState, TSelectedState>(
    IStateContainer<TRootState> store,
    Func<TRootState, TSelectedState> selector,
    bool notifyImmediately
) : IObservable<TSelectedState>, IStateObservable<TRootState>
    where TRootState : class, IEquatable<TRootState>
    where TSelectedState : IEquatable<TSelectedState>
{
    private readonly ConcurrentDictionary<IObserver<TSelectedState>, byte> _observers = new();

    public void OnStateChanged(TRootState prevState, TRootState newState)
    {
        if (_observers.IsEmpty) return;

        TSelectedState prevSelectedValue;
        TSelectedState newSelectedValue;
        try
        {
            prevSelectedValue = selector(prevState);
            newSelectedValue = selector(newState);
        }
        catch (Exception ex)
        {
            foreach (var observer in _observers.Keys)
            {
                observer.OnError(new StateObservableSelectorException(ex));
            }
            _observers.Clear();
            return;
        }
        
        if (EquatableExtensions.NullableEquals(prevSelectedValue, newSelectedValue))
        {
            return;
        }
        
        foreach (var observer in _observers.Keys)
        {
            observer.OnNext(newSelectedValue);
        }
    }

    public IDisposable Subscribe(IObserver<TSelectedState> observer)
    {
        ArgumentNullException.ThrowIfNull(observer);
        _observers.TryAdd(observer, default);
        if (notifyImmediately)
        {
            observer.OnNext(selector(store.State));
        }
        return new StateSubscriptionHandle(this, observer);
    }

    private class StateSubscriptionHandle(
        StateObservable<TRootState, TSelectedState> observable,
        IObserver<TSelectedState> observer
    ) : IDisposable
    {
        public void Dispose()
        {
            observable._observers.Remove(observer, out _);
        }
    }

    public class StateObservableSelectorException(Exception innerException)
        : Exception(
            $"An error was thrown in the selector for this {nameof(StateObservable<TRootState, TSelectedState>)}",
            innerException
        );
}