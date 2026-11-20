using Entity;
using EntityFramework.AppDbContext;
using Perigon.PostgreSQL;

namespace CommonMod;

public static class InitModule
{
    public static async Task<bool> InitializeAsync(DefaultDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Users.Where(user => user.UserName == "user").AnyAsync(cancellationToken))
        {
            return false;
        }

        await context.Users.InsertAsync(
            new User
            {
                UserName = "user",
                Password = "123456",
                NickName = "Demo User",
            },
            cancellationToken);
        return true;
    }
}
