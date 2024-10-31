using System;
using System.Linq;
using System.Text.RegularExpressions;
using ApacheAGE.Internal;
using ApacheAGE.Resolvers;
using Npgsql;
using Npgsql.TypeMapping;

namespace ApacheAGE
{
    public static class NpgsqlAgeExtensions
    {
        /// <summary>
        /// Use Apache AGE types and connection initializer
        /// </summary>
        /// <param name="builder">Npgsql data source builder.</param>
        /// <param name="superUser">Whether to use super user privileges.</param>
        /// <returns></returns>
        public static INpgsqlTypeMapper UseAge(this NpgsqlDataSourceBuilder builder, bool superUser = true)
        {
            builder.AddTypeInfoResolverFactory(new AgtypeResolverFactory());
            builder.UsePhysicalConnectionInitializer(
                connection => ConnectionInitializer.UsePhysicalConnectionInitializer(connection, superUser),
                connection => ConnectionInitializer.UsePhysicalConnectionInitializerAsync(connection, superUser));
            return builder;
        }
    }

    public static class NpgsqlDataSourceAgeExtensions
    {

        public static NpgsqlCommand CreateGraphCommand(this NpgsqlDataSource dataSource, string graphName)
        {
            NpgsqlCommand command = dataSource.CreateCommand($"SELECT * FROM ag_catalog.create_graph($1);");
            command.Parameters.AddWithValue("name", graphName);
            return command;
        }

        public static NpgsqlCommand DropGraphCommand(this NpgsqlDataSource dataSource, string graphName)
        {
            NpgsqlCommand command = dataSource.CreateCommand($"SELECT * FROM ag_catalog.drop_graph($1);");
            command.Parameters.AddWithValue("name", graphName);
            return command;
        }

        public static NpgsqlCommand GraphExistsCommand(this NpgsqlDataSource dataSource, string graphName)
        {
            NpgsqlCommand command = dataSource.CreateCommand($"SELECT * FROM ag_catalog.ag_graph WHERE name = $1;");
            command.Parameters.AddWithValue("name", graphName);
            return command;
        }

        public static NpgsqlCommand CreateCypherCommand(this NpgsqlDataSource dataSource, string graph, string cypher)
        {
            string asPart = GenerateAsPart(cypher);
            string query = $"SELECT * FROM cypher('{graph}', $$ {cypher} $$) as {asPart};";
            NpgsqlCommand command = dataSource.CreateCommand(query);
            return command;
        }

        private static string GenerateAsPart(string cypher)
        {
            // Extract the return part of the Cypher query
            var match = Regex.Match(cypher, @"RETURN\s+(.*)", RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                throw new ArgumentException("Invalid Cypher query: RETURN clause not found.");
            }

            // Split the return values
            var returnValues = match.Groups[1].Value.Split(',');

            // Generate the 'as (...)' part
            var asPart = string.Join(", ", returnValues.Select((value, index) => $"{value.Trim()} agtype"));
            return $"({asPart})";
        }
    }
}
