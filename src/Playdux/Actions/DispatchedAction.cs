using System;
using System.Diagnostics;

namespace Playdux.Actions;

/// An IAction wrapped with some additional metadata for debugging purposes.
public sealed record DispatchedAction<TRootState>(IAction<TRootState> Action, DateTime DispatchTime, StackTrace DispatchStackTrace, bool IsCanceled)
    where TRootState : class, IEquatable<TRootState>
{
    public DispatchedAction(IAction<TRootState> action) : this(action, DateTime.Now, new StackTrace(1), false) { }
}