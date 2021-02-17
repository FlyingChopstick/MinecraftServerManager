namespace ServerRestore
{
    class Program
    {
        static void Main(string[] args)
        {
            RestoreController.Startup();
            RestoreController.SelectBackup();
        }
    }
}
