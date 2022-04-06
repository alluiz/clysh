namespace CommandLineInterface
{
    public class Metadata
    {
        public string Title { get; set; }
        public string RootCommandName { get; set; }
        public string Description { get; set; }

        public Metadata(string title, string rootCommandName, string description)
        {
            Title = title;
            RootCommandName = rootCommandName;
            Description = description;
        }
    }
}