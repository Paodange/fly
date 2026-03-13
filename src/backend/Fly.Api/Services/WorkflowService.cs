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

    // Flow step (node) management methods
    public async Task<WorkflowNode?> GetStepAsync(string workflowId, string stepId)
    {
        var workflow = await _context.Workflows.FindAsync(workflowId);
        return workflow?.Nodes.FirstOrDefault(n => n.Id == stepId);
    }

    public async Task<IEnumerable<WorkflowNode>> GetStepsAsync(string workflowId)
    {
        var workflow = await _context.Workflows.FindAsync(workflowId);
        return workflow?.Nodes ?? Enumerable.Empty<WorkflowNode>();
    }

    public async Task<WorkflowNode?> CreateStepAsync(string workflowId, WorkflowNode node)
    {
        var workflow = await _context.Workflows.FindAsync(workflowId);
        if (workflow == null) return null;

        node.Id = Guid.NewGuid().ToString();
        workflow.Nodes.Add(node);
        workflow.UpdatedAt = DateTime.UtcNow;

        // Mark Nodes property as modified to trigger update
        _context.Entry(workflow).Property(w => w.Nodes).IsModified = true;

        await _context.SaveChangesAsync();

        return node;
    }

    public async Task<WorkflowNode?> UpdateStepAsync(string workflowId, string stepId, WorkflowNode updatedNode)
    {
        var workflow = await _context.Workflows.FindAsync(workflowId);
        if (workflow == null) return null;

        var existingNode = workflow.Nodes.FirstOrDefault(n => n.Id == stepId);
        if (existingNode == null) return null;

        // Update the node properties
        existingNode.Type = updatedNode.Type;
        existingNode.Label = updatedNode.Label;
        existingNode.Position = updatedNode.Position;
        existingNode.Parameters = updatedNode.Parameters;

        workflow.UpdatedAt = DateTime.UtcNow;

        // Mark Nodes property as modified to trigger update
        _context.Entry(workflow).Property(w => w.Nodes).IsModified = true;

        await _context.SaveChangesAsync();

        return existingNode;
    }

    public async Task<bool> DeleteStepAsync(string workflowId, string stepId)
    {
        var workflow = await _context.Workflows.FindAsync(workflowId);
        if (workflow == null) return false;

        var node = workflow.Nodes.FirstOrDefault(n => n.Id == stepId);
        if (node == null) return false;

        workflow.Nodes.Remove(node);

        // Also remove any edges connected to this node
        workflow.Edges.RemoveAll(e => e.Source == stepId || e.Target == stepId);

        workflow.UpdatedAt = DateTime.UtcNow;

        // Mark both Nodes and Edges as modified to trigger update
        _context.Entry(workflow).Property(w => w.Nodes).IsModified = true;
        _context.Entry(workflow).Property(w => w.Edges).IsModified = true;

        await _context.SaveChangesAsync();

        return true;
    }
}
