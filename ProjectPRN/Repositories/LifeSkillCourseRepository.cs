using BusinessObjects.Models;
using DataAccessObjects;
using Repositories.Interfaces;

namespace Repositories;

public class LifeSkillCourseRepository : ILifeSkillCourseRepository
{
    private readonly ILifeSkillCourseDAO _dao;

    public LifeSkillCourseRepository(ILifeSkillCourseDAO dao)
    {
        _dao = dao;
    }

    public async Task<IEnumerable<LifeSkillCourse>> GetAllAsync()
    {
        return await _dao.GetAllAsync();
    }

    public async Task<LifeSkillCourse?> GetByIdAsync(int id)
    {
        return await _dao.GetByIdAsync(id);
    }

    public async Task AddAsync(LifeSkillCourse entity)
    {
        await _dao.AddAsync(entity);
    }

    public async Task UpdateAsync(LifeSkillCourse entity)
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

    public async Task<IEnumerable<LifeSkillCourse>> GetByInstructorAsync(int instructorId)
    {
        return await _dao.GetByInstructorAsync(instructorId);
    }

    public async Task<IEnumerable<LifeSkillCourse>> GetByStatusAsync(string status)
    {
        return await _dao.GetByStatusAsync(status);
    }
}