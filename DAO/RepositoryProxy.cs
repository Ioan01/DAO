using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DAO.Models;
using static System.Char;
using System.Collections;
using Microsoft.VisualBasic.CompilerServices;

namespace DAO
{
    enum CommandType
    {
        None,
        Save,
        Select,
        Delete,

        
    }
    internal class RepositoryProxy : DispatchProxy
    {
        private static string[] keywords = { "save", "select", "delete" };

        public NpgsqlDataSource DataSource { private get; set; }

        public String Schema { private get; set; }

        private static CommandType GetType(string functionName)
        {
            if (functionName.StartsWith("find"))
                return CommandType.Select;
            if (functionName.StartsWith("delete"))
                return CommandType.Delete;
            if (functionName.StartsWith("save"))
                return CommandType.Save;
            throw new Exception($"Repository function {functionName} is invalid.");
        }

        private string GetPropertyValue(PropertyInfo propertyInfo, object instance)
        {
            if (propertyInfo.PropertyType == typeof(string))
                return $"'{propertyInfo.GetValue(instance)}'";
            if (propertyInfo.PropertyType.IsPrimitive)
                return $"{propertyInfo.GetValue(instance)}";
            // insert object in table

            return null;
        }

        private string GetObjectValue(object obj)
        {
            if (obj is string)
                return $"'{obj}'";
            if (obj.GetType().IsPrimitive)
                return obj.ToString();
            // insert object in table

            return null;
        }

        private string BuildDelete(StringBuilder query, MethodInfo methodInfo, object?[]? args)
        {
            var methodName = methodInfo.Name;

            string tableName;

            query.Append($"DELETE FROM {Schema}.");
            if (methodName.ToLower().Contains("all"))
            {
                tableName = methodName.Substring(methodName.IndexOf("all"));
                
            }
            else
            {
                tableName = methodName.Substring("delete".Length, methodName.IndexOf("By") - "delete".Length);
                query.Append($"{tableName} ");
                BuildWhere(query, methodInfo, args, "deleteBy".Length + tableName.Length);
            }

            


            
            

            return query.ToString();

        }

        private string BuildSelect(StringBuilder query, MethodInfo methodInfo, object?[]? args,out bool single)
        {
            if (methodInfo.ReturnType.IsGenericType)
                single = !(methodInfo.ReturnType.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            else single = true;

            var table = single ? methodInfo.ReturnType.Name : methodInfo.ReturnType.GenericTypeArguments[0].Name;
            query.Append($"SELECT * FROM {Schema}.{table} ");

            if (methodInfo.Name.ToLower().EndsWith("all"))
            {
                if (single)
                    throw new Exception($"Select all must have IEnumerable return type {methodInfo.Name}");
            }
            else
            {
                BuildWhere(query, methodInfo, args,"findBy".Length);

            }
            


            return query.ToString();
        }

        private void BuildWhere(StringBuilder query,MethodInfo methodInfo, object?[]? args,int index)
        {
            StringBuilder field = new StringBuilder(16);

            query.Append("WHERE ");

            var nameSpan = methodInfo.Name.Substring(index);

            field.Append(nameSpan[0]);

            int parameterIndex = 0;

            for (int i = 1; i < nameSpan.Length; i++)
            {
                if (IsUpper(nameSpan[i]))
                {
                    if (!field.Equals("And"))
                    {
                        query.Append(field);
                        query.Append(
                            $" = {GetObjectValue(args[parameterIndex++])} ");
                        
                    }
                    else query.Append($"{field} ");
                    field.Clear();
                }

                field.Append(nameSpan[i]);

                

            }

            query.Append(field);
            query.Append(
                $"= {GetObjectValue(args[parameterIndex++])}");

        }

        private string BuildInsert(StringBuilder query,MethodInfo methodInfo,object?[]? args)
        {
            if (args == null || args.Length == 0 || args[0] == null)
                throw new Exception($"No arguments provided for insert {methodInfo.Name}");
            if (methodInfo.ReturnType != typeof(bool))
                throw new Exception(
                    $"Return type of save must be bool, not {methodInfo.ReturnType} for {methodInfo.Name}");

            var obj = args[0];

            query.Append($"INSERT INTO {Schema}.{methodInfo.GetParameters()[0].ParameterType.Name}(");

            var properties = obj.GetType().GetProperties()
                .Where(pr => pr.CustomAttributes.All(attr => attr.AttributeType != typeof(PrimaryKey)));

            foreach (var property in properties)
            {
                query.Append($"{property.Name},");
            }

            query[^1] = ')';

            query.Append(" VALUES (");

            foreach (var property in properties)
            {
                query.Append($"{GetPropertyValue(property, obj)},");
            }

            query[^1] = ')';


            

            return query.ToString();

        }


        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            StringBuilder query = new StringBuilder(256);

            var commandType = GetType(targetMethod!.Name);


            switch (commandType)
            {
                case CommandType.Select:
                    bool single;
                    var select = BuildSelect(query,targetMethod,args,out single);
                    Console.WriteLine($"Select = {select}");

                    var reader = DataSource.CreateCommand(select).ExecuteReader();

                    return ReadResults(reader,targetMethod,single);

                    break;
                case CommandType.Delete:
                    var delete = BuildDelete(query, targetMethod, args);


                    Console.WriteLine($"Delete = {delete}");

                    DataSource.CreateCommand(delete).ExecuteNonQuery();
                    break;
                case CommandType.Save:
                    var insert = BuildInsert(query, targetMethod, args);

                    Console.WriteLine($"Insert = {insert}");

                    return DataSource.CreateCommand(insert).ExecuteNonQuery() != 0;
            }


            return null;
        }

        private void readObj(object obj, NpgsqlDataReader reader)
        {
            foreach (var property in obj.GetType().GetProperties())
            {
                int index = 0;
                while (reader.GetName(index).ToLower() != property.Name.ToLower() && index < reader.FieldCount)
                    index++;
                if (index != reader.FieldCount)
                {
                    property.SetValue(obj, reader.GetValue(index));
                }

            }
        }

        private object? ReadResults(NpgsqlDataReader reader, MethodInfo targetMethod,bool single)
        {
            Type objectType;
            if (targetMethod.ReturnType.IsGenericType)
                objectType = targetMethod.ReturnType.GetGenericArguments()[0];
            else objectType = targetMethod.ReturnType;

            if (!single)
            {
                var list = Activator.CreateInstance(typeof(List<>).MakeGenericType(objectType)) as IList;

                

                while (reader.Read())
                {
                    var obj = Activator.CreateInstance(objectType);

                    readObj(obj,reader);

                    list.Add(obj);

                }

                return list;

            }

            if (reader.Read())
            {
                var obj = Activator.CreateInstance(objectType);

                readObj(obj,reader);

                return obj;
            }

            return null;

        }
    }
}
