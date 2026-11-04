using Entity;
using EntityFramework.AppDbContext;
using Microsoft.Extensions.Logging;
using Perigon.PostgreSQL;
using Share.Implement;

namespace CommonMod.Managers;

/// <summary>
/// 演示用用户管理，密码明文比较，仅用于初始化示例
/// </summary>
public sealed class UserManager(DefaultDbContext dbContext, ILogger<UserManager> logger)
    : ManagerBase<DefaultDbContext, User>(dbContext, logger)
{
    public Task<User?> FindByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        return _dbSet
            .Where(user => user.UserName == userName && !user.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        return await InsertAsync(user, cancellationToken);
    }

    public override Task<bool> HasPermissionAsync(Guid id)
    {
        return Task.FromResult(true);
    }
}
