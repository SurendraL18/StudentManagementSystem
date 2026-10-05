namespace StudentManagement.Application.Common.Interfaces
{
    public interface IRefreshTokenHasher
    {
        string Hash(string refreshToken);
    }
}
