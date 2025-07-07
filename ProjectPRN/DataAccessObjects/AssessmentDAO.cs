using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects;

public interface IAssessmentDAO : IGenericDAO<Assessment>
{
    Task<IEnumerable<Assessment>> GetByCourseIdAsync(int courseId);
}

public class AssessmentDAO : IAssessmentDAO
{
    private readonly Prn212skillsHoannn6Context _context;

    public AssessmentDAO()
    {
        _context = new Prn212skillsHoannn6Context();
    }

    public async Task<IEnumerable<Assessment>> GetAllAsync()
    {
        return await _context.Assessments.Include(a => a.Course).ToListAsync();
    }

    public async Task<Assessment?> GetByIdAsync(int id)
    {
        return await _context.Assessments.Include(a => a.Course).FirstOrDefaultAsync(a => a.AssessmentId == id);
    }

    public async Task<IEnumerable<Assessment>> GetByCourseIdAsync(int courseId)
    {
        return await _context.Assessments.Where(a => a.CourseId == courseId).ToListAsync();
    }

    public async Task AddAsync(Assessment entity)
    {
        await _context.Assessments.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Assessment entity)
    {
        var existing = await _context.Assessments.FindAsync(entity.AssessmentId);
        if (existing != null)
        {
            existing.CourseId = entity.CourseId;
            existing.AssessmentName = entity.AssessmentName;
            existing.MaxScore = entity.MaxScore;
            existing.DueDate = entity.DueDate;
            existing.AssessmentType = entity.AssessmentType;
            existing.Instructions = entity.Instructions;
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Assessments.FindAsync(id);
        if (entity != null)
        {
            _context.Assessments.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}