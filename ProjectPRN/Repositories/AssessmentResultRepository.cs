using BusinessObjects.Models;
using DataAccessObjects;
using Microsoft.EntityFrameworkCore;
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
    public async Task UpdateAsyncResultNew(int id, AssessmentResult entity)
    {
        using var context = new ApplicationDbContext();

        var existing = await context.AssessmentResults.FindAsync(id);
        if (existing != null)
        {
            existing.SubmissionFilePath = entity.SubmissionFilePath;
            existing.SubmissionDate = entity.SubmissionDate; // 👈 THÊM DÒNG NÀY
            await context.SaveChangesAsync();
        }
    }
    public async Task<List<Student>> GetStudentsByCourseIdAsync(int courseId)
    {
        using var context = new ApplicationDbContext();
        return await context.Enrollments
            .Where(e => e.CourseId == courseId)
            .Select(e => e.Student)
            .ToListAsync();
    }
    public async Task<IEnumerable<AssessmentResult>> GetPassedResultsAsync(double minScore = 8, int? courseId = null, bool requireBeforeDeadline = false)
    {
        using var context = new ApplicationDbContext();

        var query = context.AssessmentResults
            .Include(ar => ar.Student)
            .Include(ar => ar.Assessment)
                .ThenInclude(a => a.Course)
            .Where(ar => ar.Score.HasValue && ar.Score.Value >= (decimal)minScore);

        if (courseId.HasValue)
        {
            query = query.Where(ar => ar.Assessment.CourseId == courseId.Value);
        }

        if (requireBeforeDeadline)
        {
            query = query.Where(ar => ar.SubmissionDate <= ar.Assessment.DueDate);
        }

        return await query.ToListAsync();
    }
}
