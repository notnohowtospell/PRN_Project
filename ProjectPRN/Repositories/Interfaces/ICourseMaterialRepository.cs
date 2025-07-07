using BusinessObjects.Models;
using System.Threading.Tasks;

namespace Repositories.Interfaces;

public interface ICourseMaterialRepository : IRepository<CourseMaterial>
{
    Task<IEnumerable<CourseMaterial>> GetByCourseIdAsync(int courseId);
}