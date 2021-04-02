using Dapper;
using GenericSQL.Attributes;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericSQL
{
    public static class Dapper
    {
        public async static Task<T> SelectItem<T>(GenericQuery<T> query)
        {
            T result = default;
            var table = query.Type.CustomAttributes.FirstOrDefault(x => x.AttributeType == typeof(TableAttribute)).ConstructorArguments.FirstOrDefault().Value;
            var joins = string.Join(" ", query.Joins.Select(x => $"LEFT JOIN {x.table} ON {x.primaryKey} = {x.foreignKey}"));
            var sql = $@"SELECT {(query.ColumnNames.Count > 0 ? string.Join(",", query.ColumnNames) : "*")}
                         FROM `{table}`
                         {joins}
                         {(query.Where != null ? $" WHERE {query.Where}" : "")}";
            using MySqlConnection connection = new(query.ConnectionString);
            result = await connection.QueryFirstOrDefaultAsync<T>(sql: sql);
            return await Task.FromResult(result);
        }

        public async static Task<IEnumerable<T>> SelectList<T>(GenericQuery<T> query)
        {
            IEnumerable<T> result = default;
            var table = query.Type.CustomAttributes.FirstOrDefault(x => x.AttributeType == typeof(TableAttribute)).ConstructorArguments.FirstOrDefault().Value;
            var sql = $"SELECT {(query.ColumnNames.Count > 0 ? string.Join(",", query.ColumnNames) : "*")} FROM `{table}`{(query.Where != null ? $" WHERE {query.Where}" : "")}";
            using MySqlConnection connection = new(query.ConnectionString);
            result = await connection.QueryAsync<T>(sql: sql);
            return await Task.FromResult(result);
        }

        public async static Task<int> Insert<T>(GenericQuery<T> query, T entity)
        {
            int result = default;
            var table = query.Type.CustomAttributes.FirstOrDefault(x => x.AttributeType == typeof(TableAttribute)).ConstructorArguments.FirstOrDefault().Value;
            var properties = query.Properties.Where(x => x.CustomAttributes.Any(y => y.AttributeType == typeof(ColumnAttribute)) && !x.CustomAttributes.Any(y => y.AttributeType == typeof(PrimaryKeyAttribute)) && AcceptedTypes.Contains(x.PropertyType));
            var columns = string.Join(", ", properties.Select(x => x.Name));
            var values = string.Join(", ", properties.Select(x => $"@{x.Name}"));
            var param = new DynamicParameters();
            foreach (var property in query.Properties)
                param.Add(property.Name, property.GetValue(entity));
            var sql = $"INSERT INTO `{table}` ({columns}) VALUES ({values})";
            using MySqlConnection connection = new(query.ConnectionString);
            result = await connection.ExecuteAsync(sql: sql, param: param);
            return await Task.FromResult(result);
        }

        public async static Task<int> Update<T>(GenericQuery<T> query, T entity)
        {
            int result = default;
            var tableName = query.Type.CustomAttributes.FirstOrDefault(x => x.AttributeType == typeof(TableAttribute)).ConstructorArguments.FirstOrDefault().Value;
            var properties = query.Properties.Where(x => x.CustomAttributes.Any(y => y.AttributeType == typeof(ColumnAttribute)) && !x.CustomAttributes.Any(y => y.AttributeType == typeof(PrimaryKeyAttribute)) && AcceptedTypes.Contains(x.PropertyType));
            var columnsSetters = string.Join(", ", properties.Select(x => $"{x.Name}=@{x.Name}"));
            var sql = $"UPDATE `{tableName}` SET {columnsSetters}{(query.Where != null ? $" WHERE {query.Where}" : "")}";
            var param = new DynamicParameters();
            foreach (var property in properties)
                param.Add(property.Name, property.GetValue(entity));
            using MySqlConnection connection = new(query.ConnectionString);
            result = await connection.ExecuteAsync(sql: sql, param: param);
            return await Task.FromResult(result);
        }

        public async static Task<int> Delete<T>(GenericQuery<T> query)
        {
            int result = default;
            var table = query.Type.CustomAttributes.FirstOrDefault(x => x.AttributeType == typeof(TableAttribute)).ConstructorArguments.FirstOrDefault().Value;
            var sql = $"DELETE FROM `{table}`{(query.Where != null ? $" WHERE {query.Where}" : "")}";
            using MySqlConnection connection = new(query.ConnectionString);
            result = await connection.ExecuteAsync(sql: sql);
            return await Task.FromResult(result);
        }

        public async static Task<int?> Count<T>(GenericQuery<T> query)
        {
            int? result = default;
            var table = typeof(T).CustomAttributes.FirstOrDefault(x => x.AttributeType == typeof(TableAttribute)).ConstructorArguments.FirstOrDefault().Value;
            var sql = $"SELECT COUNT(*) FROM `{table}`{(query.Where != null ? $" WHERE {query.Where}" : "")}";
            using MySqlConnection connection = new(query.ConnectionString);
            result = await connection.ExecuteScalarAsync<int?>(sql: sql);
            return await Task.FromResult(result);
        }

        private static readonly Type[] AcceptedTypes = new Type[]
        {
            typeof(int),
            typeof(string),
            typeof(float),
            typeof(short)
        };
    }
}
