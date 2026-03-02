using Perigon.AspNetCore.Abstraction;

namespace Entity;

/// <summary>
/// 实体基类
/// </summary>
public abstract class EntityBase : ITenantEntityBase
{
    [Key]
#if NET8_0
    public Guid Id { get; set; } = Guid.NewGuid();
#else
    public Guid Id { get; set; } = Guid.CreateVersion7();
#endif
    public DateTimeOffset CreatedTime { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedTime { get; set; } = DateTimeOffset.UtcNow;
    public bool IsDeleted { get; set; }
    public Guid TenantId { get; set; }
}
