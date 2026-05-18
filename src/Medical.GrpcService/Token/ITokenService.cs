using Medical.GrpcService.Entities;

namespace Medical.GrpcService.Token;

public interface ITokenService
{
    public Task<string> CreateToken(User user);
}
