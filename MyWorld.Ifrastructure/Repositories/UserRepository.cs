using System.Linq;
using Microsoft.EntityFrameworkCore;
using MyWorld.Domain.Interfaces.Repositories;
using MyWorld.Domain.Models;           // ВАЖНО: Domain.Models
using MyWorld.Ifrastructure.Data;      // ВАЖНО: Ifrastructure

namespace MyWorld.Ifrastructure.Repositories
{
    public class UserRepository : EfRepository<User>, IUserRepository
    {
        public UserRepository(AppDbContext ctx) : base(ctx) { }

        public User? GetByEmail(string email) =>
            _ctx.Users.AsNoTracking().FirstOrDefault(u => u.Email == email);
    }
}
