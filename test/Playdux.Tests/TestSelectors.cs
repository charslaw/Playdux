namespace Playdux.Tests;

internal static class TestSelectors
{
    public static SimpleTestState ErrorSimpleTestStateSelector(SimpleTestState state) => throw new Exception();
}