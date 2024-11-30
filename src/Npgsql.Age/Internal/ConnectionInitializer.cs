using System;
using System.Threading.Tasks;
using Npgsql;

namespace Npgsql.Age.Internal
{
    internal static class ConnectionInitializer
    {
        public static NpgsqlConnection UsePhysicalConnectionInitializer(NpgsqlConnection connection, bool superUser)
        {
            InitializeConnection(connection, superUser).GetAwaiter().GetResult();
            return connection;
        }

        public static async Task<NpgsqlConnection> UsePhysicalConnectionInitializerAsync(NpgsqlConnection connection, bool superUser)
        {
            await InitializeConnection(connection, superUser).ConfigureAwait(false);
            return connection;
        }

        private static async Task InitializeConnection(NpgsqlConnection connection, bool superUser)
        {
            using var batch = new NpgsqlBatch(connection);
            if (superUser)
            {
                batch.BatchCommands.Add(new NpgsqlBatchCommand("CREATE EXTENSION IF NOT EXISTS age;"));
                batch.BatchCommands.Add(new NpgsqlBatchCommand("LOAD 'age';"));
            }
            else
            {
                batch.BatchCommands.Add(new NpgsqlBatchCommand("LOAD '$libdir/plugins/age';"));
            }
            // batch.BatchCommands.Add(new NpgsqlBatchCommand("SET search_path = ag_catalog, \"$user\", public;"));
            await batch.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
    }
}