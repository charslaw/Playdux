using System.Diagnostics;
using FluentAssertions.Extensions;
using Playdux.Store;

namespace Playdux.Tests;

public class DispatchedActionTests
{
    private class TestAction : IAction { }

    [Fact]
    public void HasCorrectDispatchedTime()
    {
            var now = DateTime.Now;
            var da = new DispatchedAction(new TestAction());

            da.DispatchTime.Should().BeCloseTo(now, 10.Milliseconds());
        }

    [Fact]
    public void HasCorrectStackFrame()
    {
            var da = new DispatchedAction(new TestAction());
            var thisMethodInfo = new StackTrace().GetFrame(0)!.GetMethod();

            da.DispatchStackTrace.GetFrame(0)!.GetMethod().Should().Be(thisMethodInfo);
        }
}