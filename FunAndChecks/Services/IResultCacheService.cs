using FunAndChecks.DTO;

namespace FunAndChecks.Services;


public interface IResultsCacheService
{
    /// Получить результаты из кеша
    SubjectResultsDto? GetResults(int subjectId);
    
    /// Обновить/добавить результаты в кеш
    void UpdateResults(int subjectId, SubjectResultsDto results);
}

