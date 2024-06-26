using System.Diagnostics;
using FluentAssertions.Extensions;
using Playdux.Actions;

namespace Playdux.Tests.Actions;

public abstract class DispatchedActionTests
{
    private abstract record EmptyState;

    private class TestAction : IAction<EmptyState>;

    [Fact]
    public void HasCorrectDispatchedTime()
    {
            var now = DateTime.Now;
            var da = new DispatchedAction<EmptyState>(new TestAction());

            da.DispatchTime.Should().BeCloseTo(now, 10.Milliseconds());
        }

    [Fact]
    public void HasCorrectStackFrame()
    {
            var da = new DispatchedAction<EmptyState>(new TestAction());
            var thisMethodInfo = new StackTrace().GetFrame(0)!.GetMethod();

            da.DispatchStackTrace.GetFrame(0)!.GetMethod().Should().Be(thisMethodInfo);
        }
}