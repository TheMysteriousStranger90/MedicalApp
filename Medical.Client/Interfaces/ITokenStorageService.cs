namespace Medical.Client.Interfaces;

public interface ITokenStorageService
{
    string? GetToken();
    void SetToken(string token);
    void ClearToken();
}