using System;
using System.Threading;
using System.Threading.Tasks;
using BlogLab.Models.Account;
using BlogLab.Repository;
using Microsoft.AspNetCore.Identity;

namespace BlogLab.Identity
{
    public class UserStore : 
        IUserStore<ApplicationUserIdentity>, 
        IUserEmailStore<ApplicationUserIdentity>,
        IUserPasswordStore<ApplicationUserIdentity>
    {
        private readonly IAccountRepository _accountRepository;

        public UserStore(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public void Dispose()
        {
            // Nothing to do
        }

        public Task<string> GetUserIdAsync(ApplicationUserIdentity user, CancellationToken cancellationToken)
        {
            return Task.FromResult<string>(user.ApplicationUserId.ToString());
        }

        public Task<string> GetUserNameAsync(ApplicationUserIdentity user, CancellationToken cancellationToken)
        {
            return Task.FromResult<string>(user.Username);
        }

        public Task SetUserNameAsync(ApplicationUserIdentity user, string userName, CancellationToken cancellationToken)
        {
            user.Username = userName;
            return Task.CompletedTask;
        }

        public Task<string> GetNormalizedUserNameAsync(ApplicationUserIdentity user, CancellationToken cancellationToken)
        {
            return Task.FromResult<string>(user.NormalizedUsername);
        }

        public Task SetNormalizedUserNameAsync(ApplicationUserIdentity user, string normalizedName,
            CancellationToken cancellationToken)
        {
            user.NormalizedUsername = normalizedName;
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> CreateAsync(ApplicationUserIdentity user, CancellationToken cancellationToken)
        {
            return await _accountRepository.CreateAsync(user, cancellationToken);
        }

        public Task<IdentityResult> UpdateAsync(ApplicationUserIdentity user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> DeleteAsync(ApplicationUserIdentity user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<ApplicationUserIdentity> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<ApplicationUserIdentity> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            return await _accountRepository.GetByUserNameAsync(normalizedUserName, cancellationToken);
        }

        public Task SetEmailAsync(ApplicationUserIdentity user, string email, CancellationToken cancellationToken)
        {
            user.Email = email;
            return Task.CompletedTask;
        }

        public Task<string> GetEmailAsync(ApplicationUserIdentity user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(ApplicationUserIdentity user, CancellationToken cancellationToken)
        {
            return Task.FromResult<bool>(true);
        }

        public Task SetEmailConfirmedAsync(ApplicationUserIdentity user, bool confirmed, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task<ApplicationUserIdentity> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetNormalizedEmailAsync(ApplicationUserIdentity user, CancellationToken cancellationToken)
        {
            return Task.FromResult<string>(user.NormalizedEmail);
        }

        public Task SetNormalizedEmailAsync(ApplicationUserIdentity user, string normalizedEmail, CancellationToken cancellationToken)
        {
            user.NormalizedEmail = normalizedEmail;
            return Task.CompletedTask;
        }

        public Task SetPasswordHashAsync(ApplicationUserIdentity user, string passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;
            return Task.CompletedTask;
        }

        public Task<string> GetPasswordHashAsync(ApplicationUserIdentity user, CancellationToken cancellationToken)
        {
            return Task.FromResult<string>(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(ApplicationUserIdentity user, CancellationToken cancellationToken)
        {
            return Task.FromResult<bool>(user.PasswordHash != null);
        }
    }
}