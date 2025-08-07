namespace MyWorld.Models
{
    public class ExerciseSession
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string Title { get; set; } = "Колесо баланса";

        public ICollection<Answer> Answers { get; set; } = new List<Answer>();
    }
}
