using Medical.GrpcService.Entities;

namespace Medical.GrpcService.Token;

public interface ITokenService
{
    Task<string> CreateToken(User user);
}