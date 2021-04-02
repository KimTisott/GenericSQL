using MySql.Data.MySqlClient;
using System;

namespace GenericSQL
{
    public class GenericDB : IDisposable
    {
        public string ConnectionString { get; }

        public GenericDB(string connectionString)
        {
            ConnectionString = connectionString;
            SetupDatabase(ConnectionString);
        }

        private static bool SetupDatabase(string connectionString)
        {
            using MySqlConnection connection = new(connectionString);
            return true;
        }

        public void Dispose()
        {
            
        }
    }
}