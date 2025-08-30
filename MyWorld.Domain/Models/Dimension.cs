namespace MyWorld.Domain.Models
{
    public class Dimension
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public List<Question> Questions { get; set; } = new();
    }
}
