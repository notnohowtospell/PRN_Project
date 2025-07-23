using BusinessObjects.Models;
using DataAccessObjects;
using Repositories.Interfaces;

namespace Repositories;

public class FeedbackRepository : IFeedbackRepository
{
    private readonly IFeedbackDAO _dao;

    public FeedbackRepository(IFeedbackDAO dao)
    {
        _dao = dao;
    }

    public async Task<IEnumerable<Feedback>> GetAllAsync()
    {
        return await _dao.GetAllAsync();
    }

    public async Task<Feedback?> GetByIdAsync(int id)
    {
        return await _dao.GetByIdAsync(id);
    }

    public async Task AddAsync(Feedback entity)
    {
        await _dao.AddAsync(entity);
    }

    public async Task UpdateAsync(Feedback entity)
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

    public async Task<IEnumerable<Feedback>> GetByStudentIdAsync(int studentId)
    {
        return await _dao.GetByStudentIdAsync(studentId);
    }

    public async Task<IEnumerable<Feedback>> GetByCourseIdAsync(int courseId)
    {
        return await _dao.GetByCourseIdAsync(courseId);
    }

    public IEnumerable<Feedback> GetAll()
    {
        throw new NotImplementedException();
    }
}