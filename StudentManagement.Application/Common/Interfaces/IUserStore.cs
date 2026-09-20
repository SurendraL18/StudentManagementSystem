using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StudentManagement.Domain.Entities;

namespace StudentManagement.Application.Common.Interfaces
{
    public interface IUserStore
    {
       Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

       Task AddAsync(User user, CancellationToken cancellationToken = default);

    }
}
