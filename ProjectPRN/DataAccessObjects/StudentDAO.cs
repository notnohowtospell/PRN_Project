using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects;

public interface IStudentDAO : IGenericDAO<Student>
{
    Task<Student?> GetByEmailAsync(string email);
    Task<IEnumerable<Student>> GetByStatusAsync(string status);
}

public class StudentDAO : IStudentDAO
{
    private readonly ApplicationDbContext _context;

    public StudentDAO()
    {
        _context = new ApplicationDbContext();
    }

    public async Task<IEnumerable<Student>> GetAllAsync()
    {
        return await _context.Students
    .Include(s => s.Enrollments)
        .ThenInclude(e => e.Course)
    .Include(s => s.Certificates)
    .ToListAsync();
    }

    public async Task<Student?> GetByIdAsync(int id)
    {
        return await _context.Students.Include(s => s.Enrollments).Include(s => s.Certificates).FirstOrDefaultAsync(s => s.StudentId == id);
    }

    public async Task<Student?> GetByEmailAsync(string email)
    {
        return await _context.Students.FirstOrDefaultAsync(s => s.Email == email);
    }

    public async Task<IEnumerable<Student>> GetByStatusAsync(string status)
    {
        return await _context.Students.Where(s => s.Status == status).ToListAsync();
    }

    public async Task AddAsync(Student entity)
    {
        await _context.Students.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Student entity)
    {
        var existing = await _context.Students.FindAsync(entity.StudentId);
        if (existing != null)
        {
            existing.StudentCode = entity.StudentCode;
            existing.StudentName = entity.StudentName;
            existing.Password = entity.Password;
            existing.Email = entity.Email;
            existing.Status = entity.Status;
            existing.PhoneNumber = entity.PhoneNumber;
            existing.DateOfBirth = entity.DateOfBirth;
            existing.AvatarPath = entity.AvatarPath;
            existing.LastLogin = entity.LastLogin;
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Students.FindAsync(id);
        if (entity != null)
        {
            _context.Students.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public IEnumerable<Student> GetAll()
    {
        return _context.Students.Include(s => s.Enrollments).Include(s => s.Certificates).ToList();
    }
    public async Task<List<Student>> GetStudentsByCourseIdAsync(int courseId)
    {
        return await _context.Enrollments
            .Where(e => e.CourseId == courseId)
            .Select(e => e.Student)
            .ToListAsync();
    }
}