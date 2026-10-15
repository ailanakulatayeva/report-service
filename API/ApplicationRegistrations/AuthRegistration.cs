using ERG_MES.Authorization;

namespace API.ApplicationRegistrations;

public static class AuthRegistration
{
    public static void AddAuth(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication("Bearer")
            .AddJwtBearer("Keycloak", options =>
            {
                options.TokenValidationParameters.NameClaimType = "preferred_username";
            })
            .AddMesJwtBearer();
        builder.Services.AddMultiSchemeAuthorization(["Keycloak", MesJwtBearerDefaults.AuthenticationScheme]);
    }
}

