using FunAndChecks.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FunAndChecks.Pages.Results;


public class Subject : PageModel
{
    private readonly IHttpClientFactory _clientFactory;
    public SubjectResultsDto? Results { get; set; }

    public Subject(IHttpClientFactory clientFactory) => _clientFactory = clientFactory;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var client = _clientFactory.CreateClient("ApiV1");
        Results = await client.GetFromJsonAsync<SubjectResultsDto>($"/api/cached/results/subject/{id}");
        if (Results == null) return NotFound();
        return Page();
    }


    public async Task<IActionResult> OnGetRefreshTableAsync(int id)
    {
        var client = _clientFactory.CreateClient("ApiV1");
        var results = await client.GetFromJsonAsync<SubjectResultsDto>($"/api/cached/results/subject/{id}");
        if (results == null)
        {
            return NotFound();
        }
        return Partial("_ResultsTable", results);
    }
}