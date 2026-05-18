namespace Medical.Client.Interfaces;

public interface ITokenStorageService
{
    public string? GetToken();
    public void SetToken(string token);
    public void ClearToken();
}
