using Fly.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Fly.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Workflow> Workflows => Set<Workflow>();
    public DbSet<FlowStep> FlowSteps => Set<FlowStep>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure FlowStep entity
        modelBuilder.Entity<FlowStep>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Type).IsRequired().HasMaxLength(200);
            entity.Property(s => s.Label).IsRequired().HasMaxLength(200);
            entity.Property(s => s.CreatedAt).IsRequired();
            entity.Property(s => s.UpdatedAt).IsRequired();

            // Store Position as JSON string
            entity.Property(s => s.Position)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<NodePosition>(v, (JsonSerializerOptions)null!) ?? new NodePosition()
                )
                .HasColumnType("TEXT");

            // Store Parameters as JSON string
            entity.Property(s => s.Parameters)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<Dictionary<string, object?>>(v, (JsonSerializerOptions)null!) ?? new Dictionary<string, object?>()
                )
                .HasColumnType("TEXT");
        });

        // Configure Workflow entity
        modelBuilder.Entity<Workflow>(entity =>
        {
            entity.HasKey(w => w.Id);
            entity.Property(w => w.Name).IsRequired().HasMaxLength(200);
            entity.Property(w => w.Description).HasMaxLength(1000);
            entity.Property(w => w.CreatedAt).IsRequired();
            entity.Property(w => w.UpdatedAt).IsRequired();

            // Store Nodes as JSON string
            entity.Property(w => w.Nodes)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<List<WorkflowNode>>(v, (JsonSerializerOptions)null!) ?? new List<WorkflowNode>()
                )
                .HasColumnType("TEXT");

            // Store Edges as JSON string
            entity.Property(w => w.Edges)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<List<WorkflowEdge>>(v, (JsonSerializerOptions)null!) ?? new List<WorkflowEdge>()
                )
                .HasColumnType("TEXT");

            // Create index on UpdatedAt for sorting
            entity.HasIndex(w => w.UpdatedAt);
        });
    }
}
