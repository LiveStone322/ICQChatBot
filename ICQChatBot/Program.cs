using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ICQ.Bot;
using ICQ.Bot.Args;
using ICQ.Bot.Types;

namespace ICQChatBot
{
    class Program
    {
        // *** Defining custom data types ***
        enum State { None, WaitCity, WaitStreet, WaitBuilding };
        class UserInput
        {
            public string city;
            public string street;
            public string building;
        }


        // *** Properties ***

        private readonly static IICQBotClient bot = new ICQBotClient(Token.GetToken);

        private static Dictionary<int, State> botStates;
        private static Dictionary<int, UserInput> usersInputs;

        // *** Messages ***

        private static string cityMsg = "Введите, пожалуйста, город";
        private static string streetMsg = "Введите, пожалуйста, улицу";
        private static string buildingMsg = "Введите, пожалуйста, дом";
        private static string failedMsg = "Мой адрес не дом и не улица,\nМой адрес - Советский Союз";
        private static string NEEEEEEmsg = "НИИИИИ";
        private static string answerMsg = "Отключение воды в выбранном Вами доме запланировано ";
        private static string helpMsg = "Вот что я умею:\n" 
                                           + "/help - помощь по командам\n"
                                           + "/water - напишите это, чтобы узнать, когда в выбранном доме отключат горячую воду";

        private static DataBaseManager dbManager;

        // *** Methods ***
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
            bot.OnMessage += BotOnMessageReceived;
            var me = bot.GetMeAsync().Result;

            botStates = new Dictionary<int, State>();
            usersInputs = new Dictionary<int, UserInput>();
            dbManager = new DataBaseManager();

            bot.StartReceiving();
            Console.WriteLine($"Start listening to @{me.Nick}");

            // *** Testing DataBaseManager ***
            //var data = dbManager.GetData("Краснокамск", "Коммунальная");
            //DEBUGPrintData(data);    

            Console.ReadLine(); //остановка при нажатии Enter
            bot.StopReceiving();
        }

        private static void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            string messageText = message.Text.ToLower();
            string outText = "Нипонял";

            switch (messageText)
            {
                case ("/help"):
                    outText = helpMsg;
                    break;
                case ("/water"):
                    outText = StartWaterSequence(message.From.UserId, messageText);
                    break;
                default:
                    outText = ProcessWaterSequence(message.From.UserId, messageText);
                    break;
            }

            bot.SendTextMessageAsync(message.From.UserId, outText).Wait();
        }

        private static string StartWaterSequence(int userId, string messageText)
        {
            if (!botStates.ContainsKey(userId))
            {
                botStates.Add(userId, State.WaitCity);
                usersInputs.Add(userId, new UserInput());
            }
            else
            {
                botStates[userId] = State.WaitCity;
            }

            return cityMsg;
        }

        private static string ProcessWaterSequence(int userId, string messageText)
        {
            if (!botStates.ContainsKey(userId))
                return NEEEEEEmsg;

            var userState = botStates[userId];
            var userInput = usersInputs[userId];
            string outText = "";

            switch (userState)
            {
                case (State.WaitCity):
                    userInput.city = messageText;
                    outText = streetMsg;
                    botStates[userId] = State.WaitStreet;
                    break;
                case (State.WaitStreet):
                    userInput.street = messageText;
                    outText = buildingMsg;
                    botStates[userId] = State.WaitBuilding;
                    break;
                case (State.WaitBuilding):
                    userInput.building = messageText;
                    outText = GetResult(userInput);

                    // Clearing up
                    botStates.Remove(userId);
                    usersInputs.Remove(userId);
                    break;
            }


            return outText;
        }

        private static string GetResult(UserInput userInput)
        {
            string result = "";

            Console.WriteLine($"Мой адрес: город = <{userInput.city}>, улица = <{userInput.street}>");

            var data = dbManager.GetData(userInput.city, userInput.street, userInput.building);
            int count = data.Count;

            if (count > 0)
            {
                result = answerMsg + data[0][4];
            }
            else
            {
                result = failedMsg;
            }

            return result;
        }
    }
}
