using System;
using ICQ.Bot;
using ICQ.Bot.Args;

namespace ICQChatBot
{

    class Program
    {
        private readonly static IICQBotClient bot = new ICQBotClient("001.2515800160.2031375345:752865044");

        static void Main(string[] args)
        {
            // Artrom test commit
            bot.OnMessage += BotOnMessageReceived;
            var me = bot.GetMeAsync().Result;

            bot.StartReceiving();
            Console.WriteLine($"Start listening to @{me.Nick}");

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
