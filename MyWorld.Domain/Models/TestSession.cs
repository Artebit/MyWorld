using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWorld.Domain.Models
{
    public class TestSession
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<Response> Responses { get; set; } = new List<Response>();
    }
}
