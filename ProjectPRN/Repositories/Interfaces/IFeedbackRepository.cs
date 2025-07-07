using BusinessObjects.Models;
using System.Threading.Tasks;

namespace Repositories.Interfaces;

public interface IFeedbackRepository : IRepository<Feedback>
{
    Task<IEnumerable<Feedback>> GetByStudentIdAsync(int studentId);
    Task<IEnumerable<Feedback>> GetByCourseIdAsync(int courseId);
}