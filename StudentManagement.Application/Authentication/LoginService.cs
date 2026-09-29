using StudentManagement.Application.Common.Exceptions;
using StudentManagement.Application.Common.Interfaces;
using StudentManagement.Domain.Enums;

namespace StudentManagement.Application.Authentication
{
    public class LoginService
    {
        private readonly IUserStore _userStore;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;


        public LoginService(ITokenService tokenService, IUserStore userStore, IPasswordHasher passwordHasher)
        {
            _tokenService = tokenService;
            _userStore = userStore;
            _passwordHasher = passwordHasher;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest loginRequest, CancellationToken cancellationToken = default)
        {

            if (string.IsNullOrWhiteSpace(loginRequest.Email))
            {
                throw new InvalidCredentialsException();
            }

            var normalizedEmail = loginRequest.Email.Trim().ToLowerInvariant();


            var user = await _userStore.GetByEmailAsync(normalizedEmail, cancellationToken);


            if (user is null)
            {
                throw new InvalidCredentialsException();
            }

            var isPasswordValid = _passwordHasher.Verify(loginRequest.Password, user.PasswordHash);


            if (!isPasswordValid)
            {
                throw new InvalidCredentialsException();
            }


            if (user.Status != UserStatus.Active)
            {
                throw new InvalidCredentialsException();
            }


            var tokenResult = _tokenService.GenerateTokens(
              user.Id,
              user.Email,
              user.Role);

            // 8. Map internal TokenResult tokens safely into the public use-case LoginResponse contract
            return new LoginResponse(
                tokenResult.AccessToken,
                tokenResult.RefreshToken,
                tokenResult.AccessTokenExpiresAt);
        }

    }
}
