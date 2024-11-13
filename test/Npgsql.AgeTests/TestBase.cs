﻿using Npgsql;
using Npgsql.Age;

namespace Npgsql.AgeTests;

internal class TestBase
{
    private static string _defaultConnectionString =
        "Server=localhost;Port=5432;Database=agedotnet_tests;";

    protected static string ConnectionString =>
        Environment.GetEnvironmentVariable("AGE_TEST_DB") ?? _defaultConnectionString;

    protected NpgsqlConnection GetConnection() =>
        new NpgsqlDataSourceBuilder(ConnectionString)
             .UseAge()
             .Build()
             .OpenConnection();

    protected NpgsqlCommand CreateCommand(string command) =>
        new(command, GetConnection());

    /* protected async Task<string> CreateTempGraphAsync()
    {
        var graphName = "temp_graph" + DateTime.Now.ToString("yyyyMMddHHmmssffff");
        await using var client = CreateAgeClient();
        await client.OpenConnectionAsync();
        await client.CreateGraphAsync(graphName);

        return graphName;
    }

    protected async Task DropTempGraphAsync(string graphName)
    {
        await using var client = CreateAgeClient();
        await client.OpenConnectionAsync();
        await client.DropGraphAsync(graphName, true);
    } */
}
