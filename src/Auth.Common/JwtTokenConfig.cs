namespace Auth.Common;

public class JwtTokenConfig
{
    public JwtTokenConfig(string issuer, string audience, int accessTokenExpiration, string secret)
    {
        Issuer = issuer ?? throw new ArgumentNullException(nameof(issuer));
        Audience = audience ?? throw new ArgumentNullException(nameof(audience));
        AccessTokenExpiration = accessTokenExpiration;
        Secret = secret ?? throw new ArgumentNullException(nameof(secret));
    }

    public JwtTokenConfig()
    {
    }

    public string Issuer { get; set; }
    public string Audience { get; set; }
    public int AccessTokenExpiration { get; set; }
    public string Secret { get; set; }
}