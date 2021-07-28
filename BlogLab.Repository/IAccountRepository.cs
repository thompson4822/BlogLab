﻿using System;
using System.Threading;
using System.Threading.Tasks;
using BlogLab.Models.Account;
using Microsoft.AspNet.Identity;

namespace BlogLab.Repository
{
    public interface IAccountRepository
    {
        public Task<IdentityResult> CreateAsync(ApplicationUserIdentity user, 
            CancellationToken cancellationToken);

        public Task<ApplicationUserIdentity> GetByUserNameAsync(string normalizedUsername,
            CancellationToken cancellationToken);
    }
}