using BusinessObjects.Models;
using System.Threading.Tasks;

namespace Repositories.Interfaces;

public interface IInstructorRepository : IRepository<Instructor>
{
    Task<Instructor?> GetByPhoneNumberAsync(string phoneNumber);
}