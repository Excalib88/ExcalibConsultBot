namespace ExcalibConsultBot.Services;

public class TokenValidator
{
    private readonly IConfiguration _configuration;

    public TokenValidator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public bool Validate(string token)
    {
        var appToken = _configuration["Token"];

        return !string.IsNullOrWhiteSpace(appToken) && token == appToken;
    }
}