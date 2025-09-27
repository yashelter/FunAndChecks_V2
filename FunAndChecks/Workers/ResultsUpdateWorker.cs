using FunAndChecks.DTO;
using FunAndChecks.Models.Enums;
using FunAndChecks.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FunAndChecks.Workers;

using FunAndChecks.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


public class ResultsUpdateWorker : BackgroundService
{
    private readonly ILogger<ResultsUpdateWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _updateInterval = TimeSpan.FromMinutes(1); //TODO: lift to settings

    public ResultsUpdateWorker(ILogger<ResultsUpdateWorker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Results Update Worker is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Updating results cache...");
            try
            {
                await using var scope = _serviceProvider.CreateAsyncScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var cacheService = scope.ServiceProvider.GetRequiredService<IResultsCacheService>();

                var subjectIds = await dbContext.Subjects.Select(s => s.Id).ToListAsync(stoppingToken);

                foreach (var subjectId in subjectIds)
                {
                    var results = await CalculateResultsForSubject(dbContext, subjectId);
                    if (results != null)
                    {
                        cacheService.UpdateResults(subjectId, results);
                    }
                }
                
                _logger.LogInformation("Results cache updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating results cache.");
            }

            await Task.Delay(_updateInterval, stoppingToken);
        }
    }

    private async Task<SubjectResultsDto?> CalculateResultsForSubject(ApplicationDbContext context, int subjectId)
    {
        var subject = await context.Subjects
            .Include(s => s.Tasks.OrderBy(t => t.Name)) 
            .FirstOrDefaultAsync(s => s.Id == subjectId);

        if (subject == null) return null;

        var users = await context.Users
            .Where(u => u.UserRoles.Any(ur => ur.Role.Name == "User"))
            .Include(u => u.Group)
            .Include(u => u.Submissions)
            .ThenInclude(s => s.Admin)
            .ToListAsync();

        var taskHeaders = subject.Tasks.Select(t => new TaskHeaderDto(t.Id, t.Name)).ToList();

        var userResults = users.Select(user =>
        {
            int totalPoints = 0;
            var resultsDictionary = new Dictionary<int, ResultCellDto>();

            foreach (var task in subject.Tasks)
            {
                var lastSubmission = user.Submissions
                    .Where(s => s.TaskId == task.Id)
                    .OrderByDescending(s => s.SubmissionDate)
                    .FirstOrDefault();

                ResultCellDto cell;

                if (lastSubmission == null)
                {
                    cell = new ResultCellDto("", null, SubmissionStatus.NotSubmitted);
                }
                else
                {
                    string displayValue;
                    string? adminColor = lastSubmission.Admin?.Color;

                    switch (lastSubmission.Status)
                    {
                        case SubmissionStatus.Accepted:
                            displayValue = "+";
                            totalPoints += task.MaxPoints;
                            break;
                        case SubmissionStatus.Rejected:
                            displayValue = lastSubmission?.Admin?.Letter ?? "?";
                            break;
                        default: // NotSubmitted (хотя этот случай обработан выше)
                            displayValue = "";
                            adminColor = null;
                            break;
                    }
                    cell = new ResultCellDto(displayValue, adminColor, lastSubmission.Status);
                }
                resultsDictionary[task.Id] = cell;
            }

            return new UserResultDto(
                user.Id,
                $"{user.FirstName} {user.LastName}",
                user.Group?.Name ?? "N/A",
                totalPoints,
                resultsDictionary
            );
        }).ToList();

        var result = new SubjectResultsDto(subject.Id, subject.Name, taskHeaders, userResults);
        return result;
    }
}