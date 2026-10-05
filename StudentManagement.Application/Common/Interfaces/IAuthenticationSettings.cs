namespace StudentManagement.Application.Common.Interfaces
{
    public interface IAuthenticationSettings
    {
        int RefreshTokenExpirationDays { get; }
    }
}
