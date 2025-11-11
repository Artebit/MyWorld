using MyWorld.Application.DTOs.Requests;
using MyWorld.Application.DTOs.Responses;

namespace MyWorld.Application.Interfaces;

public interface ITestSessionService
{
    Task<TestSessionStartDto> StartSessionAsync(Guid userId);
    Task<RecordedAnswerDto?> SubmitAnswerAsync(Guid sessionId, SubmitAnswerRequest request);
    Task<TestSessionCompletionDto?> CompleteSessionAsync(Guid sessionId);
    Task<IReadOnlyList<TestSessionResultItemDto>> GetResultAsync(Guid sessionId);
}
