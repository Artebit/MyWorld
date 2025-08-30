namespace MyWorld.Ifrastructure.Data
{
    public static class DbInitializer
    {
        public static Task SeedAsync(AppDbContext db) => Task.CompletedTask;
    }
}
