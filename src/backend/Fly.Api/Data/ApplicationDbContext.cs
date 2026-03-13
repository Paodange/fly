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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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
