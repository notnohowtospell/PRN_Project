using BusinessObjects.Models;
using DataAccessObjects;
using Repositories.Interfaces;

namespace Repositories;

public class CourseScheduleRepository : ICourseScheduleRepository
{
    private readonly ICourseScheduleDAO _dao;

    public CourseScheduleRepository(ICourseScheduleDAO dao)
    {
        _dao = dao;
    }

    public async Task<IEnumerable<CourseSchedule>> GetAllAsync()
    {
        return await _dao.GetAllAsync();
    }

    public async Task<CourseSchedule?> GetByIdAsync(int id)
    {
        return await _dao.GetByIdAsync(id);
    }

    public async Task AddAsync(CourseSchedule entity)
    {
        await _dao.AddAsync(entity);
    }

    public async Task UpdateAsync(CourseSchedule entity)
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

    public async Task<IEnumerable<CourseSchedule>> GetByCourseIdAsync(int courseId)
    {
        return await _dao.GetByCourseIdAsync(courseId);
    }
}