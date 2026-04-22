// PPECB.Domain/Common/BaseEntity.cs
namespace PPECB.Domain.Common;

public abstract class BaseEntity
{
    public int Id { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
}

public interface IUserOwned
{
    string UserId { get; set; }
}