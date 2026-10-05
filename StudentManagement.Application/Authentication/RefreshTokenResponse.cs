namespace StudentManagement.Application.Authentication
{
    public record RefreshTokenResponse(string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAt);

}
