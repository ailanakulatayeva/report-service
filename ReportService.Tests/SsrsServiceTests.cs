using API.Configurations;
using API.Domain.Entities;
using API.Domain.Enums;
using API.DTOs.Responses;
using API.Errors.Exceptions;
using API.Infrastructure.ModelBuilders;
using API.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using Xunit;

namespace ReportService.Tests;

public class SsrsServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<HttpMessageHandler> _httpHandlerMock;
    private readonly SSRSService _sut;

    private static readonly Guid _userId = Guid.Empty;
    private const string BaseUrl = "https://ssrs-server";

    public SsrsServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _httpHandlerMock = new Mock<HttpMessageHandler>();

        var httpClient = new HttpClient(_httpHandlerMock.Object)
        {
            BaseAddress = new Uri(BaseUrl)
        };

        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(x => x.CreateClient(SSRSService.ClientName))
            .Returns(httpClient);

        var ssrsOptions = Options.Create(new SSRSConfigurations
        {
            BaseUrl = BaseUrl,
            AuthCredentials = new AuthCredentials
            {
                Username = "",
                Password = ""
            }
        });

        var loggerMock = new Mock<ILogger<SSRSService>>();

        _sut = new SSRSService(_context, httpClientFactoryMock.Object, ssrsOptions, loggerMock.Object);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task GetSsrsList_OnlyHseReports()
    {
        var catalog = new
        {
            Value = new[]
            {
                new { Id = "1", Name = "HSE Report", Path = "/HSE/Report1", Type = "Report" },
                new { Id = "2", Name = "Other Report", Path = "/OTHER/Report2", Type = "Report" }
            }
        };

        SetupHttpResponse($"{BaseUrl}/ReportsMES/api/v2.0/Reports", HttpStatusCode.OK, catalog);

        var result = await _sut.GetSsrsList();

        Assert.Single(result);
        Assert.Equal("/HSE/Report1", result[0].ReportPath);
    }

    [Fact]
    public async Task GetSsrsList_EmptyResponse()
    {
        var catalog = new { Value = Array.Empty<object>() };

        SetupHttpResponse($"{BaseUrl}/ReportsMES/api/v2.0/Reports", HttpStatusCode.OK, catalog);

        var result = await _sut.GetSsrsList();

        Assert.Empty(result);
    }

    [Fact]
    public async Task RenderReportAsync_ReportNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.RenderReportAsync(999, new Dictionary<string, string>(), CancellationToken.None));
    }

    [Fact]
    public async Task RenderReportAsync_SsrsServerError()
    {
        var report = new Report
        {
            Name = "Test",
            Description = "Test description",
            SsrsReportPath = "/HSE/Test",
            StatusCode = RecordStatus.Active,
            CreatedBy = _userId,
            UpdatedBy = _userId
        };
        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        SetupHttpResponseForAll(HttpStatusCode.InternalServerError, null);

        await Assert.ThrowsAsync<API.Errors.Exceptions.ProviderException>(() =>
            _sut.RenderReportAsync(report.Id, new Dictionary<string, string>(), CancellationToken.None));
    }

    [Fact]
    public async Task RenderReportAsync_PdfFile()
    {
        var report = new Report
        {
            Name = "Test",
            Description = "Test description",
            SsrsReportPath = "/HSE/Test",
            StatusCode = RecordStatus.Active,
            CreatedBy = _userId,
            UpdatedBy = _userId
        };
        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        var pdfBytes = new byte[] { 1, 2, 3, 4 };
        SetupHttpResponseForAll(HttpStatusCode.OK, null, pdfBytes);

        var result = await _sut.RenderReportAsync(report.Id, new Dictionary<string, string>(), CancellationToken.None);

        Assert.Equal("application/pdf", result.ContentType);
        Assert.Equal("Test.pdf", result.FileName);
        Assert.Equal(pdfBytes, result.FileContent);
    }

    private void SetupHttpResponse(string url, HttpStatusCode statusCode, object? body)
    {
        var json = body is not null ? JsonSerializer.Serialize(body) : string.Empty;
        var response = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(json)
        };

        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.ToString().StartsWith(url)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
    }

    private void SetupHttpResponseForAll(HttpStatusCode statusCode, object? body, byte[]? bytes = null)
    {
        HttpContent content = bytes is not null
            ? new ByteArrayContent(bytes)
            : new StringContent(body is not null ? JsonSerializer.Serialize(body) : string.Empty);

        var response = new HttpResponseMessage(statusCode) { Content = content };

        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
    }
}