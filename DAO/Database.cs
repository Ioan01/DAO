using System.Reflection;
using Npgsql;

namespace DAO
{
    internal class Database
    {
        public NpgsqlDataSource DataSource { get; set; }

        public string Schema { get; }

        public Database(string connectionString,string schema)
        {
            DataSource = NpgsqlDataSource.Create(connectionString);

            Schema = schema;

        }

        ~Database()
        {
            DataSource.Dispose();
        }

        public T CreateRepository<T>()
        {
            var proxy = DispatchProxy.Create<T, RepositoryProxy>();
            (proxy as RepositoryProxy).DataSource = DataSource;

            if (typeof(T).Name.Contains("Repository"))
                (proxy as RepositoryProxy).Table = $"{Schema}.{typeof(T).Name.Replace("Repository","")}";
            else (proxy as RepositoryProxy).Table = $"{Schema}.{typeof(T).Name}";

            return proxy;
        }

    }
}
