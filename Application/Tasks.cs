using MediatR;
using TaskApi.Domain;
namespace TaskApi.Application;
public interface ITaskRepository { Task<TaskItem> Add(TaskItem item, CancellationToken ct); Task<IReadOnlyList<TaskItem>> List(CancellationToken ct); Task<bool> Complete(int id, CancellationToken ct); }
public record CreateTask(string Title) : IRequest<TaskItem>;
public record ListTasks : IRequest<IReadOnlyList<TaskItem>>;
public record CompleteTask(int Id) : IRequest<bool>;
public class CreateTaskHandler(ITaskRepository repository) : IRequestHandler<CreateTask, TaskItem> {
 public Task<TaskItem> Handle(CreateTask request, CancellationToken ct) {
  if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Trim().Length > 200) throw new ArgumentException("Title must contain 1 to 200 characters.");
  return repository.Add(new TaskItem { Title = request.Title.Trim() }, ct);
 }
}
public class ListTasksHandler(ITaskRepository repository) : IRequestHandler<ListTasks, IReadOnlyList<TaskItem>> { public Task<IReadOnlyList<TaskItem>> Handle(ListTasks request, CancellationToken ct) => repository.List(ct); }
public class CompleteTaskHandler(ITaskRepository repository) : IRequestHandler<CompleteTask, bool> { public Task<bool> Handle(CompleteTask request, CancellationToken ct) => repository.Complete(request.Id, ct); }
