using FunAndChecks.DTO;

namespace FunAndChecks.Services;

using System.Collections.Concurrent;

public class ResultsCacheService : IResultsCacheService
{
    private readonly ConcurrentDictionary<int, SubjectResultsDto> _cache = new();

    public SubjectResultsDto? GetResults(int subjectId)
    {
        _cache.TryGetValue(subjectId, out var results);
        return results;
    }

    public void UpdateResults(int subjectId, SubjectResultsDto results)
    {
        _cache[subjectId] = results;
    }
}