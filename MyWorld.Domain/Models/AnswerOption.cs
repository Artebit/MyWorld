using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWorld.Domain.Models
{
    public class AnswerOption
    {
        public Guid Id { get; set; }
        public Guid QuestionId { get; set; }
        public string Text { get; set; }
        public int Value { get; set; }
    }
}
