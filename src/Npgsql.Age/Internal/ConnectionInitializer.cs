using System;
using System.Threading.Tasks;
using Npgsql;

namespace Npgsql.Age.Internal
{
    internal static class ConnectionInitializer
    {
        public static NpgsqlConnection UsePhysicalConnectionInitializer(
            NpgsqlConnection connection,
            bool loadFromPlugins
        )
        {
            InitializeConnection(connection, loadFromPlugins).GetAwaiter().GetResult();
            return connection;
        }

        public static async Task<NpgsqlConnection> UsePhysicalConnectionInitializerAsync(
            NpgsqlConnection connection,
            bool loadFromPlugins
        )
        {
            await InitializeConnection(connection, loadFromPlugins).ConfigureAwait(false);
            return connection;
        }

        private static async Task InitializeConnection(
            NpgsqlConnection connection,
            bool loadFromPlugins
        )
        {
            using var batch = new NpgsqlBatch(connection);
            if (loadFromPlugins)
            {
                batch.BatchCommands.Add(new NpgsqlBatchCommand("LOAD '$libdir/plugins/age';"));
            }
            else
            {
                batch.BatchCommands.Add(
                    new NpgsqlBatchCommand("CREATE EXTENSION IF NOT EXISTS age;")
                );
                batch.BatchCommands.Add(new NpgsqlBatchCommand("LOAD 'age';"));
            }
            await batch.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
    }
}
