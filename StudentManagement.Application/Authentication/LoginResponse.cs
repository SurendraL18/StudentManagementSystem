using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentManagement.Application.Authentication
{
    public record LoginResponse( string AccessToken,string RefreshToken,DateTimeOffset AccessTokenExpiresAt);
}
