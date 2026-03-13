using Fly.Api.Data;
using Fly.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Fly.Api.Services;

public class FlowStepService(ApplicationDbContext context)
{
    public async Task<IEnumerable<FlowStep>> GetAllAsync() =>
        await context.FlowSteps
            .OrderByDescending(s => s.UpdatedAt)
            .ToListAsync();

    public async Task<FlowStep?> GetByIdAsync(string id) =>
        await context.FlowSteps.FindAsync(id);

    public async Task<FlowStep> CreateAsync(FlowStep step)
    {
        step.Id = Guid.NewGuid().ToString();
        step.CreatedAt = DateTime.UtcNow;
        step.UpdatedAt = DateTime.UtcNow;

        context.FlowSteps.Add(step);
        await context.SaveChangesAsync();

        return step;
    }

    public async Task<FlowStep?> UpdateAsync(string id, FlowStep updated)
    {
        var existing = await context.FlowSteps.FindAsync(id);
        if (existing is null) return null;

        existing.Type = updated.Type;
        existing.Label = updated.Label;
        existing.Position = updated.Position;
        existing.Parameters = updated.Parameters;
        existing.UpdatedAt = DateTime.UtcNow;

        context.Entry(existing).Property(s => s.Position).IsModified = true;
        context.Entry(existing).Property(s => s.Parameters).IsModified = true;

        await context.SaveChangesAsync();

        return existing;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var step = await context.FlowSteps.FindAsync(id);
        if (step is null) return false;

        context.FlowSteps.Remove(step);
        await context.SaveChangesAsync();

        return true;
    }
}
