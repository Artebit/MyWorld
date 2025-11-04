namespace MyWorld.Domain.Models
{
    public class Question
    {
        public Guid Id { get; set; }
        public Guid DimensionId { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Order { get; set; }
        public QuestionType Type { get; set; }

        // навигации (по желанию):
        public Dimension? Dimension { get; set; }
        public List<AnswerOption> Options { get; set; } = new();
    }
}
