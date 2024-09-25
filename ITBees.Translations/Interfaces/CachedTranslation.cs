namespace ITBees.Translations.Interfaces;

public class CachedTranslation
{
    public bool Found { get; set; }
    public string Value { get; set; }
    public bool HasReplicableFields { get; set; }
    public string ReplicableFields { get; set; }
}