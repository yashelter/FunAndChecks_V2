using FunAndChecks.DTO;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FunAndChecks.Pages.Results;

public class Index : PageModel
{
    private readonly IHttpClientFactory _clientFactory;
    public List<SubjectDto> Subjects { get; set; } = new();

    public Index(IHttpClientFactory clientFactory) => _clientFactory = clientFactory;

    public async Task OnGetAsync()
    {
        var client = _clientFactory.CreateClient("ApiV1");
        Subjects = await client.GetFromJsonAsync<List<SubjectDto>>("/api/public/get-all/subjects") ?? new();
    }
}