namespace CAT.Utils
{
    public class FileSystem : IFileSystem
    {
        public bool DirectoryExists(string path)
        {
            return System.IO.Directory.Exists(path);
        }
    }
}
