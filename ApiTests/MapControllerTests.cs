using System.Net;
using System.Threading.Tasks;
using Xunit;
using dotSocialNetwork.Server.Controllers;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json.Linq;
namespace ApiTests;
public class MapControllerTests : IClassFixture<WebApplicationFactory<MapController>>
{
    private readonly WebApplicationFactory<MapController> _factory;

    public MapControllerTests(WebApplicationFactory<MapController> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetHouses_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/Map/gethouses?start=0&count=10");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetHouses_ReturnsCorrectNumberOfItems()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/Map/gethouses?start=0&count=10");
        var content = await response.Content.ReadAsStringAsync();
        var data = JObject.Parse(content);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(10, data.Count);
    }

    [Fact]
    public async Task HousesCount_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/Map/housescount");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task HousesCount_ReturnsCorrectCount()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/Map/housescount");
        var content = await response.Content.ReadAsStringAsync();
        var count = int.Parse(content);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.True(count >= 0); // Assuming there should be at least zero items
    }
}