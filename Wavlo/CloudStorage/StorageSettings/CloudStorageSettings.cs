namespace Wavlo.CloudStorage.StorageSettings
{
    public class CloudStorageSettings
    {
        public const string AzureStorage = "BlobStorage";
        public string ConnectionString { get; set; }
        public ContainersOptions Containers { get; set; }
    }
}
