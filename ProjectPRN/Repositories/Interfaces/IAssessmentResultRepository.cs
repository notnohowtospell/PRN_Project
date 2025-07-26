using BusinessObjects.Models;
using System.Threading.Tasks;

namespace Repositories.Interfaces;

public interface IAssessmentResultRepository : IRepository<AssessmentResult>
{
    Task<IEnumerable<AssessmentResult>> GetByStudentIdAsync(int studentId);
    Task<IEnumerable<AssessmentResult>> GetByAssessmentIdAsync(int assessmentId);
    Task UpdateAsyncResultNew(int id, AssessmentResult entity);

    // ➕ Thêm dòng này để lấy danh sách học sinh của khóa học
    Task<List<Student>> GetStudentsByCourseIdAsync(int courseId);

    Task<IEnumerable<AssessmentResult>> GetPassedResultsAsync(double minScore = 8, int? courseId = null, bool requireBeforeDeadline = false);
}