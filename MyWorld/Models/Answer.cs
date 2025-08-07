using System.ComponentModel.DataAnnotations;
namespace MyWorld.Models
{
    public class Answer
    {
        [Key]
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public string UserId { get; set; } = null!;
        public string Response { get; set; } = null!;
        public DateTime LastUpdated { get; set; }
        public Question? question { get; set; }
        public int SessionId { get; set; }
        public ExerciseSession? Session { get; set; }
    }
}
