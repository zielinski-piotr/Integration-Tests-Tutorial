using System.Net;
using System.Text.Json;
using Contract.Requests;
using RestSharp.Authenticators;
using Seeding;
using Xunit;
using House = Contract.Responses.House;

namespace RestSharp.Api.Tests;

public class HouseControllerRestSharpTests : IDisposable
{
    private const string BaseAddress = "https://localhost:44364";
    private readonly RestClient _client;

    public HouseControllerRestSharpTests()
    {
        _client = new RestClient(BaseAddress);
        _client.Options.ThrowOnAnyError = false;
    }

    #region GetById_Should

    [Fact]
    public async Task GetById_Should_Return_Valid_Entity()
    {
        // Arrange
        var request = new RestRequest($"House/{Houses.House1.Id}");
        var token = await GetAuthenticationToken();
        _client.Authenticator = new JwtAuthenticator(token);

        // Act
        var httpResponse = await _client.GetAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
        Assert.NotNull(httpResponse.Content);

        var house = JsonSerializer.Deserialize<House.Response>(httpResponse.Content!,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.Equal(Houses.House1.Id, house?.Id);
        Assert.Equal(Houses.House1.Address.Street, house?.Address.Street);
    }

    [Fact]
    public async Task GetById_Should_Return_404_When_Entity_NotFound()
    {
        var request = new RestRequest($"House/{Guid.NewGuid()}");
        var token = await GetAuthenticationToken();
        _client.Authenticator = new JwtAuthenticator(token);

        // Act
        var httpResponse = await _client.GetAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
    }

    #endregion

    #region Patch_Should

    [Fact]
    public async Task Patch_Should_Update_House_Successfully()
    {
        // Arrange
        const string newStreetName = "New street name";
        const string patch = "[{\"op\": \"replace\", \"path\": \"/address/street\", \"value\" : \"" +
                             newStreetName + "\"}]";
        //var houseId = Houses.House1.Id;
        var houseId = Houses.House8.Id;

        var request = new RestRequest($"House/{houseId}").AddBody(patch);
        var token = await GetAuthenticationToken();
        _client.Authenticator = new JwtAuthenticator(token);

        //requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/json-patch+json");

        // Act
        var httpResponse = await _client.PatchAsync(request);

        // Assert response
        Assert.Equal(HttpStatusCode.Accepted, httpResponse.StatusCode);

        // Assert persisted entity after Patch 
        var houseAfterPatch = await GetHouseAfterPatch(houseId);

        Assert.Equal(newStreetName, houseAfterPatch.Address.Street);
    }

    [Theory]
    [InlineData("add")]
    [InlineData("remove")]
    [InlineData("replace")]
    public async Task Patch_Should_Return_UnprocessableEntity_When_Trying_To_Patch_Not_Existing_Property(
        string operation)
    {
        // Arrange
        const string newStreetName = "New street name";
        var patch = "[{\"op\": \"" + operation + "\", \"path\": \"/address/length\", \"value\" : \"" +
                    newStreetName + "\"}]";

        var request = new RestRequest($"House/{Houses.House1.Id}").AddBody(patch);
        var token = await GetAuthenticationToken();
        _client.Authenticator = new JwtAuthenticator(token);

        // Act
        var exception = await Assert.ThrowsAsync<HttpRequestException>(async () => await _client.PatchAsync(request));

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, exception.StatusCode);
    }

    [Fact]
    public async Task Patch_Should_Add_Room_To_House_Successfully()
    {
        // Arrange
        const string patch =
            "[{\"op\": \"add\", \"path\": \"/rooms/-\", \"value\" : {\"name\" : \"Room1\", \"color\" : \"Red\", \"area\" : \"12.2\" }}]";
        var houseId = Houses.House1.Id;

        var request = new RestRequest($"House/{houseId}").AddBody(patch);
        var token = await GetAuthenticationToken();
        _client.Authenticator = new JwtAuthenticator(token);

        // Act
        var httpResponse = await _client.PatchAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Accepted, httpResponse.StatusCode);

