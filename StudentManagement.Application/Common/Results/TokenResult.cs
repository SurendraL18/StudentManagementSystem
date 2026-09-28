using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StudentManagement.Application.Common.Interfaces;
using StudentManagement.Domain.Enums;

namespace StudentManagement.Application.Common.Results
{
    public record TokenResult(string AccessToken,string RefreshToken,DateTimeOffset AccessTokenExpiresAt);

}
