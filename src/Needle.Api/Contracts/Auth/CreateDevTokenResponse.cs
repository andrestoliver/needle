namespace Needle.Api.Contracts.Auth;

public sealed record CreateDevTokenResponse(
    string AccessToken,
    DateTimeOffset ExpiresAt);