namespace ElectronicSignatureService
{
    public static class AppInit
    {
        public static string AppDataPath { get; private set; } = string.Empty;
        public static string ProductName { get; private set; } = "LIONS.sign";

        public static void Init(IWebHostEnvironment environment)
        {
            AppDataPath = Path.Combine(environment.ContentRootPath, "App_Data");
            if (!Directory.Exists(AppDataPath))
                Directory.CreateDirectory(AppDataPath);

            AppConfig.Init();
            Database.Init();
            Blockchain.Init();
        }
    }
}
