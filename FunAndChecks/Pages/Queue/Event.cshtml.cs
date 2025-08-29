using FunAndChecks.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FunAndChecks.Pages.Queue;

public class Event : PageModel
{
    private readonly IHttpClientFactory _clientFactory;

    // Свойство для хранения полной информации об очереди
    public QueueDetailsDto? QueueDetails { get; set; }

    public Event(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    // Метод, который выполняется при GET-запросе, с параметром id из URL
    public async Task<IActionResult> OnGetAsync(int id)
    {
        var client = _clientFactory.CreateClient("ApiV1");
        try
        {
            // Запрашиваем детали конкретной очереди
            QueueDetails = await client.GetFromJsonAsync<QueueDetailsDto>($"/api/public/queue/{id}/details");
            if (QueueDetails == null)
            {
                return NotFound(); // Если очередь не найдена, возвращаем 404
            }

            return Page(); // Отображаем страницу
        }
        catch (HttpRequestException)
        {
            return NotFound();
        }
    }
}