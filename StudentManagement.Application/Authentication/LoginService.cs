using StudentManagement.Application.Common.Exceptions;
using StudentManagement.Application.Common.Interfaces;
using StudentManagement.Domain.Entities;
using StudentManagement.Domain.Enums;

namespace StudentManagement.Application.Authentication
{
    public class LoginService
    {
        private readonly IUserStore _userStore;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;
        private readonly IRefreshTokenStore _refreshTokenStore;
        private readonly IRefreshTokenHasher _refreshTokenHasher;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthenticationSettings _authenticationSettings;

        public LoginService
            (ITokenService tokenService,
            IUserStore userStore,
            IPasswordHasher passwordHasher,
            IRefreshTokenStore refreshTokenStore,
            IUnitOfWork unitOfWork,
            IRefreshTokenHasher refreshTokenHasher,
            IAuthenticationSettings authenticationSettings)
        {
            _tokenService = tokenService;
            _userStore = userStore;
            _passwordHasher = passwordHasher;
            _refreshTokenStore = refreshTokenStore;
            _refreshTokenHasher = refreshTokenHasher;
            _unitOfWork = unitOfWork;
            _authenticationSettings = authenticationSettings;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest loginRequest, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(loginRequest);
            if (string.IsNullOrWhiteSpace(loginRequest.Email))
            {
                throw new InvalidCredentialsException();
            }

            if (string.IsNullOrWhiteSpace(loginRequest.Password))
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

            var hashedRefreshToken = _refreshTokenHasher.Hash(tokenResult.RefreshToken);

            var refreshTokenEntity = new RefreshToken
            (
               user.Id,
               hashedRefreshToken,
               TimeSpan.FromDays(_authenticationSettings.RefreshTokenExpirationDays)
            );

            await _refreshTokenStore.AddAsync(refreshTokenEntity, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new LoginResponse(
                tokenResult.AccessToken,
                tokenResult.RefreshToken,
                tokenResult.AccessTokenExpiresAt);
        }

    }
}
