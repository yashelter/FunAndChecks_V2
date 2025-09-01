using FunAndChecks.Services;
using Microsoft.AspNetCore.Mvc;

namespace FunAndChecks.Controllers;

/// <summary>
/// 
/// </summary>
/// <param name="cacheService"></param>
[ApiController]
[Route("api/cached/results")]
public class CachedResultsController(IResultsCacheService cacheService) : ControllerBase
{
    [HttpGet("subject/{subjectId}")]
    public IActionResult GetCachedResultsForSubject(int subjectId)
    {
        var results = cacheService.GetResults(subjectId);
        
        if (results == null)
        {
            return Accepted("Results are being generated. Please try again in a moment.");
        }

        return Ok(results);
    }
}