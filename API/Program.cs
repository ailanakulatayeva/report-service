using API.ApplicationRegistrations;
using API.Controllers;
using API.Errors.ExceptionHandlers;
using API.Infrastructure.ModelBuilders;
using API.Services.Implementations;
using API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Providers.OrgSphere;
using System.Text.Json.Serialization;
 
var builder = WebApplication.CreateBuilder(args);

builder.RegisterOrgSphere();
builder.Services.AddUserContext();

builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddControllers(options =>
{
    options.Conventions.Add(new HyphenateRouteTokenTransformer());
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.AddSwagger();
builder.AddLogging();
builder.AddHealthChecks();
builder.AddAuth();
builder.AddSSRS();
builder.AddInfrastructureServices(builder.Configuration);
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddExceptionHandler<ProviderExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();


var app = builder.Build();


app.Services.ApplyMigrations();
app.UseExceptionHandler();
app.UseHsts();
app.UseHttpsRedirection();
app.SetupHealthChecks();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.SetupSwagger();
app.UseUserContextMiddleware();
app.MapControllers();
app.SetupLogging(builder.Environment);

app.Run();
