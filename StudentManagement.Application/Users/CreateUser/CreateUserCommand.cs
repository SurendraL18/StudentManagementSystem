using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StudentManagement.Domain.Enums;

namespace StudentManagement.Application.Users.CreateUser
{
    public record CreateUserCommand(string Email,
    string Password,
    UserRole Role);
   
}
