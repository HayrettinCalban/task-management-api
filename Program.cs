using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TaskApi.Application;
using TaskApi.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var key = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Set Jwt__Key to a random secret of at least 32 bytes.");
if (Encoding.UTF8.GetByteCount(key) < 32) throw new InvalidOperationException("JWT key must contain at least 32 bytes.");
var password = builder.Configuration["Demo:Password"] ?? throw new InvalidOperationException("Set Demo__Password.");
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o => o.TokenValidationParameters = new TokenValidationParameters { ValidateIssuer = true, ValidIssuer = "TaskApi", ValidateAudience = true, ValidAudience = "TaskApiClient", ValidateLifetime = true, ValidateIssuerSigningKey = true, IssuerSigningKey = signingKey, ClockSkew = TimeSpan.Zero });
builder.Services.AddAuthorization();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreateTask>());
builder.Services.AddDbContext<TaskDb>(o => o.UseSqlite(builder.Configuration.GetConnectionString("Database") ?? "Data Source=tasks.db"));
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddEndpointsApiExplorer(); builder.Services.AddSwaggerGen();
var app = builder.Build();
using (var scope = app.Services.CreateScope()) scope.ServiceProvider.GetRequiredService<TaskDb>().Database.EnsureCreated();
app.UseSwagger(); app.UseSwaggerUI(); app.UseAuthentication(); app.UseAuthorization();
app.MapPost("/auth/token", (Login input) => {
 if (input.Username != "admin" || !CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(input.Password ?? ""), Encoding.UTF8.GetBytes(password))) return Results.Unauthorized();
 var token = new JwtSecurityToken("TaskApi", "TaskApiClient", new[] { new Claim(ClaimTypes.Name, "admin"), new Claim(ClaimTypes.Role, "Admin") }, expires: DateTime.UtcNow.AddMinutes(30), signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));
 return Results.Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
});
app.MapGet("/tasks", async (ISender sender, CancellationToken ct) => Results.Ok(await sender.Send(new ListTasks(), ct))).RequireAuthorization();
app.MapPost("/tasks", async (CreateTask cmd, ISender sender, CancellationToken ct) => {
 try { var item = await sender.Send(cmd, ct); return Results.Created($"/tasks/{item.Id}", item); } catch (ArgumentException e) { return Results.BadRequest(e.Message); }
}).RequireAuthorization(p => p.RequireRole("Admin"));
app.MapPut("/tasks/{id:int}/complete", async (int id, ISender sender, CancellationToken ct) => await sender.Send(new CompleteTask(id), ct) ? Results.NoContent() : Results.NotFound()).RequireAuthorization(p => p.RequireRole("Admin"));
app.Run();
public record Login(string Username, string Password);
