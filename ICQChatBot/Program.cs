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
        private static string NEEEEEEmsg = "Не понял Вашего сообщения. Введите, пожалуйста, еще раз, но другими словами";
        private static string tooLongMsg = "Вы ввели слишком длинную команду";
        private static string answerMsg = "Отключение воды в выбранном Вами доме запланировано ";
        private static string helpMsg = "Вот что я умею:\n" 
                                           + "/help - помощь по командам\n"
                                           + "/water - напишите это, чтобы узнать, когда в выбранном доме отключат горячую воду";
        private static string notFoundMsg = "Я не нашел точного совпадения, но нашел ";

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

            //Item1 - text, Item2 - bool error
            var naturalLanguageOutput = NaturalLanguageProcess(messageText);
            if (naturalLanguageOutput.Item2)
            {
                if (naturalLanguageOutput.Item1 != null)
                    messageText = naturalLanguageOutput.Item1;

                switch (messageText)
                {
                    case ("/help"):
                    case ("/start"):
                        outText = helpMsg;
                        break;
                    case ("/water"):
                        outText = StartWaterSequence(message.From.UserId, messageText);
                        break;
                    default:
                        outText = ProcessWaterSequence(message.From.UserId, messageText);
                        break;
                }
            }
            else outText = naturalLanguageOutput.Item1;
            bot.SendTextMessageAsync(message.From.UserId, outText).Wait();
        }

        private static Tuple<string, bool> NaturalLanguageProcess(string messageText)
        {
            //if we are being ddosed
            if (messageText.Length >= 256) return new Tuple<string, bool>(tooLongMsg, false);

            var message = messageText
                            .Replace(",", "")
                            .Replace(".", "")
                            .Replace("?", "")
                            .Replace("!", "")
                            .Replace(".", "")
                            .Replace(":", "")
                            .Split(' ');
            string result;
            if (message.Length != 1)
            {
                foreach (var e in message)
                    foreach (var a in message)
                    {
                        result = dbManager.SearchForEntityAndActivity(e, a);
                        if (result != null) return new Tuple<string, bool>(result, true);
                    }
            }
            else
            {
                result = dbManager.SearchForEntityAndActivity(message[0]);
                if (result != null) return new Tuple<string, bool>(result, true);
            }

            return new Tuple<string, bool>(null, true);
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
                    var c = dbManager.FindCity(messageText);
                    userInput.city = c;
                    outText = streetMsg;
                    botStates[userId] = State.WaitStreet;
                    if (c != messageText) outText = notFoundMsg + c + "\n" + outText;
                    break;
                case (State.WaitStreet):
                    var s = dbManager.FindStreet(messageText, userInput.city);
                    userInput.street = s;
                    outText = buildingMsg;
                    botStates[userId] = State.WaitBuilding;
                    if (s != messageText) outText = notFoundMsg + s + "\n" + outText;
                    break;
                case (State.WaitBuilding):
                    var b = dbManager.FindBuilding(messageText, userInput.city, userInput.street);
                    userInput.building = b;
                    outText = GetResult(userInput);
                    if (b != messageText) outText = notFoundMsg + b + " здание\n Возможно, нет данных для выбранного дома \n" + outText;

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
