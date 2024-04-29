using Playdux.Store;

namespace Playdux.Tests;

internal record EmptyAction : IAction;

internal record SimpleStateAddAction(int Value) : IAction;

internal record BetterSimpleStateAddAction(int Value) : IAction;