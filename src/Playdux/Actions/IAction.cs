using System;

namespace Playdux.Actions;

/// <summary>An action to be dispatched to a Store.</summary>
public interface IAction<TRootState> where TRootState : class, IEquatable<TRootState>;