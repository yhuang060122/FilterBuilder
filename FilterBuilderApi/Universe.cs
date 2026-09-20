using Microsoft.EntityFrameworkCore;

[PrimaryKey(nameof(UniverseId), nameof(Version))]
public class UniverseVersion
{
    public UniverseVersion(Guid universeId, int version, DateTime createdAt, string description)
    {
        this.UniverseId = universeId;
        this.Version = version;
        this.CreatedAt = createdAt;
        this.Description = description;
    }

    public UniverseVersion() { }

    public Guid UniverseId { get; set; }
    public int Version { get; set; }

    public DateTime CreatedAt { get; set; }
    public string Description { get; set; } = string.Empty;
}