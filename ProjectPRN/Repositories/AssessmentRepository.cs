using BusinessObjects.Models;
using DataAccessObjects;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;

namespace Repositories;

public class AssessmentRepository : IAssessmentRepository
{
    private readonly IAssessmentDAO _dao;
    private readonly ApplicationDbContext _context = new ApplicationDbContext();

    public AssessmentRepository(IAssessmentDAO dao)
    {
        _dao = dao;
    }

    public async Task<IEnumerable<Assessment>> GetAllAsync()
    {
        using var context = new ApplicationDbContext(); // đổi thành context thật
        return await context.Assessments
            .Include(a => a.Course) // <-- include Course để lấy tên
            .ToListAsync();
    }

    public async Task<Assessment?> GetByIdAsync(int id)
    {
        return await _dao.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Assessment>> GetByCourseIdAsync(int courseId)
    {
        return await _dao.GetByCourseIdAsync(courseId);
    }

    public async Task AddAsync(Assessment entity)
    {
        await _dao.AddAsync(entity);
    }

    public async Task UpdateAsync(Assessment entity)
    {
        await _dao.UpdateAsync(entity);
    }

    public async Task DeleteAsync(int id)
    {
        await _dao.DeleteAsync(id);
    }

    public async Task SaveChangesAsync()
    {
        await Task.CompletedTask;
    }

    public IEnumerable<Assessment> GetAll()
    {
        throw new NotImplementedException();
    }
    public async Task UpdateAsyncNew(int id, Assessment entity)
    {
        using var context = new ApplicationDbContext();

        var existing = await context.Assessments.FindAsync(id);
        if (existing != null)
        {
            existing.InstructionFilePath = entity.InstructionFilePath;
            await context.SaveChangesAsync();
        }
    }
}