namespace LoadVectorDbWithSk
{
    public class CollectionDefinition
    {
        public string Name { get; set; } = string.Empty;
        public string Structure { get; set; } = string.Empty;
        public string SearchDirectory { get; set; } = string.Empty;
        public string[] FileFilters { get; set; } = Array.Empty<string>();
        public string[] DirectoriesToIgnore { get; set; } = Array.Empty<string>();
        public string[] Questions { get; set; } = Array.Empty<string>();

    }

    public class CollectionsConfiguration
    {
        public List<CollectionDefinition> Collections { get; set; } = new();
    }

}
