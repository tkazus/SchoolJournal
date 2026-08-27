using System;

namespace SchoolJournal.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "SchoolJournal Server";
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("ШКОЛЬНЫЙ ЖУРНАЛ - СЕРВЕР");
            Console.ResetColor();
            Console.WriteLine();

            var server = new TcpServer(8888);

            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                Console.WriteLine("\nЗавершение работы сервера");
                server.Stop();
                Environment.Exit(0);
            };

            try
            {
                server.Start();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ошибка: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}