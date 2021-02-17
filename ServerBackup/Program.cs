using System;

namespace ServerBackup
{
    class Program
    {
        static void Main(string[] args)
        {
            bool shouldAsk = true;

            while (shouldAsk)
            {
                Console.Write("Start the server or perform backup? [s/b]: ");
                string input = Console.ReadLine();

                switch (input)
                {
                    case "s":
                        {
                            ServerController.StartServer();
                            ServerController.BackupPrompt();
                            shouldAsk = false;
                            break;
                        }
                    case "b":
                        {
                            ServerController.BackupPrompt(false);
                            shouldAsk = false;
                            break;
                        }
                    default:
                        {
                            Console.WriteLine("Wrong input, retry");
                            break;
                        }
                }
            }
        }
    }
}
