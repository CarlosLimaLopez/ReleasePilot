using ReleasePilot.Domain.Exceptions;
using ReleasePilot.Domain.ValueObjects;

namespace ReleasePilot.Domain.UnitTests.ValueObjects;

public class WorkItemReferenceTests
{
    [Fact]
    public void Create_WithValidIssueId_ReturnsWorkItemReference()
    {
        var workItem = WorkItemReference.Create("WI-123");

        Assert.Equal("WI-123", workItem.IssueId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespace_ThrowsDomainException(string? issueId)
    {
        Assert.Throws<DomainException>(() => WorkItemReference.Create(issueId!));
    }

    [Fact]
    public void Constructor_SetsIssueId()
    {
        var workItem = new WorkItemReference("ISSUE-1");

        Assert.Equal("ISSUE-1", workItem.IssueId);
    }

    [Fact]
    public void Equality_TwoInstancesWithSameIssueId_AreEqual()
    {
        var workItem1 = WorkItemReference.Create("WI-1");
        var workItem2 = WorkItemReference.Create("WI-1");

        Assert.Equal(workItem1, workItem2);
    }

    [Fact]
    public void Equality_TwoInstancesWithDifferentIssueIds_AreNotEqual()
    {
        var workItem1 = WorkItemReference.Create("WI-1");
        var workItem2 = WorkItemReference.Create("WI-2");

        Assert.NotEqual(workItem1, workItem2);
    }
}