using BusinessObjects.Models;
using System.Threading.Tasks;

namespace Repositories.Interfaces;

public interface IAssessmentRepository : IRepository<Assessment>
{
    Task<IEnumerable<Assessment>> GetByCourseIdAsync(int courseId);
    Task UpdateAsyncNew(int id, Assessment entity);
}