using System;
using System.Diagnostics;

namespace Playdux.Store;

/// An IAction wrapped with some additional metadata for debugging purposes.
public sealed record DispatchedAction(IAction Action, DateTime DispatchTime, StackTrace DispatchStackTrace, bool IsCanceled)
{
    public DispatchedAction(IAction action) : this(action, DateTime.Now, new StackTrace(1), false) { }
}