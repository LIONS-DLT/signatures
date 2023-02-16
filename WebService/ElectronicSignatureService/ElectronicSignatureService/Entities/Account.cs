namespace ElectronicSignatureService.Entities
{
    public class Account : DatabaseEntity
    {
        [DatabaseProperty]
        public string Name { get; set; } = string.Empty;


        [DatabaseProperty]
        public string EMail { get; set; } = string.Empty;

        [DatabaseProperty]
        public string PasswordHash { get; set; } = string.Empty;


    }
}
