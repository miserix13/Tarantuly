namespace Tarantuly.Abstractions;

public interface IHasRowVersion
{
    byte[] RowVersion { get; set; }
}
