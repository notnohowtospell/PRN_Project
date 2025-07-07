using BusinessObjects.Models;
using System.Threading.Tasks;

namespace Repositories.Interfaces;

public interface INotificationRepository : IRepository<Notification>
{
    Task<IEnumerable<Notification>> GetByStudentIdAsync(int studentId);
}