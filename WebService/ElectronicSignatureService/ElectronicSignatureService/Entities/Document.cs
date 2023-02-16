namespace ElectronicSignatureService.Entities
{
    public class Document : DatabaseEntity
    {
        [DatabaseProperty]
        public string BlockchainMappingID { get; set; } = string.Empty;
        [DatabaseProperty]
        public string HashCode { get; set; } = string.Empty;

        [DatabaseProperty]
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

        [DatabaseProperty]
        public string Name { get; set; } = string.Empty;

        [DatabaseProperty]
        public string Filename { get; set; } = string.Empty;

        [DatabaseProperty]
        public int SignaturePlaceholderCount { get; set; } = 1;

        [DatabaseProperty]
        public string AccountID { get; set; } = string.Empty;
    }
}
