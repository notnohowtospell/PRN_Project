using BusinessObjects.Models;
using DataAccessObjects;
using Repositories.Interfaces;

namespace Repositories;

public class AssessmentResultRepository : IAssessmentResultRepository
{
    private readonly IAssessmentResultDAO _dao;

    public AssessmentResultRepository(IAssessmentResultDAO dao)
    {
        _dao = dao;
    }

    public async Task<IEnumerable<AssessmentResult>> GetAllAsync()
    {
        return await _dao.GetAllAsync();
    }

    public async Task<AssessmentResult?> GetByIdAsync(int id)
    {
        return await _dao.GetByIdAsync(id);
    }

    public async Task AddAsync(AssessmentResult entity)
    {
        await _dao.AddAsync(entity);
    }

    public async Task UpdateAsync(AssessmentResult entity)
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

    public async Task<IEnumerable<AssessmentResult>> GetByAssessmentIdAsync(int assessmentId)
    {
        return await _dao.GetByAssessmentIdAsync(assessmentId);
    }

    public async Task<IEnumerable<AssessmentResult>> GetByStudentIdAsync(int studentId)
    {
        return await _dao.GetByStudentIdAsync(studentId);
    }

    public IEnumerable<AssessmentResult> GetAll()
    {
        throw new NotImplementedException();
    }
}