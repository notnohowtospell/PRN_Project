using BusinessObjects.Models;
using System.Threading.Tasks;

namespace Repositories.Interfaces;

public interface ICertificateRepository : IRepository<Certificate>
{
    Task<IEnumerable<Certificate>> GetByStudentIdAsync(int studentId);
    Task<IEnumerable<Certificate>> GetByCourseIdAsync(int courseId);

    Task<Certificate?> GetCertificateByStudentAndCourseAsync(int studentId, int courseId);

    Task CreateAsync(Certificate certificate);
    Task<IEnumerable<Certificate>> GetCertificatesByStudentAsync(int studentId);
}