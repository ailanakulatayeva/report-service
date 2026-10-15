using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi;

namespace API.ApplicationRegistrations;

public static class SwaggerRegistration
{
    public static void AddSwagger(this WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            var profilerLinks = new List<string>();
            if (builder.Environment.IsDevelopment() && builder.Environment.IsEnvironment("Local"))
            {
                var appUrl = Environment.GetEnvironmentVariable("HOSTNAME").Contains("report-service")
                            ? "https://hse-report-service.dev.k8s.erg.kz"
                            : builder.Configuration["ASPNETCORE_URLS"]?.Split(';').FirstOrDefault();
                profilerLinks.Add($@"<a href=""{appUrl}/profiler/results"" target=""_blank"">🔍 MiniProfiler Results</a>");
                profilerLinks.Add($@"<a href=""{appUrl}/profiler/results-index"" target=""_blank"">📊 MiniProfiler Results Index</a>");
            }
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "HSE Report Service service API",
                Description = $@"HSE Сервис отчетов <br/>
                Host: {Environment.GetEnvironmentVariable("HOSTNAME")} <br/>
                Environment: <b>{builder.Environment.EnvironmentName}</b> <br/>
                Startup time : <b>{DateTime.Now.ToString("dd.MM.yyyy => HH:mm")}</b> <br/>                
                {string.Join("</br>", profilerLinks)}
                "
            });
        

            var securityScheme = new OpenApiSecurityScheme
            {
                Description = "Using the Authorization header with the Bearer scheme.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
            };

            options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, securityScheme);

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(JwtBearerDefaults.AuthenticationScheme, document)] = []
            });

            options.CustomSchemaIds(type =>
            {
                if (type.FullName == null)
                    return type.Name;

                if (type.IsGenericType)
                {
                    var genericTypeName = type.GetGenericTypeDefinition().FullName!
                        .Remove(type.GetGenericTypeDefinition().FullName!.IndexOf('`'))
                        .Replace('+', '_');

                    var genericArgNames = type.GetGenericArguments()
                        .Select(t => t.FullName!.Replace('+', '_'));

                    return $"{genericTypeName}Of{string.Join("And", genericArgNames)}";
                }
                return type.FullName.Replace('+', '_');
            });
        });
    }

    public static void SetupSwagger(this WebApplication app)
    {
        app.UseSwagger(options =>
        {
            options.RouteTemplate = "api/hse/report-service/swagger/{documentName}/swagger.json";
        });
        app.UseSwaggerUI(options =>
        {
            options.DocumentTitle = $"{app.Environment.ApplicationName}: {app.Environment.EnvironmentName}";
            options.RoutePrefix = "api/hse/report-service/swagger";
            options.DefaultModelsExpandDepth(0);
            options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        });
    }
}