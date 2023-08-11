namespace TSRACT.Models
{
    public class HfCacheItem
    {
        public string RepoId { get; set; }
        public string RepoType { get; set; }
        public string Revision { get; set; }
        public string SizeOnDisk { get; set; }
        public int NbFiles { get; set; }
        public string LastModified { get; set; }
        public string Refs { get; set; }
        public string LocalPath { get; set; }
    }
}
