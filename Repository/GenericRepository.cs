﻿using Dapper;
using Npgsql;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Reflection;
using System.Text;

namespace DapperGenericRepository.Repository;
public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly NpgsqlConnection _connection;

    readonly string connectionString =  """
                Host=localhost;
                Username=postgres;
                Password=test;
                Database=railway-rlkko;
                """;
    public GenericRepository()
    {
        _connection = new NpgsqlConnection(connectionString);
        _connection.Open();
    }

    public bool Add(T entity)
    {
        int rowsAffected = 0;
        try
        {
            string tableName = GetTableName();
            string columns = GetColumns(excludeKey: true);
            string properties = GetPropertyNames(excludeKey: true);
            string query = $"INSERT INTO {tableName} ({columns}) VALUES ({properties})";

            rowsAffected = _connection.Execute(query, entity);
        }
        catch(Exception) { }

        return rowsAffected > 0;
    }

    public async Task<bool> AddAsync(T entity)
    {
        int rowsAffected = 0;
        try
        {
            string tableName = GetTableName();
            string columns = GetColumns(excludeKey: true);
            string properties = GetPropertyNames(excludeKey: true);
            string query = $"INSERT INTO {tableName} ({columns}) VALUES ({properties})";

            rowsAffected = await _connection.ExecuteAsync(query, entity);
        }
        catch(Exception) { }

        return rowsAffected > 0;
    }

    public bool Delete(T entity)
    {
        int rowsAffected = 0;
        try
        {
            string tableName = GetTableName();
            string keyColumn = GetKeyColumnName();
            string? keyProperty = GetKeyPropertyName();

            string query = $"DELETE FROM {tableName} WHERE {keyColumn} = @{keyProperty}";

            rowsAffected = _connection.Execute(query, entity);
        }
        catch(Exception) { }

        return rowsAffected > 0;
    }

    public virtual IEnumerable<T> GetAll()
    {
        IEnumerable<T> result = null;
        try
        {
            string TableName = GetTableName();
            string query = $"SELECT * FROM {TableName}";

            result = _connection.Query<T>(query);
        }
        catch(Exception) { }

        return result;
    }

    public T? GetById(long id)
    {
        T result = null;

        try
        {
            string TableName = GetTableName();
            string keyColumn = GetKeyColumnName();
            string query = $"SELECT * FROM {TableName} WHERE {keyColumn} = {id}";

            result = _connection.QueryFirst<T>(query);
        }
        catch(Exception) { }

        return result;
    }

    public bool Update(T entity)
    {
        int rowsEffected = 0;
        try
        {
            string tableName   = GetTableName();
            string keyColumn   = GetKeyColumnName();
            string keyProperty = GetKeyPropertyName();

            StringBuilder query = new StringBuilder();
            query.Append($"UPDATE {tableName} SET ");

            foreach(var property in GetProperties(true))
            {
                var columnAttr = property.GetCustomAttribute<ColumnAttribute>();

                string propertyName = property.Name;
                string columnName   = columnAttr.Name;

                query.Append($"{columnName} = @{propertyName},");
            }

            query.Remove(query.Length - 1, 1);

            query.Append($" WHERE {keyColumn} = @{keyProperty}");

            rowsEffected = _connection.Execute(query.ToString(), entity);
        }
        catch(Exception ex)
        {
            return false;
        }

        return rowsEffected > 0;
    }

    public async Task<bool> UpdateAsync(T entity)
    {
        int rowsEffected = 0;
        try
        {
            string tableName   = GetTableName();
            string keyColumn   = GetKeyColumnName();
            string keyProperty = GetKeyPropertyName();

            StringBuilder query = new StringBuilder();
            query.Append($"UPDATE {tableName} SET ");

            foreach(var property in GetProperties(true))
            {
                var columnAttr = property.GetCustomAttribute<ColumnAttribute>();

                string propertyName = property.Name;
                string columnName   = columnAttr.Name;

                query.Append($"{columnName} = @{propertyName},");
            }

            query.Remove(query.Length - 1, 1);

            query.Append($" WHERE {keyColumn} = @{keyProperty}");

            rowsEffected = await _connection.ExecuteAsync(query.ToString(), entity);
        }
        catch(Exception ex)
        {
            return false;
        }

        return rowsEffected > 0;
    }

    protected string GetPropertyNames(bool excludeKey = false)
    {
        var properties = typeof(T)
                .GetProperties()
                .Where(p=>!excludeKey || p.GetCustomAttribute<KeyAttribute>() == null);

        var values = string.Join(", ",properties
                .Select(p =>
                {
                    return $"@{p.Name}";
                }));
        return values;
    }

    private string GetTableName()
    {
        var type = typeof(T);
        var tableAttr = type.GetCustomAttribute<TableAttribute>();
        if(tableAttr != null)
        {
            return tableAttr.Name;
        }

        return type.Name + "s";
    }
    public static string GetKeyColumnName()
    {
        PropertyInfo[] properties = typeof(T).GetProperties();

        foreach(PropertyInfo property in properties)
        {
            object[] keyAttributes = property.GetCustomAttributes(typeof(KeyAttribute), true);

            if(keyAttributes != null && keyAttributes.Length > 0)
            {
                object[] columnAttributes = property.GetCustomAttributes(typeof(ColumnAttribute), true);

                if(columnAttributes != null && columnAttributes.Length > 0)
                {
                    ColumnAttribute columnAttribute = (ColumnAttribute)columnAttributes[0];
                    return columnAttribute.Name;
                }
                else
                {
                    return property.Name;
                }
            }
        }

        return null;
    }

    private string GetColumns(bool excludeKey = true)
    {
        var type = typeof(T);
        var columns = string.Join(", ", type.GetProperties()
                .Where(p => !excludeKey || !p.IsDefined(typeof(KeyAttribute)))
                .Select(p =>
                {
                    var columnAttr = p.GetCustomAttribute<ColumnAttribute>();
                    return columnAttr != null ? columnAttr.Name : p.Name;
                }));
        return columns;
    }

    protected IEnumerable<PropertyInfo> GetProperties(bool excludeKey = false)
    {
        var properties = typeof(T).GetProperties()
                .Where(p => !excludeKey || p.GetCustomAttribute<KeyAttribute>() == null);

        return properties;
    }
    protected string? GetKeyPropertyName()
    {
        var properties = typeof(T).GetProperties()
                .Where(p => p.GetCustomAttribute<KeyAttribute>() != null);

        if(properties.Any())
        {
            return properties.FirstOrDefault().Name;
        }

        return null;
    }
}

