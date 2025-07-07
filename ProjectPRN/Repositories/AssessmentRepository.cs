using BusinessObjects.Models;
using DataAccessObjects;
using Repositories.Interfaces;

namespace Repositories;

public class AssessmentRepository : IAssessmentRepository
{
    private readonly IAssessmentDAO _dao;

    public AssessmentRepository(IAssessmentDAO dao)
    {
        _dao = dao;
    }

    public async Task<IEnumerable<Assessment>> GetAllAsync()
    {
        return await _dao.GetAllAsync();
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
}