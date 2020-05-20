using System;
using System.Collections.Generic;
using System.Diagnostics;
using ICQ.Bot;
using ICQ.Bot.Args;

namespace ICQChatBot
{
    class Program
    {
        private readonly static IICQBotClient bot = new ICQBotClient(Token.GetToken);


        private static void DEBUGPrintData(List<string[]> data)
        {
            foreach (string[] row in data)
            {
                for (int i = 0; i < row.Length; i++)
                {
                    Console.Write(row[i]);
                }
                Console.WriteLine();
            }
        }


        static void Main(string[] args)
        {
            // Artrom test commit
            bot.OnMessage += BotOnMessageReceived;
            var me = bot.GetMeAsync().Result;

            bot.StartReceiving();
            Console.WriteLine($"Start listening to @{me.Nick}");

            // *** Testing DataBaseManager ***
            var dbManager = new DataBaseManager();
            var data = dbManager.GetData("Краснокамск", "Коммунальная");
            DEBUGPrintData(data);

            Console.ReadLine(); //остановка при нажатии Enter
            bot.StopReceiving();
        }

        private static void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            bot.SendTextMessageAsync(message.From.UserId, message.Text).Wait();
        }
    }
}
