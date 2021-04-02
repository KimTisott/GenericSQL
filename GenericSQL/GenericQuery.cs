using System;
using System.Collections.Generic;
using System.Reflection;

namespace GenericSQL
{
    public class GenericQuery<T>
    {
        public Type Type => typeof(T);
        public PropertyInfo[] Properties => Type.GetProperties();
        public string ConnectionString { get; }
        public string TableName { get; set; }
        public List<string> ColumnNames { get; set; } = new List<string>();
        public List<(string table, string primaryKey, string foreignKey)> Joins { get; set; } = new List<(string table, string primaryKey, string foreignKey)>();
        public string Where { get; set; }
        public GenericQuery(string connectionString)
        {
            ConnectionString = connectionString;
        }
    }
}