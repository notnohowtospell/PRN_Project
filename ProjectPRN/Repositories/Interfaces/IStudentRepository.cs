using BusinessObjects.Models;

namespace Repositories.Interfaces;

public interface IStudentRepository : IRepository<Student>
{
    Task<IEnumerable<Student>> GetByEmail(string email);
    Task<IEnumerable<Student>> GetByStatus(string status);
}