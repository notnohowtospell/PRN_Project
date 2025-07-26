using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects;

public interface IActivityLogDAO : IGenericDAO<ActivityLog>
{
    Task<IEnumerable<ActivityLog>> GetByUserIdAsync(int userId);
}

public class ActivityLogDAO : IActivityLogDAO
{
    private readonly ApplicationDbContext _context;

    public ActivityLogDAO()
    {
        _context = new ApplicationDbContext();
    }

    public async Task<IEnumerable<ActivityLog>> GetAllAsync()
    {
        return await _context.ActivityLogs.ToListAsync();
    }

    public async Task<ActivityLog?> GetByIdAsync(int id)
    {
        return await _context.ActivityLogs.FirstOrDefaultAsync(a => a.LogId == id);
    }

    public async Task<IEnumerable<ActivityLog>> GetByUserIdAsync(int userId)
    {
        return await _context.ActivityLogs.Where(a => a.UserId == userId).ToListAsync();
    }

    public async Task AddAsync(ActivityLog entity)
    {
        await _context.ActivityLogs.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(ActivityLog entity)
    {
        var existing = await _context.ActivityLogs.FindAsync(entity.LogId);
        if (existing != null)
        {
            existing.UserId = entity.UserId;
            existing.Action = entity.Action;
            existing.Timestamp = entity.Timestamp;
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.ActivityLogs.FindAsync(id);
        if (entity != null)
        {
            _context.ActivityLogs.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public IEnumerable<ActivityLog> GetAll()
    {
        throw new NotImplementedException();
    }
}