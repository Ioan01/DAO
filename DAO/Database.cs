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
            (proxy as RepositoryProxy).Schema = Schema;

            return proxy;
        }

    }
}
