using FunAndChecks.Data;

namespace ApiTests;
using FunAndChecks;

using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;


// IAsyncLifetime нужен Xunit, чтобы асинхронно запустить и остановить контейнер
public class CustomApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // Создаем "рецепт" нашего контейнера PostgreSQL
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithDatabase("test_db")
        .WithUsername("test_user")
        .WithPassword("test_password")
        .Build();

    // Этот метод будет вызван Xunit ПЕРЕД запуском первого теста
    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync(); // Запускаем Docker-контейнер
    }

    // Этот метод будет вызван Xunit ПОСЛЕ завершения всех тестов
    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync(); // Останавливаем и удаляем контейнер
    }

    // Этот метод переопределяет конфигурацию сервисов вашего API
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // 1. Находим регистрацию оригинального DbContext
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            // 2. Если нашли - удаляем ее
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // 3. Регистрируем наш DbContext заново, но уже с ConnectionString от Testcontainers
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString());
            });
        });
    }
}