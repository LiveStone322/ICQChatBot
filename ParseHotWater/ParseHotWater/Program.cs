using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Npgsql;

namespace ParseHotWater
{
    class Program
    {
        public static string SConnStr = new NpgsqlConnectionStringBuilder()
        {
            Host = PrivateData.DbHost,
            Username = PrivateData.DbLogin,
            Database = PrivateData.DbName,
            Password = PrivateData.DbPassword,
            Port = PrivateData.DbPort,
            SslMode = SslMode.Require,
            TrustServerCertificate = true
        }.ConnectionString;

        static void Main(string[] args)
        {

            string[][] values = null;
            int i = 1;
            using (var sConn = new NpgsqlConnection(SConnStr))
            {
                sConn.Open();
                while (true)
                {
                    string temp = "";
                    var html = @"https://permkrai.ru/ajax/water/getPointsList.php?theme=&PAGEN_1=" + i + @"&AJAX_LOAD=&MORE=Y&query=&type=";
                    HtmlWeb web = new HtmlWeb();
                    var htmlDoc = web.Load(html);
                    foreach (var c in htmlDoc.DocumentNode.ChildNodes)
                        temp += c.InnerText + "\n";
                    i++;

                    values = temp               //ужасный код, но вижак не хочет сам его форматировать
                                    .Replace("Жилой дом", ", ")
                                    .Replace("Школа", ", ")
                                    .Replace("Детский сад", ", ")
                                    .Replace("Административное здание", ", ")
                                    .Replace("Прочее", ", ")
                                    .Replace("Медицинское учреждение", ", ")
                                    .Replace("Техникум", ", ")
                                    .Replace("Вуз", ", ")
                                    .Replace("Гимназия", ", ")
                                    .Replace("Детское учреждение", ", ")
                                    .Replace("Здравоохранение", ", ")
                                    .Replace("Колледж", ", ")
                                    .Replace("Отель", ", ")
                                    .Replace("Реабилитационный центр", ", ")
                                    .Replace("Училище", ", ")
                                    .Replace("Детский дом", ", ")
                                    .Replace("Лицей", ", ")
                                    .Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries)
                                    .Select(t => t.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries))
                                    .ToArray();


                    foreach (var e in values)
                    {
                        var sCommand = new NpgsqlCommand
                        {
                            Connection = sConn,
                            CommandText = @"insert into hot_water (city, street, building, duration)
                                            values (@p1, @p2, @p3, @p4)"
                        };
                        int kostil = 0;
                        var parsedElement = ParseElement(e);

                        if (e.Length < 4) continue;
                        sCommand.Parameters.AddWithValue("@p1", parsedElement[0]);
                        sCommand.Parameters.AddWithValue("@p2", parsedElement[1]);
                        sCommand.Parameters.AddWithValue("@p3", parsedElement[2]);
                        sCommand.Parameters.AddWithValue("@p4", parsedElement[3]);
                        sCommand.ExecuteNonQuery();
                        foreach (var c in e)
                            Console.Write(c + " ");
                        Console.WriteLine();
                        Console.WriteLine(parsedElement[0] + " " + parsedElement[1] + " " + parsedElement[2] + " " + parsedElement[3] + " added");
                    }
                    Console.WriteLine("Закончилась " + i + " страница.");
                }
            }
        }

        private static string[] ParseElement(string[] e)
        {
            string city, street, building, duration;
            int offset;
            if (e.Length < 4) return e;
            //сперва посмотрим, а не слились ли номер дома и длительность
            if (e[e.Length - 1][0] != 'с')
            {
                var idx = e[e.Length - 1].IndexOf('с');
                if (idx == -1) idx = e[e.Length - 1].IndexOf('c');
                if (idx == -1) return new string[] { };
                building = e[e.Length - 1].Substring(0, idx);
                duration = e[e.Length - 1].Substring(idx);
                street = e[e.Length - 2];
                city = e[e.Length - 3];
            }
            else
            {
                duration = e[e.Length - 1];
                building = e[e.Length - 2];
                street = e[e.Length - 3];
                city = e[e.Length - 4];
            }
            if (!(street.Contains("улица") ||        //если street - не улица, то отсутствует дом
                    street.Contains("площадь") ||
                    street.Contains("проспект") ||
                    street.Contains("бульвар") ||
                    street.Contains("переулок") ||
                    street.Contains("аллея") ||
                    street.Contains("шоссе")))
            {
                city = street;
                street = building;
                building = "";
                if (!(street.Contains("улица") ||        //еще раз проверяем. Ох этот говнокод
                    street.Contains("площадь") ||
                    street.Contains("проспект") ||
                    street.Contains("бульвар") ||
                    street.Contains("переулок") ||
                    street.Contains("аллея") ||
                    street.Contains("шоссе")))
                {
                    city = street;
                    street = "";
                }
            }
            city = TrimCity(city);
            street = TrimStreet(street);

            return new string[] { city, street, building.Trim(), duration };
            
        }

        private static string TrimStreet(string v)
        {
            return v.Replace("улица", "")
                    .Replace("площадь", "")
                    .Replace("проспект", "")
                    .Replace("бульвар", "")
                    .Replace("переулок", "")
                    .Replace("аллея", "")
                    .Replace("шоссе", "")
                    .Trim();
        }
        private static string TrimCity(string v)
        {
            return v
                                    .Replace("поселок городского типа", "")
                                    .Replace("посёлок городского типа", "")
                                    .Replace("рабочий поселок", "")
                                    .Replace("рабочий посёлок", "")
                                    .Replace("деревня", "")
                                    .Replace("посёлок", "")
                                    .Replace("поселок", "")
                                    .Replace("город", "")
                                    .Replace("село", "")
                                    .Trim();
        }
    }
}
