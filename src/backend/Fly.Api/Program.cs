using Fly.Api.Data;
using Fly.Api.Models;
using Fly.Api.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Add DbContext with SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=fly.db"));

// Register services
builder.Services.AddScoped<WorkflowService>();
builder.Services.AddSingleton<WorkflowExecutorService>();

const string CorsPolicy = "FrontendCors";
builder.Services.AddCors(options =>
    options.AddPolicy(CorsPolicy, policy =>
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()));

var app = builder.Build();

// Ensure database is created and seed demo data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();

    // Seed demo workflow if database is empty
    if (!context.Workflows.Any())
    {
        var demo = new Workflow
        {
            Id = "demo-workflow-1",
            Name = "示例：ELISA检测流程",
            Description = "酶联免疫吸附测定 (ELISA) 标准流程示例",
            Nodes =
            [
                new WorkflowNode { Id = "n1", Type = "start",        Label = "开始",     Position = new() { X = 250, Y = 50  } },
                new WorkflowNode { Id = "n2", Type = "pipetting",    Label = "加样",     Position = new() { X = 250, Y = 150 },
                    Parameters = new() { ["volume_ul"] = 100.0, ["source"] = "样品孔板", ["target"] = "反应板" } },
                new WorkflowNode { Id = "n3", Type = "incubation",   Label = "孵育",     Position = new() { X = 250, Y = 250 },
                    Parameters = new() { ["temperature_c"] = 37.0, ["duration_min"] = 60.0 } },
                new WorkflowNode { Id = "n4", Type = "washing",      Label = "洗板",     Position = new() { X = 250, Y = 350 },
                    Parameters = new() { ["cycles"] = 3.0, ["volume_ul"] = 300.0 } },
                new WorkflowNode { Id = "n5", Type = "pipetting",    Label = "加酶标抗体", Position = new() { X = 250, Y = 450 },
                    Parameters = new() { ["volume_ul"] = 100.0, ["source"] = "抗体储液", ["target"] = "反应板" } },
                new WorkflowNode { Id = "n6", Type = "incubation",   Label = "二抗孵育", Position = new() { X = 250, Y = 550 },
                    Parameters = new() { ["temperature_c"] = 37.0, ["duration_min"] = 30.0 } },
                new WorkflowNode { Id = "n7", Type = "washing",      Label = "洗板",     Position = new() { X = 250, Y = 650 },
                    Parameters = new() { ["cycles"] = 5.0, ["volume_ul"] = 300.0 } },
                new WorkflowNode { Id = "n8", Type = "detection",    Label = "酶标仪检测", Position = new() { X = 250, Y = 750 },
                    Parameters = new() { ["mode"] = "absorbance", ["wavelength_nm"] = 450.0 } },
                new WorkflowNode { Id = "n9", Type = "end",          Label = "结束",     Position = new() { X = 250, Y = 850 } }
            ],
            Edges =
            [
                new WorkflowEdge { Id = "e1", Source = "n1", Target = "n2" },
                new WorkflowEdge { Id = "e2", Source = "n2", Target = "n3" },
                new WorkflowEdge { Id = "e3", Source = "n3", Target = "n4" },
                new WorkflowEdge { Id = "e4", Source = "n4", Target = "n5" },
                new WorkflowEdge { Id = "e5", Source = "n5", Target = "n6" },
                new WorkflowEdge { Id = "e6", Source = "n6", Target = "n7" },
                new WorkflowEdge { Id = "e7", Source = "n7", Target = "n8" },
                new WorkflowEdge { Id = "e8", Source = "n8", Target = "n9" }
            ]
        };
        context.Workflows.Add(demo);
        context.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors(CorsPolicy);
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
