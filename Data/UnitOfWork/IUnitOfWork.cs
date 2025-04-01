using Email_Worker_Service.Data.Repositories;
using Email_Worker_Service.Models;
using System;
using System.Threading.Tasks;

namespace Email_Worker_Service.Data.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<User> Users { get; }
        Task<int> CompleteAsync();
    }
} 