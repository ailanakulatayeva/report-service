namespace API.Configurations;

public class SSRSConfigurations
{
    public string BaseUrl { get; set; }
    public AuthCredentials AuthCredentials { get; set; }    
}

public class AuthCredentials
{
    public string Username { get; set; }
    public string Password { get; set; }
}
