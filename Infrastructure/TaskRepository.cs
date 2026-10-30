using Microsoft.EntityFrameworkCore;
using TaskApi.Application;
using TaskApi.Domain;
namespace TaskApi.Infrastructure;
public class TaskDb(DbContextOptions<TaskDb> options) : DbContext(options) { public DbSet<TaskItem> Tasks => Set<TaskItem>(); }
public class TaskRepository(TaskDb db) : ITaskRepository {
 public async Task<TaskItem> Add(TaskItem item, CancellationToken ct) { db.Tasks.Add(item); await db.SaveChangesAsync(ct); return item; }
 public async Task<IReadOnlyList<TaskItem>> List(CancellationToken ct) => await db.Tasks.AsNoTracking().OrderBy(x => x.Id).ToListAsync(ct);
 public async Task<bool> Complete(int id, CancellationToken ct) { var item = await db.Tasks.FindAsync(new object[]{id}, ct); if (item is null) return false; item.IsComplete = true; await db.SaveChangesAsync(ct); return true; }
}
