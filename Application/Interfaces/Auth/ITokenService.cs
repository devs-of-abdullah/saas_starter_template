using Domain.Entities.System;
using Domain.Entities.User;

namespace Application.Interfaces.Auth;

public interface ITokenService
{
    string GenerateAccessToken(UserEntity user);
    string GenerateSystemOwnerAccessToken(SystemOwnerEntity owner);
}
