using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace STVDirector
{
    internal class Program
    {
        private static int matchID = 0;
        private static TimeZoneInfo moscowTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
        private static HubConnection hubConnection;

        static async Task Main(string[] args)
        {
            ClientMod.SendCommandToWindow($"echo хуй");
            ClientMod.SendCommandToWindow($"echo dick");

            try
            {
                hubConnection = new HubConnectionBuilder()
                    .WithUrl(new Uri($"http://localhost:4001/STVDirectorHub"))
                    .Build();

                hubConnection.On("GetMessage", (string message) =>
                {
                    Console.WriteLine(message);
                });

                hubConnection.On("GetMatchID", (int id) =>
                {
                    matchID = id;
                });

                hubConnection.On("GetMatchLogs", (List<MatchLog> logs) =>
                {
                    if (logs.Where(x => x.Message.Contains("<Match End>")).Any())
                    {
                        Task.Run(async () =>
                        {
                            await Task.Delay(70000);
                            Environment.Exit(0);
                        });
                    }

                    DateTime criteria = DateTime.Now.AddSeconds(-61);
                    DateTime value = TimeZoneInfo.ConvertTime(criteria, TimeZoneInfo.Local, moscowTimeZone);

                    var msg = logs.FirstOrDefault(x => x.DateTime >= value);

                    if (msg != null)
                    {
                        var match = Regex.Match(msg.Message, @"(.*?)<(.+?)>\s+killed\s+(.*?)<(.+?)>\s+with\s+weapon\s+<(.*?)>\s+(\<.+?\>) \[([\d:.]+)\]$");
                        if (match.Success)
                        {
                            string name = match.Groups[1].Value;
                            Console.WriteLine($"переключаемся на {name}");
                            ClientMod.SendCommandToWindow($"spec_player \"{name}\"");
                            ClientMod.SendCommandToWindow($"say \"{name}\"");
                        }
                    }
                });

                await hubConnection.StartAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            Console.WriteLine("Выберите сервер, который Вы хотите отслеживать:");

            Console.Write("Напишите ID сервера (1 - 6) - ");

            int server = int.Parse(Console.ReadLine());

            await hubConnection.InvokeAsync("Start", server);

            while (matchID == 0)
            {
                await Task.Delay(3000);
            }

            while (true)
            {
                await hubConnection.SendAsync("SendMatchLogs", matchID);

                await Task.Delay(3000);
            }
        }
    }
}
