using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentManagement.Application.Common.Exceptions
{
    public class DuplicateUserEmailException:Exception
    {
        public string Email { get; }

        public DuplicateUserEmailException (string email)
        {
            Email = email.Trim().ToLowerInvariant();
        }
    }
}
