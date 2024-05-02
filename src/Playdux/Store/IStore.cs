using System;

namespace Playdux.Store;

public interface IStore<TRootState>
    : IActionDispatcher<TRootState>, IStateContainer<TRootState>
    where TRootState : class, IEquatable<TRootState>;