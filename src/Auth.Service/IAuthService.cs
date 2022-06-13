using Contract.Responses;

namespace Auth.Service;

public interface IAuthService
{
    Task<Login.Response> LoginUser(Contract.Requests.Login.Request loginRequest);
}