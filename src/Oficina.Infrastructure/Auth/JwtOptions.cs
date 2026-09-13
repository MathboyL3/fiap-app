namespace Oficina.Infrastructure.Auth;

public sealed class JwtOptions
{
    public string Issuer { get; set; } = "Oficina.Api";
    public string Audience { get; set; } = "Oficina.Api";
    public string Secret { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;
}
