using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MyWorld.Models
{
    public class Question
    {
        public int Id { get; set; }
        public string Text { get; set; } = null!;
        public string Hint { get; set; } = null!;
    }
}
