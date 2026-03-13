using Fly.Api.Data;
using Fly.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Fly.Api.Services;

public class WorkflowService
{
    private readonly ApplicationDbContext _context;

    public WorkflowService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Workflow>> GetAllAsync() =>
        await _context.Workflows
            .OrderByDescending(w => w.UpdatedAt)
            .ToListAsync();

    public async Task<Workflow?> GetByIdAsync(string id) =>
        await _context.Workflows.FindAsync(id);

    public async Task<Workflow> CreateAsync(Workflow workflow)
    {
        workflow.Id = Guid.NewGuid().ToString();
        workflow.CreatedAt = DateTime.UtcNow;
        workflow.UpdatedAt = DateTime.UtcNow;

        _context.Workflows.Add(workflow);
        await _context.SaveChangesAsync();

        return workflow;
    }

    public async Task<Workflow?> UpdateAsync(string id, Workflow workflow)
    {
        var existing = await _context.Workflows.FindAsync(id);
        if (existing == null) return null;

        existing.Name = workflow.Name;
        existing.Description = workflow.Description;
        existing.Nodes = workflow.Nodes;
        existing.Edges = workflow.Edges;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return existing;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var workflow = await _context.Workflows.FindAsync(id);
        if (workflow == null) return false;

        _context.Workflows.Remove(workflow);
        await _context.SaveChangesAsync();

        return true;
    }
}