        var afterPatch = await GetHouseAfterPatch(houseId);
        Assert.Single(afterPatch.Rooms);
    }

    [Fact]
    public async Task Patch_Should_Replace_Rooms_In_House_Successfully()
    {
        // Arrange
        const string patch =
            "[{\"op\": \"replace\", \"path\": \"/rooms\", \"value\" :" +
            " [{\"name\" : \"Room1\", \"color\" : \"Red\", \"area\" : \"12.2\" }, {\"name\" : \"Room2\", \"color\" : \"Yellow\", \"area\" : \"24.2\" }]}]";
        var houseId = Houses.House3.Id;

        var request = new RestRequest($"House/{houseId}").AddBody(patch);
        var token = await GetAuthenticationToken();
        _client.Authenticator = new JwtAuthenticator(token);

        // Act
        var httpResponse = await _client.PatchAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Accepted, httpResponse.StatusCode);

        var afterPatch = await GetHouseAfterPatch(houseId);
        Assert.Equal(2, afterPatch.Rooms.Count());
    }

    [Fact]
    public async Task Patch_Should_Return_UnprocessableEntity_When_Trying_To_Remove_Room_By_Id_Not_Index_In_Array()
    {
        // Arrange
        var houseId = Houses.House2.Id;
        var patch = "[{\"op\": \"remove\", \"path\": \"/rooms/" + Houses.House2.Rooms.Last().Id + "\", }]";

        var request = new RestRequest($"House/{houseId}").AddBody(patch);
        var token = await GetAuthenticationToken();
        _client.Authenticator = new JwtAuthenticator(token);

        // Act
        var exception = await Assert.ThrowsAsync<HttpRequestException>(async () => await _client.PatchAsync(request));

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, exception.StatusCode);
    }

    [Fact]
    public async Task Patch_Should_Return_404_When_House_Not_Exists()
    {
        // Arrange
        const string newStreetName = "New street name";
        const string patch = "[{\"op\": \"replace\", \"path\": \"/address/street\", \"value\" : \"" +
                             newStreetName + "\"}]";

        var request = new RestRequest($"House/{Guid.NewGuid()}").AddBody(patch);
        var token = await GetAuthenticationToken();
        _client.Authenticator = new JwtAuthenticator(token);

        // Act
        var httpResponse = await _client.PatchAsync(request);

        // Assert response
        Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
    }

    #endregion

    #region Update_Should

    [Fact]
    public async Task Update_Should_Return_404_When_House_Not_Exists()
    {
        // Arrange
        var requestBody = new Contract.Requests.House.Update
        {
            Area = 51,
            Color = "Azure",
            Name = "Some New Name"
        };

        var request = new RestRequest($"House/{Guid.NewGuid()}").AddBody(requestBody);
        var token = await GetAuthenticationToken();
        _client.Authenticator = new JwtAuthenticator(token);

        // Act
        var httpResponse = await _client.PutAsync(request);

        // Assert response
        Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
    }

    [Fact]
    public async Task Update_Should_Return_400_When_Request_Body_Invalid()
    {
        // Arrange
        var requestBody = new StringContent("{ 'Area' : 'Text' }");
        var request = new RestRequest($"House/{Houses.House1.Id}").AddBody(requestBody);
        var token = await GetAuthenticationToken();
        _client.Authenticator = new JwtAuthenticator(token);

        // Act
        var httpResponse = await _client.ExecutePutAsync(request);

        // Assert response
        Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);
    }
