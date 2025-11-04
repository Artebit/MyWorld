namespace MyWorld.Domain.Models
{
    public class AnswerOption
    {
        public Guid Id { get; set; }
        public Guid QuestionId { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Value { get; set; }

        public Question? Question { get; set; }
    }
}
