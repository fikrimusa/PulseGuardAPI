using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using PulseGuard.Api.Data;
using PulseGuard.Api.DTOs;

namespace PulseGuard.Api.Tests;

public sealed class ApiIntegrationTests : IClassFixture<ApiIntegrationTests.ApiFactory>
{
    private readonly HttpClient _client;

    public ApiIntegrationTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_returns_success()
    {
        var response = await _client.GetAsync("/api/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Register_and_login_return_tokens_and_monitors_require_authentication()
    {
        var email = $"test-{Guid.NewGuid():N}@pulseguard.test";
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new { email, password = "SecurePass123!" });
        var registeredUser = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new { email, password = "SecurePass123!" });
        var loggedInUser = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        var monitorsResponse = await _client.GetAsync("/api/monitors");

        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        registeredUser!.AccessToken.Should().NotBeNullOrWhiteSpace();
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        loggedInUser!.AccessToken.Should().NotBeNullOrWhiteSpace();
        monitorsResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    public sealed class ApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
                services.RemoveAll<IHostedService>();
                services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("api-tests"));
            });
        }
    }
}
