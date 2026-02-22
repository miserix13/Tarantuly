namespace Tarantuly.SampleApp.Data;

public sealed class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public bool IsDeleted { get; set; }

    public byte[] RowVersion { get; set; } = [];
}
