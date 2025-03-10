using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Npgsql;

using Xunit;
namespace ApiTests;
public class HousingControllerIntegrationTests 
{

    private readonly HttpClient _client = new();

    

    [Fact]
    public async Task GetHouses_ReturnsHouses_WhenDataExists()
    {
        // Act
        var response = await _client.GetAsync("https://45.130.214.139:5020/api/Housing/getgishouses?lat1=0&lat2=30&lon1=0&lon2=30");

        // Assert
        response.EnsureSuccessStatusCode(); // Проверяем, что статус код 200
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("A", content); // Проверяем, что данные содержат ожидаемый класс энергоэффективности
    }

    [Fact]
    public async Task GetHouses_ReturnsEmptyList_WhenNoData()
    {
        // Act
        var response = await _client.GetAsync("https://45.130.214.139:5020/api/Housing/getgishouses?lat1=100&lat2=200&lon1=100&lon2=200");

        // Assert
        response.EnsureSuccessStatusCode(); // Проверяем, что статус код 200
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("[]", content); // Проверяем, что возвращается пустой список
    }
}
