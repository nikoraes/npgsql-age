using System;
using System.Linq;
using System.Text.RegularExpressions;
using Npgsql.Age.Internal;
using Npgsql;
using Npgsql.TypeMapping;
using System.Threading.Tasks;

namespace Npgsql.Age
{
    public static class NpgsqlAgeExtensions
    {
        /// <summary>
        /// Use Apache AGE types and connection initializer
        /// </summary>
        /// <param name="builder">Npgsql data source builder.</param>
        /// <param name="superUser">Whether to use super user privileges.</param>
        /// <returns>The same builder instance so that multiple calls can be chained</returns>
        public static NpgsqlDataSourceBuilder UseAge(this NpgsqlDataSourceBuilder builder, bool superUser = true)
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
            command.Parameters.AddWithValue(graphName);
            return command;
        }

        public static NpgsqlCommand DropGraphCommand(this NpgsqlDataSource dataSource, string graphName)
        {
            NpgsqlCommand command = dataSource.CreateCommand($"SELECT * FROM ag_catalog.drop_graph($1, true);");
            command.Parameters.AddWithValue(graphName);
            return command;
        }

        public static NpgsqlCommand GraphExistsCommand(this NpgsqlDataSource dataSource, string graphName)
        {
            NpgsqlCommand command = dataSource.CreateCommand($"SELECT EXISTS (SELECT 1 FROM ag_catalog.ag_graph WHERE name = $1);");
            command.Parameters.AddWithValue(graphName);
            return command;
        }

        public static async ValueTask<NpgsqlConnection> OpenAgeConnectionAsync(this NpgsqlDataSource dataSource)
        {
            NpgsqlConnection connection = dataSource.CreateConnection();
            await connection.OpenAsync();
            return connection;
        }

        public static NpgsqlCommand CreateCypherCommand(this NpgsqlDataSource dataSource, string graphName, string cypher)
        {
            string asPart = CypherHelpers.GenerateAsPart(cypher);
            string query = $"SELECT * FROM cypher('{graphName}', $$ {cypher} $$) as {asPart};";
            NpgsqlCommand command = dataSource.CreateCommand(query);
            return command;
        }
    }

    public static class NpgsqlConnectionAgeExtensions
    {
        public static NpgsqlCommand CreateGraphCommand(this NpgsqlConnection connection, string graphName)
        {
            NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = $"SELECT * FROM ag_catalog.create_graph($1);";
            command.Parameters.AddWithValue(graphName);
            return command;
        }

        public static NpgsqlCommand DropGraphCommand(this NpgsqlConnection connection, string graphName)
        {
            NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = $"SELECT * FROM ag_catalog.drop_graph($1);";
            command.Parameters.AddWithValue(graphName);
            return command;
        }

        public static NpgsqlCommand GraphExistsCommand(this NpgsqlConnection connection, string graphName)
        {
            NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = $"SELECT * FROM ag_catalog.ag_graph WHERE name = $1;";
            command.Parameters.AddWithValue(graphName);
            return command;
        }

        public static NpgsqlCommand CreateCypherCommand(this NpgsqlConnection connection, string graph, string cypher)
        {
            string asPart = CypherHelpers.GenerateAsPart(cypher);
            string query = $"SELECT * FROM cypher('{graph}', $$ {cypher} $$) as {asPart};";
            NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = query;
            return command;
        }
    }

    internal static class CypherHelpers
    {
        internal static string GenerateAsPart(string cypher)
        {
            // Extract the return part of the Cypher query
            var match = Regex.Match(cypher, @"RETURN\s+(.*?)(?:\s+LIMIT|\s+SKIP|\s+ORDER|[\[{]|$)", RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                return "(result agtype)";
            }

            // Split the return values
            var returnValues = match.Groups[1].Value.Split(',');

            // Generate the 'as (...)' part
            var asPart = string.Join(", ", returnValues.Select((value, index) =>
            {
                var trimmedValue = value.Trim().TrimStart('$');
                if (int.TryParse(trimmedValue, out _) || double.TryParse(trimmedValue, out _))
                {
                    return $"num{index} agtype";
                }
                if (Regex.IsMatch(trimmedValue, @"\w+\(.*\)"))
                {
                    var exprName = Regex.Match(trimmedValue, @"\w+").Value;
                    return $"{exprName} agtype"; // TODO: use index or something when there are multiple of the same $"{exprName}{index} agtype";
                }
                if (trimmedValue.Any(char.IsUpper))
                {
                    return $"\"{trimmedValue}\" agtype";
                }
                var sanitizedValue = Regex.Replace(trimmedValue, @"[^\w]", "_");
                return $"{sanitizedValue} agtype";
            }));
            return $"({asPart})";
        }
    }
}
