using Microsoft.AspNetCore.Mvc;
using Moq;
using Npgsql;
using dotSocialNetwork.Server.Controllers;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Xunit;
using dotSocialNetwork.Server.Models;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ApiTests;
public class HousingControllerTests : IClassFixture<WebApplicationFactory<HousingController>>
{
    [Fact]
    public void GetHouses_ReturnsOkResult_WithListOfHouses()
    {
        // Arrange
        var mockConnection = new Mock<NpgsqlConnection>();
        var mockCommand = new Mock<NpgsqlCommand>();
        var mockReader = new Mock<NpgsqlDataReader>();

        // Настройка моков для возврата данных
        mockReader.SetupSequence(r => r.Read())
            .Returns(true)
            .Returns(false); // Симулируем одну строку данных
        mockReader.Setup(r => r.GetString(0)).Returns("A");
        mockReader.Setup(r => r.GetDouble(1)).Returns(10.0);
        mockReader.Setup(r => r.GetDouble(2)).Returns(20.0);

        mockReader.Setup(r => r.GetDouble(It.IsAny<int>())).Returns((int column) =>
       {
           if (column == 1)
               return 10.0;
           else if (column == 2)
               return 20.0;
           throw new ArgumentOutOfRangeException(nameof(column));
       });
        mockConnection.Setup(c => c.CreateCommand()).Returns(mockCommand.Object);

        var controller = new HousingController();

        // Act
        var result = controller.GetHouses(0, 10, 0, 10);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var houses = Assert.IsType<List<HouseData>>(okResult.Value);
        Assert.Single(houses);
        Assert.Equal("A", houses[0].IhsEeClass);
        Assert.Equal(10.0, houses[0].HplGeoLongitude);
        Assert.Equal(20.0, houses[0].HplGeoLatitude);
    }

    [Fact]
    public void GetHouses_ReturnsEmptyList_WhenNoData()
    {
        // Arrange
        var mockConnection = new Mock<NpgsqlConnection>();
        var mockCommand = new Mock<NpgsqlCommand>();
        var mockReader = new Mock<NpgsqlDataReader>();

        // Настройка моков для возврата пустого результата
        mockReader.Setup(r => r.Read()).Returns(false);

        mockReader.Setup(r => r.GetDouble(It.IsAny<int>())).Returns((int column) =>
       {
           if (column == 1)
               return 10.0;
           else if (column == 2)
               return 20.0;
           throw new ArgumentOutOfRangeException(nameof(column));
       });
        mockConnection.Setup(c => c.CreateCommand()).Returns(mockCommand.Object);

        var controller = new HousingController();

        // Act
        var result = controller.GetHouses(0, 10, 0, 10);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var houses = Assert.IsType<List<HouseData>>(okResult.Value);
        Assert.Empty(houses);
    }
    private readonly WebApplicationFactory<HousingController> _factory;

    public HousingControllerTests(WebApplicationFactory<HousingController> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetHouses_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("api/Housing/getgishouses?lat1=0&lat2=90&lon1=-180&lon2=180&page=1&pageSize=100");

        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299
    }

    [Fact]
    public async Task GetHouses_ReturnsExpectedNumberOfHouses()
    {
        // Arrange
        var client = _factory.CreateClient();
        int expectedCount = 10;

        // Act
        var response = await client.GetAsync($"api/Housing/getgishouses?lat1=0&lat2=90&lon1=-180&lon2=180&page=1&pageSize={expectedCount}");
        var content = await response.Content.ReadAsStringAsync();
        var houses = System.Text.Json.JsonSerializer.Deserialize<List<HouseData>>(content);

        // Assert
        Assert.Equal(expectedCount, houses.Count);
    }

    [Fact]
    public async Task GetHouses_ReturnsCorrectHouseData()
    {
        // Arrange
        var client = _factory.CreateClient();
        double expectedLatitude = 50.45;
        double expectedLongitude = 30.52;

        // Act
        var response = await client.GetAsync("api/Housing/getgishouses?lat1=0&lat2=90&lon1=-180&lon2=180&page=1&pageSize=1");
        var content = await response.Content.ReadAsStringAsync();
        var houses = System.Text.Json.JsonSerializer.Deserialize<List<HouseData>>(content);
        var house = houses.FirstOrDefault();

        // Assert
        Assert.Equal(expectedLatitude, house.HplGeoLatitude);
        Assert.Equal(expectedLongitude, house.HplGeoLongitude);
    }

    [Fact]
    public async Task GetHouses_ReturnsEmptyListForInvalidParameters()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("api/Housing/getgishouses?lat1=90&lat2=0&lon1=-180&lon2=180&page=1&pageSize=100");
        var content = await response.Content.ReadAsStringAsync();
        var houses = System.Text.Json.JsonSerializer.Deserialize<List<HouseData>>(content);

        // Assert
        Assert.Empty(houses);
    }

    [Fact]
    public async Task GetHouses_ReturnsBadRequestForNegativePageSize()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("api/Housing/getgishouses?lat1=0&lat2=90&lon1=-180&lon2=180&page=1&pageSize=-1");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetHouses_ReturnsBadRequestForZeroPageSize()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("api/Housing/getgishouses?lat1=0&lat2=90&lon1=-180&lon2=180&page=1&pageSize=0");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetHouses_ReturnsEmptyListForOutOfRangePage()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("api/Housing/getgishouses?lat1=0&lat2=90&lon1=-180&lon2=180&page=100&pageSize=10");
        var content = await response.Content.ReadAsStringAsync();
        var houses = System.Text.Json.JsonSerializer.Deserialize<List<HouseData>>(content);

        // Assert
        Assert.Empty(houses);
    }

    [Fact]
    public async Task GetHouses_ReturnsEmptyListForNonexistentCoordinates()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("api/Housing/getgishouses?lat1=90&lat2=-90&lon1=180&lon2=-180&page=1&pageSize=10");
        var content = await response.Content.ReadAsStringAsync();
        var houses = System.Text.Json.JsonSerializer.Deserialize<List<HouseData>>(content);

        // Assert
        Assert.Empty(houses);
    }
}