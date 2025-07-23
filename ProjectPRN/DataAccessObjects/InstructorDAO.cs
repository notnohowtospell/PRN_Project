using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects;

public interface IInstructorDAO : IGenericDAO<Instructor>
{
    Task<Instructor?> GetByPhoneNumberAsync(string phoneNumber);
    Task<Instructor?> GetByEmailAsync(string email);
}

public class InstructorDAO : IInstructorDAO
{
    private readonly ApplicationDbContext _context;

    public InstructorDAO()
    {
        _context = new ApplicationDbContext();
    }

    public async Task<IEnumerable<Instructor>> GetAllAsync()
    {
        return await _context.Instructors.Include(i => i.LifeSkillCourses).ToListAsync();
    }

    public async Task<Instructor?> GetByIdAsync(int id)
    {
        return await _context.Instructors.Include(i => i.LifeSkillCourses).FirstOrDefaultAsync(i => i.InstructorId == id);
    }

    public async Task<Instructor?> GetByPhoneNumberAsync(string phoneNumber)
    {
        return await _context.Instructors.FirstOrDefaultAsync(i => i.PhoneNumber == phoneNumber);
    }

    public async Task<Instructor?> GetByEmailAsync(string email)
    {
        return await _context.Instructors.FirstOrDefaultAsync(i => i.Email == email);
    }

    public async Task AddAsync(Instructor entity)
    {
        await _context.Instructors.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Instructor entity)
    {
        var existing = await _context.Instructors.FindAsync(entity.InstructorId);
        if (existing != null)
        {
            existing.InstructorName = entity.InstructorName;
            existing.PhoneNumber = entity.PhoneNumber;
            existing.Email = entity.Email;
            existing.Experience = entity.Experience;
            existing.Password = entity.Password;
            existing.LastLogin = entity.LastLogin;
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Instructors.FindAsync(id);
        if (entity != null)
        {
            _context.Instructors.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public IEnumerable<Instructor> GetAll()
    {
        return _context.Instructors.Include(i => i.LifeSkillCourses).ToList();
    }
}