using BusinessObjects.Models;
using System.Threading.Tasks;

namespace Repositories.Interfaces;

public interface ILifeSkillCourseRepository : IRepository<LifeSkillCourse>
{
    Task<IEnumerable<LifeSkillCourse>> GetByInstructorAsync(int instructorId);
    Task<IEnumerable<LifeSkillCourse>> GetByStatusAsync(string status);
}