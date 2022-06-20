using System.Net;
using System.Text.Json;
using Contract.Requests;
using RestSharp.Authenticators;
using Seeding;
using Xunit;

namespace RestSharp.Api.Tests;

public class TimeZoneControllerRestSharpTests
{
    private const string BaseAddress = "https://localhost:44364"; // address of Api hosted in Windows
    //private const string BaseAddress = "http://localhost"; // address of Api hosted as Docker image
    private readonly RestClient _client;

    public TimeZoneControllerRestSharpTests()
    {
        _client = new RestClient(BaseAddress);
        _client.Options.ThrowOnAnyError = false;
    }
    
    [Fact]
    public async Task CalculateDateTimeInTimeZone_Should_Return_DateTime_Recalculated_To_TimeZone()
    {
        // Arrange
        var requestBody = new Contract.Requests.TimeZone.Request
        {
            DateTimeInUtc = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc),
            TimeZoneId = "Asia/Baghdad"
        };

        var request = new RestRequest($"TimeZone/calculate").AddBody(requestBody);
        var token = await GetAuthenticationToken();
        _client.Authenticator = new JwtAuthenticator(token);

        // Act
        var httpResponse = await _client.ExecutePostAsync(request);

        // Assert response
        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
        
        var recalculatedDateTime = JsonSerializer.Deserialize<Contract.Responses.TimeZone.Response>(httpResponse.Content!,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(requestBody.TimeZoneId);
        var expectedDateTime = TimeZoneInfo.ConvertTimeFromUtc(requestBody.DateTimeInUtc, timeZoneInfo);
        
        Assert.Equal(expectedDateTime, recalculatedDateTime.DateTimeInTimeZone);
    }
    
    private async Task<string> GetAuthenticationToken()
    {
        var requestBody = new Login.Request()
        {
            Email = Users.AdminUser.Email,
            Password = Users.AdminPassword
        };

        var request = new RestRequest("Auth").AddBody(requestBody);

        var response = await _client.PostAsync<Contract.Responses.Login.Response>(request) ??
                       throw new ArgumentException(nameof(requestBody));

        return response.AccessToken;
    }
}