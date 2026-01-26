


namespace EntityFramework.AppDbContext;

/// <summary>
/// default data access for main business
/// </summary>
/// <param name="options"></param>
public partial class DefaultDbContext(DbContextOptions<DefaultDbContext> options)
    : ContextBase(options)
{
    #region CMSMod
    
    
    #endregion

    #region SystemMod
    
    
    
    

    /// <summary>
    /// 菜单
    /// </summary>
    
    

    /// <summary>
    /// 权限组
    /// </summary>
    
    
    
    #endregion


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        

        
        
    }
}
