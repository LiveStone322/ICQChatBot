using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Npgsql;

namespace ICQChatBot
{
    class DataBaseManager
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

        public List<string[]> GetData()
        {
            string commandText = @"select * from hot_water";
            Dictionary<string, string> parameters = new Dictionary<string, string> { };

            var fetchedData = FetchData(commandText, parameters);
            return fetchedData;
        }

        public List<string[]> GetData(string city)
        {
            city = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(city);

            string commandText = @"select * from hot_water
                                    where city = @city";
            Dictionary<string, string> parameters = new Dictionary<string, string>
            {
                { "city", city }
            };

            var fetchedData = FetchData(commandText, parameters);
            return fetchedData;
        }

        public List<string[]> GetData(string city, string street)
        {
            city = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(city);
            street = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(street);

            string commandText = @"select * from hot_water
                                    where (city = @city) and (street = @street)";
            Dictionary<string, string> parameters = new Dictionary<string, string>
            {
                { "city", city },
                { "street", street }
            };

            var fetchedData = FetchData(commandText, parameters);
            return fetchedData;
        }

        public List<string[]> GetData(string city, string street, string building)
        {
            city = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(city);
            street = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(street);

            string commandText = @"select * from hot_water
                                    where (city = @city) and (street = @street) and (building = @building)";
            Dictionary<string, string> parameters = new Dictionary<string, string>
            {
                { "city", city },
                { "street", street },
                { "building", building }
            };

            var fetchedData = FetchData(commandText, parameters);
            return fetchedData;
        }


        private List<string[]> FetchData(string commandText, Dictionary<string, string> parameters)
        {
            List<string[]> fetchedData = new List<string[]>();

            using (var sConn = new NpgsqlConnection(SConnStr))
            {
                sConn.Open();
                var sCommand = new NpgsqlCommand
                {
                    Connection = sConn,
                    CommandText = commandText
                };

                foreach (string parameter in parameters.Keys)
                {
                    string p = "@" + parameter;
                    sCommand.Parameters.AddWithValue(p, parameters[parameter]);
                }

                var dataReader = sCommand.ExecuteReader();
                string[] array = new string[dataReader.FieldCount];

                while (dataReader.Read())                                // reads rows
                {
                    for (int i = 0; i < dataReader.FieldCount; i++)      // reads fields
                    {
                        array[i] = dataReader[i].ToString();
                    }
                    fetchedData.Add(array);
                }   
            }
            return fetchedData;

        }

        public string SearchForEntityAndActivity(string entity, string activity)
        {
            string commandText = @"select * from knowledge_base
                                    where entity = @e and activity = @a";
            Dictionary<string, string> parameters = new Dictionary<string, string>
            {
                { "e", entity },
                { "a", activity }
            };

            var fetchedData = FetchData(commandText, parameters);

            if (fetchedData.Count >= 1) return fetchedData[0][2];
            else return null;
        }

        internal string SearchForEntityAndActivity(string activity)
        {
            string commandText = @"select * from knowledge_base
                                    where activity = @a";
            Dictionary<string, string> parameters = new Dictionary<string, string>
            {
                { "a", activity }
            };

            var fetchedData = FetchData(commandText, parameters);

            if (fetchedData.Count >= 1) return fetchedData[0][2];
            else return null;
        }
    }
}

