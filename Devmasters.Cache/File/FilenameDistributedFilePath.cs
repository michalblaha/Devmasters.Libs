namespace Devmasters.Cache.File
{
    public class FilenameDistributedFilePath
        : DistributedFilePath<string>
    {
        public FilenameDistributedFilePath(int hashLength)
            : this(hashLength, Devmasters.Config.GetWebConfigValue("PrilohyDataPath"))
        { }

        public FilenameDistributedFilePath(int hashLength, string root)
        : base(hashLength, root, (s) => { return s; })
        {
        }
    }
}
