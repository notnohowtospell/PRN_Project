using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects;

public interface INotificationDAO : IGenericDAO<Notification>
{
    Task<IEnumerable<Notification>> GetByStudentIdAsync(int studentId);
}

public class NotificationDAO : INotificationDAO
{
    private readonly ApplicationDbContext _context;

    public NotificationDAO()
    {
        _context = new ApplicationDbContext();
    }

    public async Task<IEnumerable<Notification>> GetAllAsync()
    {
        return await _context.Notifications.Include(n => n.Student).ToListAsync();
    }

    public async Task<Notification?> GetByIdAsync(int id)
    {
        return await _context.Notifications.Include(n => n.Student).FirstOrDefaultAsync(n => n.NotificationId == id);
    }

    public async Task<IEnumerable<Notification>> GetByStudentIdAsync(int studentId)
    {
        return await _context.Notifications
    .Where(n => n.StudentId == studentId)
    .OrderByDescending(n => n.CreatedDate)
    .ToListAsync();
    }

    public async Task AddAsync(Notification entity)
    {
        await _context.Notifications.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Notification entity)
    {
        var existing = await _context.Notifications.FindAsync(entity.NotificationId);
        if (existing != null)
        {
            existing.Title = entity.Title;
            existing.Content = entity.Content;
            existing.StudentId = entity.StudentId;
            existing.CreatedDate = entity.CreatedDate;
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Notifications.FindAsync(id);
        if (entity != null)
        {
            _context.Notifications.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public IEnumerable<Notification> GetAll()
    {
        throw new NotImplementedException();
    }
}