namespace Tarantuly.Abstractions;

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }

    DateTimeOffset? DeletedAtUtc { get; set; }
}
