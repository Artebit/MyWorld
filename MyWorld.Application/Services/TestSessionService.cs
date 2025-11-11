using System.Linq;
using MyWorld.Application.DTOs.Requests;
using MyWorld.Application.DTOs.Responses;
using MyWorld.Application.Interfaces;
using MyWorld.Domain.Interfaces;
using MyWorld.Domain.Models;

namespace MyWorld.Application.Services;

public class TestSessionService : ITestSessionService
{
    private readonly IUnitOfWork _uow;

    public TestSessionService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<TestSessionStartDto> StartSessionAsync(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User identifier is required.", nameof(userId));
        }

        var session = new TestSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            StartedAt = DateTime.UtcNow,
        };

        _uow.TestSessions.Add(session);
        await _uow.CommitAsync();

        return new TestSessionStartDto(session.Id, session.StartedAt);
    }

    public async Task<RecordedAnswerDto?> SubmitAnswerAsync(Guid sessionId, SubmitAnswerRequest request)
    {
        if (sessionId == Guid.Empty)
        {
            throw new ArgumentException("Session identifier is required.", nameof(sessionId));
        }

        if (request.QuestionId == Guid.Empty)
        {
            throw new ArgumentException("Question identifier is required.", nameof(request));
        }

        if (request.Value is null && string.IsNullOrWhiteSpace(request.Text))
        {
            throw new ArgumentException("Either a numeric value or a text answer must be provided.", nameof(request));
        }

        var session = _uow.TestSessions.GetById(sessionId);
        if (session is null)
        {
            return null;
        }

        var response = new Response
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            QuestionId = request.QuestionId,
            AnswerValue = request.Value,
            AnswerText = request.Text,
        };

        _uow.Responses.Add(response);
        await _uow.CommitAsync();

        return new RecordedAnswerDto(response.Id, response.SessionId, response.QuestionId, response.AnswerValue, response.AnswerText);
    }

    public async Task<TestSessionCompletionDto?> CompleteSessionAsync(Guid sessionId)
    {
        if (sessionId == Guid.Empty)
        {
            throw new ArgumentException("Session identifier is required.", nameof(sessionId));
        }

        var session = _uow.TestSessions.GetById(sessionId);
        if (session is null)
        {
            return null;
        }

        if (session.CompletedAt is null)
        {
            session.CompletedAt = DateTime.UtcNow;
            _uow.TestSessions.Update(session);
            await _uow.CommitAsync();
        }

        return new TestSessionCompletionDto(session.Id, session.CompletedAt);
    }

    public Task<IReadOnlyList<TestSessionResultItemDto>> GetResultAsync(Guid sessionId)
    {
        if (sessionId == Guid.Empty)
        {
            throw new ArgumentException("Session identifier is required.", nameof(sessionId));
        }

        var responses = _uow.Responses
            .GetAll()
            .Where(r => r.SessionId == sessionId && r.AnswerValue.HasValue)
            .ToList();

        var questions = _uow.Questions.GetAll().ToDictionary(q => q.Id, q => q.DimensionId);
        var dimensions = _uow.Dimensions.GetAll().ToDictionary(d => d.Id, d => d.Name);

        var result = responses
            .GroupBy(r => questions.TryGetValue(r.QuestionId, out var dimensionId) ? dimensionId : Guid.Empty)
            .Where(g => g.Key != Guid.Empty)
            .Select(g => new TestSessionResultItemDto(
                g.Key,
                dimensions.TryGetValue(g.Key, out var name) ? name : g.Key.ToString(),
                Math.Round(g.Average(r => (double)r.AnswerValue!.Value), 2)))
            .ToList()
            .AsReadOnly();

        return Task.FromResult<IReadOnlyList<TestSessionResultItemDto>>(result);
    }
}
