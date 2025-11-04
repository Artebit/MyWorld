using MyWorld.Domain.Models;

namespace MyWorld.Domain.Interfaces.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        User? GetByEmail(string email);
    }
}
