using System.Linq;
using MyWorld.Domain.Interfaces.Repositories;
using MyWorld.Domain.Models;
using MyWorld.Infrastructure.Data;

namespace MyWorld.Infrastructure.Repositories;

public class UserRepository : EfRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext ctx) : base(ctx)
    {
    }

    public User? GetByEmail(string email) =>
        _ctx.Users.FirstOrDefault(u => u.Email == email);
}
