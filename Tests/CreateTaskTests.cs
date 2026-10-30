using TaskApi.Application;
using TaskApi.Domain;
using Xunit;
public class CreateTaskTests {
 [Fact] public async Task EmptyTitleIsRejected() { var handler = new CreateTaskHandler(new FakeRepository()); await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(new CreateTask("  "), CancellationToken.None)); }
 [Fact] public async Task ValidTitleIsTrimmedAndStored() { var repo = new FakeRepository(); var result = await new CreateTaskHandler(repo).Handle(new CreateTask("  Review  "), CancellationToken.None); Assert.Equal("Review", result.Title); Assert.Same(result, repo.Stored); }
 private class FakeRepository : ITaskRepository { public TaskItem? Stored; public Task<TaskItem> Add(TaskItem item, CancellationToken ct) { Stored = item; return Task.FromResult(item); } public Task<IReadOnlyList<TaskItem>> List(CancellationToken ct) => throw new NotImplementedException(); public Task<bool> Complete(int id, CancellationToken ct) => throw new NotImplementedException(); }
}
