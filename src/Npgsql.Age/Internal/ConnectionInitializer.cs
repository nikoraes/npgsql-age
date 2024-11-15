using System;
using System.Threading.Tasks;
using Npgsql;

namespace Npgsql.Age.Internal
{
    internal static class ConnectionInitializer
    {
        public static void UsePhysicalConnectionInitializer(NpgsqlConnection connection, bool superUser)
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
            batch.BatchCommands.Add(new NpgsqlBatchCommand("SET search_path = ag_catalog, \"$user\", public;"));
            batch.ExecuteNonQuery();
        }

        public static async Task UsePhysicalConnectionInitializerAsync(NpgsqlConnection connection, bool superUser)
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
            batch.BatchCommands.Add(new NpgsqlBatchCommand("SET search_path = ag_catalog, \"$user\", public;"));
            await batch.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
    }
}