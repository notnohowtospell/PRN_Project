using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects;

public interface ICourseMaterialDAO : IGenericDAO<CourseMaterial>
{
    Task<IEnumerable<CourseMaterial>> GetByCourseIdAsync(int courseId);
}

public class CourseMaterialDAO : ICourseMaterialDAO
{
    private readonly Prn212skillsHoannn6Context _context;

    public CourseMaterialDAO()
    {
        _context = new Prn212skillsHoannn6Context();
    }

    public async Task<IEnumerable<CourseMaterial>> GetAllAsync()
    {
        return await _context.CourseMaterials.Include(m => m.Course).ToListAsync();
    }

    public async Task<CourseMaterial?> GetByIdAsync(int id)
    {
        return await _context.CourseMaterials.Include(m => m.Course).FirstOrDefaultAsync(m => m.MaterialId == id);
    }

    public async Task<IEnumerable<CourseMaterial>> GetByCourseIdAsync(int courseId)
    {
        return await _context.CourseMaterials.Where(m => m.CourseId == courseId).ToListAsync();
    }

    public async Task AddAsync(CourseMaterial entity)
    {
        await _context.CourseMaterials.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(CourseMaterial entity)
    {
        var existing = await _context.CourseMaterials.FindAsync(entity.MaterialId);
        if (existing != null)
        {
            existing.CourseId = entity.CourseId;
            existing.Title = entity.Title;
            existing.FilePath = entity.FilePath;
            existing.UploadDate = entity.UploadDate;
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.CourseMaterials.FindAsync(id);
        if (entity != null)
        {
            _context.CourseMaterials.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}