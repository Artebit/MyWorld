// Infrastructure/Repositories/UserRepository.cs
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MyWorld.Domain.Interfaces.Repositories;
using MyWorld.Domain.Models;
using MyWorld.Infrastructure.Data;
using MyWorld.Models;

namespace MyWorld.Infrastructure.Repositories
{
    public class UserRepository : EfRepository<User>, IUserRepository
    {
        public UserRepository(AppDbContext ctx)
            : base(ctx)
        { }

        public User GetByEmail(string email) =>
            _ctx.Users
                .AsNoTracking()
                .FirstOrDefault(u => u.Email == email);
    }
}
