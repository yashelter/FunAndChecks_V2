namespace ApiTests;

using System.Net;
using FluentAssertions;
using Xunit;

// IClassFixture говорит xUnit: "Создай ОДИН экземпляр CustomApiFactory для ВСЕХ тестов в этом классе"
// Это значит, что контейнер запустится один раз для всего набора тестов, что очень быстро.
public class PublicApiTests : IClassFixture<CustomApiFactory>
{
    private readonly HttpClient _client;
    private readonly CustomApiFactory _factory;

    public PublicApiTests(CustomApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(); // Создаем HTTP-клиент, который отправляет запросы в наше API в памяти
    }

    [Fact]
    public async Task Get_PublicResultsTable_ShouldReturnOk()
    {
        // Arrange
        // (Здесь можно было бы добавить в тестовую БД какие-то данные, если бы требовалось)
        
        // Act
        var response = await _client.GetAsync("/api/public/results-table");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}