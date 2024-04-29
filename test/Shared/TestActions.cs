using Playdux.Store;

namespace Shared;

public record EmptyAction : IAction;

public record SimpleStateAddAction(int Value) : IAction;

public record BetterSimpleStateAddAction(int Value) : IAction;