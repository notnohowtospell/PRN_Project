using BusinessObjects.Models;
using DataAccessObjects;
using Repositories.Interfaces;

namespace Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly IPaymentDAO _paymentDAO;

    public PaymentRepository(IPaymentDAO paymentDAO)
    {
        _paymentDAO = paymentDAO;
    }

    public async Task<IEnumerable<Payment>> GetAllAsync()
    {
        return await _paymentDAO.GetAllAsync();
    }

    public async Task<Payment?> GetByIdAsync(int id)
    {
        return await _paymentDAO.GetByIdAsync(id);
    }

    public async Task AddAsync(Payment entity)
    {
        await _paymentDAO.AddAsync(entity);
    }

    public async Task UpdateAsync(Payment entity)
    {
        await _paymentDAO.UpdateAsync(entity);
    }

    public async Task DeleteAsync(int id)
    {
        await _paymentDAO.DeleteAsync(id);
    }

    public async Task SaveChangesAsync()
    {
        await Task.CompletedTask;
    }

    public async Task<IEnumerable<Payment>> GetByStudentIdAsync(int studentId)
    {
        return await _paymentDAO.GetByStudentIdAsync(studentId);
    }

    public async Task<IEnumerable<Payment>> GetByCourseIdAsync(int courseId)
    {
        return await _paymentDAO.GetByCourseIdAsync(courseId);
    }
}