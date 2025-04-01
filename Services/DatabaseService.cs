using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Email_Worker_Service.Data.UnitOfWork;
using Email_Worker_Service.Models;
using Microsoft.Extensions.Logging;

namespace Email_Worker_Service.Services
{
    public interface IDatabaseService
    {
        Task<IEnumerable<User>> GetUsersForEmailProcessingAsync();
        Task UpdateUserEmailStatusAsync(int userId, DateTime emailSentTime);
    }

    public class DatabaseService : IDatabaseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DatabaseService> _logger;

        public DatabaseService(IUnitOfWork unitOfWork, ILogger<DatabaseService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<User>> GetUsersForEmailProcessingAsync()
        {
            try
            {
                // Get all users who haven't received an email yet
                var users = await _unitOfWork.Users.FindAsync(u => !u.IsEmailSent);
                _logger.LogInformation("Found {Count} users who need to receive emails", users);
                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching users for email processing");
                throw;
            }
        }

        public async Task UpdateUserEmailStatusAsync(int userId, DateTime emailSentTime)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user != null)
                {
                    user.LastEmailSent = emailSentTime;
                    user.IsEmailSent = true;
                    _unitOfWork.Users.Update(user);
                    await _unitOfWork.CompleteAsync();
                    _logger.LogInformation("Updated email status for user {UserId}", userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating user email status for userId: {UserId}", userId);
                throw;
            }
        }
    }
}
