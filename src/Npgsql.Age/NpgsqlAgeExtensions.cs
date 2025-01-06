using Npgsql.Age.Internal;

namespace Npgsql.Age
{
    public static class NpgsqlAgeExtensions
    {
        /// <summary>
        /// Use Apache AGE types and connection initializer
        /// </summary>
        /// <param name="builder">Npgsql data source builder.</param>
        /// <param name="loadFromPlugins">Whether to use super user privileges.</param>
        /// <returns>The same builder instance so that multiple calls can be chained</returns>
        public static NpgsqlDataSourceBuilder UseAge(
            this NpgsqlDataSourceBuilder builder,
            bool loadFromPlugins = false
        )
        {
            builder.AddTypeInfoResolverFactory(new AgtypeResolverFactory());
            builder.UsePhysicalConnectionInitializer(
                connection =>
                    ConnectionInitializer.UsePhysicalConnectionInitializer(
                        connection,
                        loadFromPlugins
                    ),
                connection =>
                    ConnectionInitializer.UsePhysicalConnectionInitializerAsync(
                        connection,
                        loadFromPlugins
                    )
            );

            return builder;
        }
    }

    public static class NpgsqlConnectionAgeExtensions
    {
        public static NpgsqlCommand CreateGraphCommand(
            this NpgsqlConnection connection,
            string graphName
        )
        {
            return new NpgsqlCommand($"SELECT * FROM ag_catalog.create_graph($1);", connection)
            {
                Parameters = { new NpgsqlParameter { Value = graphName } },
            };
        }

        public static NpgsqlCommand DropGraphCommand(
            this NpgsqlConnection connection,
            string graphName
        )
        {
            return new NpgsqlCommand($"SELECT * FROM ag_catalog.drop_graph($1, true);", connection)
            {
                Parameters = { new NpgsqlParameter { Value = graphName } },
            };
        }

        public static NpgsqlCommand GraphExistsCommand(
            this NpgsqlConnection connection,
            string graphName
        )
        {
            return new NpgsqlCommand(
                $"SELECT EXISTS (SELECT 1 FROM ag_catalog.ag_graph WHERE name = $1);",
                connection
            )
            {
                Parameters = { new NpgsqlParameter { Value = graphName } },
            };
        }

        public static NpgsqlCommand CreateCypherCommand(
            this NpgsqlConnection connection,
            string graphName,
            string cypher
        )
        {
            string query =
                $"SELECT * FROM cypher('{graphName}', $$ {CypherHelpers.EscapeCypher(cypher)} $$) as {CypherHelpers.GenerateAsPart(cypher)};";
            return new NpgsqlCommand(query, connection);
        }
    }
}
