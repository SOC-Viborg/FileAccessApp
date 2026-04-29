namespace FileAccessApp.Domain
{
    public class FileDomain
    {
        public string Name { get; set; } = string.Empty;
        public bool IsDirectory { get; set; }

        public FileDomain() { }

        public FileDomain(string name, bool isDirectory)
        {
            Name = name;
            IsDirectory = isDirectory;
        }
    }
}
