using FunAndChecks.DTO;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FunAndChecks.Pages.Queue;

public class Index : PageModel
{
    private readonly IHttpClientFactory _clientFactory;
        
    // Свойство для хранения списка событий, которое будет доступно в верстке
    public List<QueueEventDto> QueueEvents { get; set; } = new();

    public Index(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    // Метод, который выполняется при GET-запросе к странице
    public async Task OnGetAsync()
    {
        var client = _clientFactory.CreateClient("ApiV1");
        try
        {
            // Запрашиваем список всех активных событий очереди
            QueueEvents = await client.GetFromJsonAsync<List<QueueEventDto>>("/api/public/queue/events") ?? new();
        }
        catch (HttpRequestException ex)
        {
            // Здесь можно добавить логирование ошибки
            // В данном примере просто оставляем список пустым
            QueueEvents = new List<QueueEventDto>();
        }
    }
}
