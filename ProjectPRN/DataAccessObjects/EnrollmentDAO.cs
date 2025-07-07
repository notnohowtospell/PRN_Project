using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects;

public interface IEnrollmentDAO : IGenericDAO<Enrollment>
{
    Task<IEnumerable<Enrollment>> GetByStudentAsync(int studentId);
    Task<IEnumerable<Enrollment>> GetByCourseAsync(int courseId);
}

public class EnrollmentDAO : IEnrollmentDAO
{
    private readonly Prn212skillsHoannn6Context _context;

    public EnrollmentDAO()
    {
        _context = new Prn212skillsHoannn6Context();
    }

    public async Task<IEnumerable<Enrollment>> GetAllAsync()
    {
        return await _context.Enrollments.Include(e => e.Student).Include(e => e.Course).ToListAsync();
    }

    public async Task<Enrollment?> GetByIdAsync(int id)
    {
        return await _context.Enrollments.Include(e => e.Student).Include(e => e.Course).FirstOrDefaultAsync(e => e.EnrollmentId == id);
    }

    public async Task<IEnumerable<Enrollment>> GetByStudentAsync(int studentId)
    {
        return await _context.Enrollments.Where(e => e.StudentId == studentId).Include(e => e.Course).ToListAsync();
    }

    public async Task<IEnumerable<Enrollment>> GetByCourseAsync(int courseId)
    {
        return await _context.Enrollments.Where(e => e.CourseId == courseId).Include(e => e.Student).ToListAsync();
    }

    public async Task AddAsync(Enrollment entity)
    {
        await _context.Enrollments.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Enrollment entity)
    {
        var existing = await _context.Enrollments.FindAsync(entity.EnrollmentId);
        if (existing != null)
        {
            existing.StudentId = entity.StudentId;
            existing.CourseId = entity.CourseId;
            existing.CompletionStatus = entity.CompletionStatus;
            existing.CompletionDate = entity.CompletionDate;
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Enrollments.FindAsync(id);
        if (entity != null)
        {
            _context.Enrollments.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}