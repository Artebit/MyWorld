using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWorld.Domain.Models
{
    public class Dimension
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Question> Questions { get; set; } = new List<Question>();
    }
}
