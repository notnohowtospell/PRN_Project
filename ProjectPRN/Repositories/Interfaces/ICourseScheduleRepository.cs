using BusinessObjects.Models;
using System.Threading.Tasks;

namespace Repositories.Interfaces;

public interface ICourseScheduleRepository : IRepository<CourseSchedule>
{
    Task<IEnumerable<CourseSchedule>> GetByCourseIdAsync(int courseId);
}