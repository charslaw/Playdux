using Playdux.DataStructures;
// ReSharper disable UseObjectOrCollectionInitializer

namespace Playdux.Tests.DataStructures;

public class PostInsertSortedListTests
{
    [Fact]
    public void Add_AddsInitialElement()
    {
        var list = new PostInsertSortedList<string>();
        list.Add(1, "element");
        list.Should().Equal("element");
    }

    [Fact]
    public void Add_InsertsLowerPriorityElementBeforeHigherPriorityElement()
    {
        var list = new PostInsertSortedList<string>();
        list.Add(2, "first");
        list.Add(1, "second");
        list.Should().Equal("second", "first");
    }

    [Fact]
    public void Add_InsertsHigherPriorityElementAfterLowerPriorityElement()
    {
        var list = new PostInsertSortedList<string>();
        list.Add(1, "first");
        list.Add(2, "second");
        list.Should().Equal("first", "second");
    }

    [Fact]
    public void Add_InsertsElementsWithSamePriorityInOrderOfInsertionAtEndOfList()
    {
        var list = new PostInsertSortedList<string>();
        list.Add(1, "one");
        list.Add(1, "two");
        list.Add(1, "three");
        list.Should().Equal("one", "two", "three");
    }

    [Fact]
    public void Add_InsertsElementsWithSamePriorityInOrderOfInsertionInMiddleOfList()
    {
        var list = new PostInsertSortedList<string>();
        list.Add(2, "two-a");
        list.Add(1, "one");
        list.Add(3, "three");
        list.Add(2, "two-b");
        list.Should().Equal("one", "two-a", "two-b", "three");
    }
}