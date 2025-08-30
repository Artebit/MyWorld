using MyWorld.Application.DTOs.Requests;
using MyWorld.Application.DTOs.Responses;

namespace MyWorld.Application.Interfaces
{
    public interface IQuestionService
    {
        Task<IReadOnlyList<QuestionDto>> GetAllAsync();
        Task<QuestionDto?> GetByIdAsync(Guid id);
        Task<QuestionDto> CreateAsync(CreateQuestionRequest req);
        Task UpdateAsync(UpdateQuestionRequest req);
        Task DeleteAsync(Guid id);
    }
}
