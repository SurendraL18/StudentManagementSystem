using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using StudentManagement.Application.Common.Interfaces;

namespace StudentManagement.Application.Users.CreateUser
{
    public class CreateUserService
    {
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUserStore _userStore;


        public CreateUserService(IPasswordHasher passwordHasher, IUserStore userStore)
        {
            _passwordHasher = passwordHasher;
            _userStore = userStore;
        }


        public async Task<CreateUserResponse> CreateAsync(CreateUserCommand command,CancellationToken cancellationToken= default)
        {
            var existingUser = await _userStore.GetByEmailAsync(command.Email, cancellationToken);
            if (existingUser != null) 
            { throw new InvalidOperationException($"A user with the email '{command.Email}' already exists."); }
        }

    }
}
