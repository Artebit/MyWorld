using MyWorld.Application.DTOs.Requests;
using MyWorld.Application.DTOs.Responses;
using MyWorld.Application.Interfaces;
using MyWorld.Domain.Interfaces;
using MyWorld.Domain.Models;

namespace MyWorld.Application.Services;

public class QuestionService : IQuestionService
{
    private readonly IUnitOfWork _uow;
    public QuestionService(IUnitOfWork uow) => _uow = uow;

    public Task<IReadOnlyList<QuestionDto>> GetAllAsync()
    {
        var list = _uow.Questions.GetAll().Select(Map).ToList().AsReadOnly();
        return Task.FromResult((IReadOnlyList<QuestionDto>)list);
    }

    public Task<QuestionDto?> GetByIdAsync(Guid id)
    {
        var entity = _uow.Questions.GetById(id);
        return Task.FromResult(entity is null ? null : Map(entity));
    }

    public async Task<QuestionDto> CreateAsync(CreateQuestionRequest request)
    {
        var entity = new Question
        {
            Id = Guid.NewGuid(),
            DimensionId = request.DimensionId,
            Text = request.Text,
            Order = request.Order,
            Type = request.Type,
        };

        _uow.Questions.Add(entity);
        await _uow.CommitAsync();
        return Map(entity);
    }

    public async Task UpdateAsync(UpdateQuestionRequest request)
    {
        var entity = _uow.Questions.GetById(request.Id);
        if (entity is null)
        {
            return;
        }

        entity.DimensionId = request.DimensionId;
        entity.Text = request.Text;
        entity.Order = request.Order;
        entity.Type = request.Type;
        _uow.Questions.Update(entity);
        await _uow.CommitAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = _uow.Questions.GetById(id);
        if (entity is null)
        {
            return;
        }

        _uow.Questions.Remove(entity);
        await _uow.CommitAsync();
    }

    private static QuestionDto Map(Question entity) => new(entity.Id, entity.DimensionId, entity.Text, entity.Order, entity.Type);
}
