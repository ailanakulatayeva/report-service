using System.Net;
using API.Configurations;
using API.Services.Implementations;
using API.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace API.ApplicationRegistrations;

public static class SSRSRegistration
{
    public static void AddSSRS(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<SSRSConfigurations>(builder.Configuration.GetSection("SSRSConfigurations"));
        builder.Services.AddHttpClient(SSRSService.ClientName, (serviceProvider, client) =>
        {
            var config = serviceProvider.GetRequiredService<IOptions<SSRSConfigurations>>().Value;
            client.BaseAddress = new Uri(config.BaseUrl);
        })
        .ConfigurePrimaryHttpMessageHandler((serviceProvider) =>
        {
            var config = serviceProvider.GetRequiredService<IOptions<SSRSConfigurations>>().Value;

            return new HttpClientHandler()
            {
                Credentials = new NetworkCredential(config.AuthCredentials.Username, config.AuthCredentials.Password)
            };
        });
        builder.Services.AddScoped<ISSRSService, SSRSService>();
    }
}