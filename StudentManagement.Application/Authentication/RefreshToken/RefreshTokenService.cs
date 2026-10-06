using StudentManagement.Application.Common.Exceptions;
using StudentManagement.Application.Common.Interfaces;
using StudentManagement.Domain.Enums;


namespace StudentManagement.Application.Authentication.RefreshToken;

public class RefreshTokenService
{
    private readonly IRefreshTokenHasher _refreshTokenHasher;
    private readonly IRefreshTokenStore _refreshTokenStore;
    private readonly IUserStore _userStore;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthenticationSettings _authenticationSettings;

    public RefreshTokenService(
        IRefreshTokenHasher refreshTokenHasher,
        IRefreshTokenStore refreshTokenStore,
        IUserStore userStore,
        ITokenService tokenService,
        IUnitOfWork unitOfWork,
        IAuthenticationSettings authenticationSettings)
    {
        _refreshTokenHasher = refreshTokenHasher;
        _refreshTokenStore = refreshTokenStore;
        _userStore = userStore;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
        _authenticationSettings = authenticationSettings;
    }

    public async Task<RefreshTokenResponse> RefreshAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            throw new InvalidCredentialsException();
        }

        var tokenHash =
            _refreshTokenHasher.Hash(request.RefreshToken);

        var storedRefreshToken =
            await _refreshTokenStore.GetByTokenAsync(
                tokenHash,
                cancellationToken);

        if (storedRefreshToken is null ||
            !storedRefreshToken.IsActive)
        {
            throw new InvalidCredentialsException();
        }

        var user =
            await _userStore.GetByIdAsync(
                storedRefreshToken.UserId,
                cancellationToken);

        if (user is null ||
            user.Status != UserStatus.Active)
        {
            throw new InvalidCredentialsException();
        }

        var tokenResult =
            _tokenService.GenerateTokens(
                user.Id,
                user.Email,
                user.Role);

        var newRefreshTokenHash =
            _refreshTokenHasher.Hash(
                tokenResult.RefreshToken);

        var newRefreshToken = new Domain.Entities.RefreshToken(
            user.Id,
            newRefreshTokenHash,
            TimeSpan.FromDays(
                _authenticationSettings.RefreshTokenExpirationDays));

        storedRefreshToken.Revoke();

        await _refreshTokenStore.AddAsync(
            newRefreshToken,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return new RefreshTokenResponse(
            tokenResult.AccessToken,
            tokenResult.RefreshToken,
            tokenResult.AccessTokenExpiresAt);
    }
}


