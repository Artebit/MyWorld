namespace MyWorld.Domain.Models
{
    public class Response
    {
        public Guid Id { get; set; }
        public Guid SessionId { get; set; }
        public Guid QuestionId { get; set; }
        public int? AnswerValue { get; set; }
        public string? AnswerText { get; set; }
    }
}
