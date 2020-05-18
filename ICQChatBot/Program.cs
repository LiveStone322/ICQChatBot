using System;
using ICQ.Bot;
using ICQ.Bot.Args;

namespace ICQChatBot
{

    class Program
    {
        private readonly static IICQBotClient bot = new ICQBotClient("[BOT_ID_FROM_ICQ_METABOT]");

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
