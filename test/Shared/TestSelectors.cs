using System;

namespace Shared;

public static class TestSelectors
{
    public static SimpleTestState ErrorSimpleTestStateSelector(SimpleTestState state) => throw new Exception();
}