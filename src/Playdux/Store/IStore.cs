namespace Playdux.Store;

public interface IStore<out TRootState> : IActionDispatcher<TRootState>, IStateContainer<TRootState> { }