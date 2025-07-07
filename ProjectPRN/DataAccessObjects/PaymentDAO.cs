using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects;

public interface IPaymentDAO : IGenericDAO<Payment>
{
    Task<IEnumerable<Payment>> GetByStudentIdAsync(int studentId);
    Task<IEnumerable<Payment>> GetByCourseIdAsync(int courseId);
}

public class PaymentDAO : IPaymentDAO
{
    private readonly Prn212skillsHoannn6Context _context;

    public PaymentDAO()
    {
        _context = new Prn212skillsHoannn6Context();
    }

    public async Task<IEnumerable<Payment>> GetAllAsync()
    {
        return await _context.Payments.Include(p => p.Student).Include(p => p.Course).ToListAsync();
    }

    public async Task<Payment?> GetByIdAsync(int id)
    {
        return await _context.Payments.Include(p => p.Student).Include(p => p.Course).FirstOrDefaultAsync(p => p.PaymentId == id);
    }

    public async Task<IEnumerable<Payment>> GetByStudentIdAsync(int studentId)
    {
        return await _context.Payments.Where(p => p.StudentId == studentId).ToListAsync();
    }

    public async Task<IEnumerable<Payment>> GetByCourseIdAsync(int courseId)
    {
        return await _context.Payments.Where(p => p.CourseId == courseId).ToListAsync();
    }

    public async Task AddAsync(Payment entity)
    {
        await _context.Payments.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Payment entity)
    {
        var existing = await _context.Payments.FindAsync(entity.PaymentId);
        if (existing != null)
        {
            existing.StudentId = entity.StudentId;
            existing.CourseId = entity.CourseId;
            existing.Amount = entity.Amount;
            existing.Status = entity.Status;
            existing.PaymentDate = entity.PaymentDate;
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Payments.FindAsync(id);
        if (entity != null)
        {
            _context.Payments.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}