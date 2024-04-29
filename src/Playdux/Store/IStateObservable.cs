using System;

namespace Playdux.Store;

internal interface IStateObservable<in TRootState>
    where TRootState : class, IEquatable<TRootState>
{
    void OnStateChanged(TRootState newState);
}