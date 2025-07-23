using BusinessObjects.Models;
using DataAccessObjects;
using Repositories.Interfaces;

namespace Repositories;

public class EnrollmentRepository : IEnrollmentRepository
{
    private readonly IEnrollmentDAO _dao;

    public EnrollmentRepository(IEnrollmentDAO dao)
    {
        _dao = dao;
    }

    public async Task<IEnumerable<Enrollment>> GetAllAsync()
    {
        return await _dao.GetAllAsync();
    }

    public async Task<Enrollment?> GetByIdAsync(int id)
    {
        return await _dao.GetByIdAsync(id);
    }

    public async Task AddAsync(Enrollment entity)
    {
        await _dao.AddAsync(entity);
    }

    public async Task UpdateAsync(Enrollment entity)
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

    public async Task<IEnumerable<Enrollment>> GetByStudentAsync(int studentId)
    {
        return await _dao.GetByStudentAsync(studentId);
    }

    public async Task<IEnumerable<Enrollment>> GetByCourseAsync(int courseId)
    {
        return await _dao.GetByCourseAsync(courseId);
    }

    public IEnumerable<Enrollment> GetAll()
    {
        return _dao.GetAll();
    }
}