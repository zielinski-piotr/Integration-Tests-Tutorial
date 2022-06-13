using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Api.Tests.Factories;
using Contract.Requests;
using Seeding;
using Serialization.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using House = Contract.Responses.House;

namespace Api.Tests
{
    public sealed class HouseControllerTests : IDisposable
    {
        private readonly HttpClient _client;

        public HouseControllerTests()
        {
            var factory = new ApiFactory();

            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        #region GetById_Should

        [Fact]
        public async Task GetById_Should_Return_Valid_Entity()
        {
            //Arrange
            JsonSerializerOptions options = new(JsonSerializerDefaults.Web);

            var token = await GetAuthenticationToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            //Act
            var httpResponse = await _client.GetAsync($"House/{Houses.House1.Id}");
            await using var stream = await httpResponse.Content.ReadAsStreamAsync();

            //Assert
            Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);

            var house = await JsonSerializationHelper.DeserializeJsonFromStream<House.Response>(stream, options);

            Assert.Equal(Houses.House1.Id, house.Id);
            Assert.Equal(Houses.House1.Address.Street, house.Address.Street);
        }

        [Fact]
        public async Task GetById_Should_Return_404_When_Entity_NotFound()
        {
            var token = await GetAuthenticationToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            //Act
            var httpResponse = await _client.GetAsync($"House/{Guid.NewGuid()}");
            await using var stream = await httpResponse.Content.ReadAsStreamAsync();

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
        }

        #endregion

        #region Patch_Should

        [Fact]
        public async Task Patch_Should_Update_House_Successfully()
        {
            //Arrange
            const string newStreetName = "New street name";
            const string patch = "[{\"op\": \"replace\", \"path\": \"/address/street\", \"value\" : \"" +
                                 newStreetName + "\"}]";
            var houseId = Houses.House1.Id;

            var requestContent = new ByteArrayContent(Encoding.UTF8.GetBytes(patch));
            requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/json-patch+json");
            requestContent.Headers.ContentEncoding.Add("utf8");

            var token = await GetAuthenticationToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            //Act
            var httpResponse = await _client.PatchAsync($"House/{houseId}", requestContent);

            //Assert response
            Assert.Equal(HttpStatusCode.Accepted, httpResponse.StatusCode);

            //Assert persisted entity after Patch 
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
            //Arrange
            const string newStreetName = "New street name";
            var patch = "[{\"op\": \"" + operation + "\", \"path\": \"/address/length\", \"value\" : \"" +
                        newStreetName + "\"}]";
            var houseId = Houses.House1.Id;

            var requestContent = new ByteArrayContent(Encoding.UTF8.GetBytes(patch));
            requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/json-patch+json");
            requestContent.Headers.ContentEncoding.Add("utf8");

            var token = await GetAuthenticationToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            //Act
            var httpResponse = await _client.PatchAsync($"House/{houseId}", requestContent);

            //Assert
            Assert.Equal(HttpStatusCode.UnprocessableEntity, httpResponse.StatusCode);
        }

        [Fact]
        public async Task Patch_Should_Add_Room_To_House_Successfully()
        {
            //Arrange
            const string patch =
                "[{\"op\": \"add\", \"path\": \"/rooms/-\", \"value\" : {\"name\" : \"Room1\", \"color\" : \"Red\", \"area\" : \"12.2\" }}]";
            var houseId = Houses.House1.Id;

            var requestContent = new ByteArrayContent(Encoding.UTF8.GetBytes(patch));
            requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/json-patch+json");
            requestContent.Headers.ContentEncoding.Add("utf8");

            var token = await GetAuthenticationToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            //Act
            var httpResponse = await _client.PatchAsync($"House/{houseId}", requestContent);

            //Assert
            Assert.Equal(HttpStatusCode.Accepted, httpResponse.StatusCode);

            var afterPatch = await GetHouseAfterPatch(houseId);
            Assert.Single(afterPatch.Rooms);
        }

        [Fact]
        public async Task Patch_Should_Replace_Rooms_In_House_Successfully()
        {
            //Arrange
            const string patch =
                "[{\"op\": \"replace\", \"path\": \"/rooms\", \"value\" :" +
                " [{\"name\" : \"Room1\", \"color\" : \"Red\", \"area\" : \"12.2\" }, {\"name\" : \"Room2\", \"color\" : \"Yellow\", \"area\" : \"24.2\" }]}]";
            var houseId = Houses.House3.Id;

            var requestContent = new ByteArrayContent(Encoding.UTF8.GetBytes(patch));
            requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/json-patch+json");
            requestContent.Headers.ContentEncoding.Add("utf8");

            var token = await GetAuthenticationToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            //Act
            var httpResponse = await _client.PatchAsync($"House/{houseId}", requestContent);

            //Assert
            Assert.Equal(HttpStatusCode.Accepted, httpResponse.StatusCode);

            var afterPatch = await GetHouseAfterPatch(houseId);
            Assert.Equal(2, afterPatch.Rooms.Count());
        }

