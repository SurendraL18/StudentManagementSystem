using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StudentManagement.Application.Common.Interfaces;

namespace StudentManagement.Application.Authentication
{
    public class LoginService
    {
        private readonly IUserStore _userStore;
        private readonly IPasswordHasher _passwordHasher;

        private readonly ITokenService _tokenService;


        public LoginService(ITokenService tokenService,IUserStore userStore,IPasswordHasher passwordHasher)
        {
            _tokenService = tokenService;
            _userStore = userStore;
            _passwordHasher = passwordHasher;
        }


    }
}
