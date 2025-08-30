using MyWorld.Application.DTOs.Requests;
using MyWorld.Application.DTOs.Responses;
using MyWorld.Application.Interfaces;
using MyWorld.Domain.Interfaces;
using MyWorld.Domain.Models;

namespace MyWorld.Application.Services
{
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
            var e = _uow.Questions.GetById(id);
            return Task.FromResult(e is null ? null : Map(e));
        }

        public async Task<QuestionDto> CreateAsync(CreateQuestionRequest req)
        {
            var e = new Question
            {
                Id = Guid.NewGuid(),
                DimensionId = req.DimensionId,
                Text = req.Text,
                Order = req.Order,
                Type = req.Type
            };
            _uow.Questions.Add(e);
            await _uow.CommitAsync();
            return Map(e);
        }

        public async Task UpdateAsync(UpdateQuestionRequest req)
        {
            var e = _uow.Questions.GetById(req.Id);
            if (e is null) return;
            e.DimensionId = req.DimensionId;
            e.Text = req.Text;
            e.Order = req.Order;
            e.Type = req.Type;
            _uow.Questions.Update(e);
            await _uow.CommitAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var e = _uow.Questions.GetById(id);
            if (e is null) return;
            _uow.Questions.Remove(e);
            await _uow.CommitAsync();
        }

        private static QuestionDto Map(Question q) => new(q.Id, q.DimensionId, q.Text, q.Order, q.Type);
    }
}