        [Fact]
        public async Task Patch_Should_Return_UnprocessableEntity_When_Trying_To_Remove_Room_By_Id_Not_Index_In_Array()
        {
            //Arrange
            var houseId = Houses.House2.Id;
            var patch = "[{\"op\": \"remove\", \"path\": \"/rooms/" + Houses.House2.Rooms.Last().Id + "\", }]";

            var requestContent = new ByteArrayContent(Encoding.UTF8.GetBytes(patch));
            requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/json-patch+json");
            requestContent.Headers.ContentEncoding.Add("utf8");

            var token = await GetAuthenticationToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            //Act
            var httpResponse = await _client.PatchAsync($"House/{houseId}", requestContent);

            //Assert
            Assert.Equal(HttpStatusCode.UnprocessableEntity, httpResponse.StatusCode);
        }

        [Fact]
        public async Task Patch_Should_Return_404_When_House_Not_Exists()
        {
            //Arrange
            const string newStreetName = "New street name";
            const string patch = "[{\"op\": \"replace\", \"path\": \"/address/street\", \"value\" : \"" +
                                 newStreetName + "\"}]";

            var requestContent = new ByteArrayContent(Encoding.UTF8.GetBytes(patch));
            requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/json-patch+json");
            requestContent.Headers.ContentEncoding.Add("utf8");

            var token = await GetAuthenticationToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            //Act
            var httpResponse = await _client.PatchAsync($"House/{Guid.NewGuid()}", requestContent);

            //Assert response
            Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
        }

        #endregion

        #region Update_Should

        [Fact]
        public async Task Update_Should_Return_404_When_House_Not_Exists()
        {
            //Arrange
            JsonSerializerOptions options = new(JsonSerializerDefaults.Web);

            var request = new Contract.Requests.House.Update
            {
                Area = 51,
                Color = "Azure",
                Name = "Some New Name"
            };

            var requestJson = JsonSerializer.SerializeToUtf8Bytes(request, options);

            var requestContent = new ByteArrayContent(requestJson);
            requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            requestContent.Headers.ContentEncoding.Add("utf8");

            var token = await GetAuthenticationToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            //Act
            var httpResponse = await _client.PutAsync($"House/{Guid.NewGuid()}", requestContent);

            //Assert response
            Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
        }

        [Fact]
        public async Task Update_Should_Return_400_When_Request_Body_Invalid()
        {
            //Arrange
            var requestContent = new StringContent("{ 'Area' : 'Text' }");
            requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            requestContent.Headers.ContentEncoding.Add("utf8");

            var token = await GetAuthenticationToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            //Act
            var httpResponse = await _client.PutAsync($"House/{Houses.House1.Id}", requestContent);

            //Assert response
            Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);
        }

        [Fact]
        public async Task Update_Should_Update_House_Successfully()
        {
            //Arrange
            JsonSerializerOptions options = new(JsonSerializerDefaults.Web);

            var houseId = Houses.House10.Id;

            var request = new Contract.Requests.House.Update
            {
                Area = 51,
                Color = "Azure",
                Name = "Some New Name"
            };

            var requestJson = JsonSerializer.SerializeToUtf8Bytes(request, options);

            var requestContent = new ByteArrayContent(requestJson);
            requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            requestContent.Headers.ContentEncoding.Add("utf8");

            var token = await GetAuthenticationToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            //Act
            var httpResponse = await _client.PutAsync($"House/{houseId}", requestContent);

            //Assert response
            Assert.Equal(HttpStatusCode.Accepted, httpResponse.StatusCode);

            //Assert persisted entity after Patch 
            var houseAfterUpdate = await GetHouseAfterPatch(houseId);

            Assert.Equal(request.Area, houseAfterUpdate.Area);
            Assert.Equal(request.Color, houseAfterUpdate.Color);
            Assert.Equal(request.Name, houseAfterUpdate.Name);
        }

        #endregion

        #region Delete_Should

        [Fact]
        public async Task Delete_Should_Return_404_When_House_Not_Exists()
        {
            var token = await GetAuthenticationToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            //Act
            var httpResponse = await _client.DeleteAsync($"House/{Guid.NewGuid()}");

            //Assert response
            Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
        }

