namespace Domain.Interfaces
{
    public interface ICurrentUserService
    {
        string GetCurrentUserLogin();
        DateTime GetCurrentDateTime();
        bool IsAuthenticated();
    }
}