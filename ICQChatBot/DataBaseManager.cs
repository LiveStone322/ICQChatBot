using System;
using System.Collections.Generic;
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


        private List<string[]> FetchData(string commandText, Dictionary<string, string> parameters)//, string street, string building, string duration)
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

                //sCommand.Parameters.AddWithValue("@p1", city);
                //sCommand.Parameters.AddWithValue("@p2", parsedElement[1]);
                //sCommand.Parameters.AddWithValue("@p3", parsedElement[2]);
                //sCommand.Parameters.AddWithValue("@p4", parsedElement[3]);
                var dataReader = sCommand.ExecuteReader();

                string[] array = new string[dataReader.FieldCount];

                while (dataReader.Read())                                // reads rows
                {
                    for (int i = 0; i < dataReader.FieldCount; i++)      // reads fields
                    {
                        array[i] = dataReader[i].ToString();
                        //Console.Write(array[i]);
                    }
                    fetchedData.Add(array);

                    //Console.WriteLine();
                }   
            }
            return fetchedData;

        }
    }
}