        [Fact]
        public async Task Delete_Should_Return_400_When_House_Empty_Guid_Passed()
        {
            var token = await GetAuthenticationToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            //Act
            var httpResponse = await _client.DeleteAsync($"House/{Guid.Empty}");

            //Assert response
            Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);
        }

        [Fact]
        public async Task Delete_Should_DeleteEntity()
        {
            //Arrange
            var houseId = Houses.House10.Id;

            var token = await GetAuthenticationToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            //Act
            var httpResponse = await _client.DeleteAsync($"House/{houseId}");

            //Assert response
            Assert.Equal(HttpStatusCode.Accepted, httpResponse.StatusCode);

            //Assert entity not exists
            var getHttpResponse = await _client.GetAsync($"House/{houseId}");

            //Assert response
            Assert.Equal(HttpStatusCode.NotFound, getHttpResponse.StatusCode);
        }

        #endregion

        #region Create_Should

        [Fact]
        public async Task Create_Should_CreateEntity()
        {
            //Arrange
            JsonSerializerOptions options = new(JsonSerializerDefaults.Web);

            var request = new Contract.Requests.House.Request()
            {
                Area = 51,
                Color = "Yellow",
                Name = "New Yellow House"
            };

            var requestJson = JsonSerializer.SerializeToUtf8Bytes(request, options);

            var requestContent = new ByteArrayContent(requestJson);
            requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            requestContent.Headers.ContentEncoding.Add("utf8");

            var token = await GetAuthenticationToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            //Act
            var httpResponse = await _client.PostAsync($"House", requestContent);

            //Assert response
            Assert.Equal(HttpStatusCode.Created, httpResponse.StatusCode);

            //Assert response content
            var response = await httpResponse.Content
                .ReadFromJsonAsync<House.Response>();

            Assert.NotNull(response);

            Assert.Equal(request.Area, response!.Area);
            Assert.Equal(request.Name, response.Name);
            Assert.Equal(request.Color, response.Color);

            //Assert entity not exists
            var getHttpResponse = await _client.GetAsync($"House/{response.Id}");

            //Assert response
            Assert.Equal(HttpStatusCode.OK, getHttpResponse.StatusCode);
        }

        [Fact]
        public async Task Create_Should_Return_400_When_Name_IsEmpty()
        {
            //Arrange
            JsonSerializerOptions options = new(JsonSerializerDefaults.Web);

            var request = new Contract.Requests.House.Request()
            {
                Area = 51,
                Color = "Yellow",
                Name = ""
            };

            var requestJson = JsonSerializer.SerializeToUtf8Bytes(request, options);

            var requestContent = new ByteArrayContent(requestJson);
            requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            requestContent.Headers.ContentEncoding.Add("utf8");

            var token = await GetAuthenticationToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            //Act
            var httpResponse = await _client.PostAsync($"House", requestContent);

            //Assert response
            Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);
        }

        [Fact]
        public async Task Create_Should_Return_400_When_Name_IsNull()
        {
            //Arrange
            JsonSerializerOptions options = new(JsonSerializerDefaults.Web);

            var request = new Contract.Requests.House.Request()
            {
                Area = 51,
                Color = "Yellow",
                Name = null
            };

            var requestJson = JsonSerializer.SerializeToUtf8Bytes(request, options);

            var requestContent = new ByteArrayContent(requestJson);
            requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            requestContent.Headers.ContentEncoding.Add("utf8");

            var token = await GetAuthenticationToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            //Act
            var httpResponse = await _client.PostAsync($"House", requestContent);

            //Assert response
            Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);
        }

        [Fact]
        public async Task Create_Should_Return_400_When_Color_IsEmpty()
        {
            //Arrange
            JsonSerializerOptions options = new(JsonSerializerDefaults.Web);

            var request = new Contract.Requests.House.Request()
            {
                Area = 51,
                Color = "",
                Name = ""
            };

            var requestJson = JsonSerializer.SerializeToUtf8Bytes(request, options);

            var requestContent = new ByteArrayContent(requestJson);
            requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            requestContent.Headers.ContentEncoding.Add("utf8");

            var token = await GetAuthenticationToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            //Act
            var httpResponse = await _client.PostAsync($"House", requestContent);

            //Assert response
            Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);
        }

        [Fact]
        public async Task Create_Should_Return_400_When_Color_IsNull()
        {
            //Arrange
            JsonSerializerOptions options = new(JsonSerializerDefaults.Web);

            var request = new Contract.Requests.House.Request()
            {
                Area = 51,
                Color = null,
                Name = "Some new name"
            };

            var requestJson = JsonSerializer.SerializeToUtf8Bytes(request, options);

            var requestContent = new ByteArrayContent(requestJson);
            requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            requestContent.Headers.ContentEncoding.Add("utf8");
            
            var token = await GetAuthenticationToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            //Act
            var httpResponse = await _client.PostAsync($"House", requestContent);

            //Assert response
            Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);
        }

        #endregion

        public void Dispose() => _client.Dispose();

        private async Task<House.Response> GetHouseAfterPatch(Guid id)
        {
            JsonSerializerOptions options = new(JsonSerializerDefaults.Web);

            var httpResponse = await _client.GetAsync($"House/{id}");
            await using var stream = await httpResponse.Content.ReadAsStreamAsync();

            return await JsonSerializationHelper.DeserializeJsonFromStream<House.Response>(stream, options);
        }

        private async Task<string> GetAuthenticationToken()
        {
            //Arrange
            JsonSerializerOptions options = new(JsonSerializerDefaults.Web);

            var request = new Login.Request()
            {
                Email = Users.AdminUser.Email,
                Password = Users.AdminPassword
            };

            var requestJson = JsonSerializer.SerializeToUtf8Bytes(request, options);

            var requestContent = new ByteArrayContent(requestJson);
            requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            requestContent.Headers.ContentEncoding.Add("utf8");

            var httpResponse = await _client.PostAsync($"Auth", requestContent);

            return (await httpResponse.Content.ReadFromJsonAsync<Contract.Responses.Login.Response>())!.AccessToken;
        }
    }
}