//[Fact(Skip="reason")]
    [Fact]
    public async Task Update_Should_Update_House_Successfully()
    {
        // Arrange
        var houseId = Houses.House10.Id;
        var requestBody = new Contract.Requests.House.Update
        {
            Area = 51,
            Color = "Azure",
            Name = "Some New Name"
        };

        var request = new RestRequest($"House/{houseId}").AddBody(requestBody);
        var token = await GetAuthenticationToken();
        _client.Authenticator = new JwtAuthenticator(token);

        // Act
        var httpResponse = await _client.PutAsync(request);

        // Assert response
        Assert.Equal(HttpStatusCode.Accepted, httpResponse.StatusCode);

        // Assert persisted entity after Patch 
        var houseAfterUpdate = await GetHouseAfterPatch(houseId);

        Assert.Equal(requestBody.Area, houseAfterUpdate.Area);
        Assert.Equal(requestBody.Color, houseAfterUpdate.Color);
        Assert.Equal(requestBody.Name, houseAfterUpdate.Name);
    }

    #endregion

    #region Delete_Should

    [Fact]
    public async Task Delete_Should_Return_404_When_House_Not_Exists()
    {
        var request = new RestRequest($"House/{Guid.NewGuid()}");
        var token = await GetAuthenticationToken();
        _client.Authenticator = new JwtAuthenticator(token);

        // Act
        var httpResponse = await _client.DeleteAsync(request);

        // Assert response
        Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_Should_Return_400_When_House_Empty_Guid_Passed()
    {
        var request = new RestRequest($"House/{Guid.Empty}");
        var token = await GetAuthenticationToken();
        _client.Authenticator = new JwtAuthenticator(token);

        // Act
        var exception = await Assert.ThrowsAsync<HttpRequestException>(async () => await _client.DeleteAsync(request));

        // Assert response
        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
    }

    [Fact]
    public async Task Delete_Should_DeleteEntity()
    {
        // Arrange
        var houseId = Houses.House10.Id;

        var request = new RestRequest($"House/{houseId}");
        var token = await GetAuthenticationToken();
        _client.Authenticator = new JwtAuthenticator(token);

        // Act
        var httpResponse = await _client.DeleteAsync(request);

        // Assert response
        Assert.Equal(HttpStatusCode.Accepted, httpResponse.StatusCode);

        // Assert entity not exists
        var getHttpResponse = await _client.GetAsync(request);

        // Assert response
        Assert.Equal(HttpStatusCode.NotFound, getHttpResponse.StatusCode);
    }

    #endregion

    #region Create_Should

    [Fact]
    public async Task Create_Should_CreateEntity()
    {
        // Arrange
        var requestBody = new Contract.Requests.House.Request()
        {
            Area = 51,
            Color = "Yellow",
            Name = "New Yellow House"
        };

        var request = new RestRequest($"House").AddBody(requestBody);
        var token = await GetAuthenticationToken();
        _client.Authenticator = new JwtAuthenticator(token);

        // Act
        var httpResponse = await _client.ExecutePostAsync(request);

        // Assert response
        Assert.Equal(HttpStatusCode.Created, httpResponse.StatusCode);

        // Assert
        var response = JsonSerializer.Deserialize<House.Response>(httpResponse.Content!,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.NotNull(response);

        Assert.Equal(requestBody.Area, response!.Area);
        Assert.Equal(requestBody.Name, response.Name);
        Assert.Equal(requestBody.Color, response.Color);
    }

    [Fact]
    public async Task Create_Should_Return_400_When_Name_IsEmpty()
    {
        // Arrange
        var requestBody = new Contract.Requests.House.Request()
        {
            Area = 51,
            Color = "Yellow",
            Name = ""
        };

        var request = new RestRequest($"House").AddBody(requestBody);
        var token = await GetAuthenticationToken();
        _client.Authenticator = new JwtAuthenticator(token);

        // Act
        var httpResponse = await _client.ExecutePostAsync(request);

        // Assert response
        Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);
    }

    [Fact]
    public async Task Create_Should_Return_400_When_Name_IsNull()
    {
        // Arrange
        var requestBody = new Contract.Requests.House.Request()
        {
            Area = 51,
            Color = "Yellow",
            Name = null
        };

        var request = new RestRequest($"House").AddBody(requestBody);
        var token = await GetAuthenticationToken();
        _client.Authenticator = new JwtAuthenticator(token);

        // Act
        var httpResponse = await _client.ExecutePostAsync(request);

        // Assert response
        Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);
    }

    [Fact]
    public async Task Create_Should_Return_400_When_Color_IsEmpty()
    {
        // Arrange
        var requestBody = new Contract.Requests.House.Request()
        {
            Area = 51,
            Color = "",
            Name = ""
        };

        var request = new RestRequest($"House").AddBody(requestBody);
        var token = await GetAuthenticationToken();
        _client.Authenticator = new JwtAuthenticator(token);

        // Act
        var httpResponse = await _client.ExecutePostAsync(request);

        // Assert response
        Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);
    }

    [Fact]
    public async Task Create_Should_Return_400_When_Color_IsNull()
    {
        // Arrange
        var requestBody = new Contract.Requests.House.Request()
        {
            Area = 51,
            Color = null,
            Name = "Some new name"
        };

        var request = new RestRequest($"House").AddBody(requestBody);
        var token = await GetAuthenticationToken();
        _client.Authenticator = new JwtAuthenticator(token);

        // Act
        var httpResponse = await _client.ExecutePostAsync(request);

        // Assert response
        Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);
    }

    #endregion

    public void Dispose() => _client.Dispose();

    private async Task<House.Response> GetHouseAfterPatch(Guid id)
    {
        var request = new RestRequest($"House/{id}");
        var httpResponse = await _client.GetAsync(request);

        var house = JsonSerializer.Deserialize<House.Response>(httpResponse.Content!,
            new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? throw new KeyNotFoundException();

        return house;
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