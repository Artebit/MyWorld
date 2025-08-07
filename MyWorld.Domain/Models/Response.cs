using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWorld.Domain.Models
{
    public class Response
    {
        public Guid Id { get; sett }
        public Guid SessionId { get; set; }
        public Guid QuestionId { get; set; }
        public int? AnswerValue { get; set; }
        public string AnswerText { get; set; }
    }

}
