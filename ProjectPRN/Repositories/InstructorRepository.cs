using BusinessObjects.Models;
using DataAccessObjects;
using Repositories.Interfaces;

namespace Repositories;

public class InstructorRepository : IInstructorRepository
{
    private readonly IInstructorDAO _dao;

    public InstructorRepository(IInstructorDAO dao)
    {
        _dao = dao;
    }

    public async Task<IEnumerable<Instructor>> GetAllAsync()
    {
        return await _dao.GetAllAsync();
    }

    public async Task<Instructor?> GetByIdAsync(int id)
    {
        return await _dao.GetByIdAsync(id);
    }

    public async Task AddAsync(Instructor entity)
    {
        await _dao.AddAsync(entity);
    }

    public async Task UpdateAsync(Instructor entity)
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

    public async Task<Instructor?> GetByPhoneNumberAsync(string phoneNumber)
    {
        return await _dao.GetByPhoneNumberAsync(phoneNumber);
    }
}