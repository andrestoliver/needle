namespace Needle.Api.Contracts.Auth;

public sealed record CreateDevTokenRequest(
    Guid UserId,
    string DisplayName);