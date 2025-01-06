using System.ComponentModel;
using Npgsql;
using Npgsql.Age;
using Npgsql.Age.Types;
using NpgsqlTypes;

namespace Npgsql.AgeTests;

public class AgeIntegrationTests : TestBase
{
    [Fact]
    public async Task OpenConnectionAsync_ExtensionExists()
    {
        // Check if the extension exists in the database.
        var command = DataSource.CreateCommand(
            "SELECT extname FROM pg_extension WHERE extname = 'age';"
        );
        var result = await command.ExecuteScalarAsync();

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GraphExistsAsync_Should_ReturnTrueIfGraphExists()
    {
        var graphName = await CreateTempGraphAsync();
        await using var connection = await DataSource.OpenConnectionAsync();
        await using var graphExistsCommand = connection.GraphExistsCommand(graphName);
        var graphExists = await graphExistsCommand.ExecuteScalarAsync();
        Assert.True((bool)graphExists!);
    }

    [Fact]
    public async Task GraphExistsAsync_Should_ReturnFalseIfGraphNotExists()
    {
        var graphName = "sidjfa23knlsd9a8dfndfhjbnzxeunjakssdf3sdmvns_asdjfk";
        await using var connection = await DataSource.OpenConnectionAsync();
        await using var graphExistsCommand = connection.GraphExistsCommand(graphName);
        var graphExists = await graphExistsCommand.ExecuteScalarAsync();
        Assert.False((bool)graphExists!);
    }

    [Fact]
    public async Task Value_Should_BeNull_When_AGEOutputsNull()
    {
        var graphname = await CreateTempGraphAsync();

        await using var connection = await DataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(
            $@"SELECT * FROM cypher('{graphname}', $$
    RETURN NULL
$$) as (value agtype);",
            connection
        );
        await using var dataReader = await command.ExecuteReaderAsync();
        Assert.NotNull(dataReader);
        Assert.True(await dataReader.ReadAsync());
        var agResult = await dataReader.GetFieldValueAsync<Agtype?>(0);

        Assert.Null(agResult);

        await DropTempGraphAsync(graphname);
    }

    [Fact]
    public async Task GetDouble_Should_ReturnPositiveInfinity_When_AGEOutputsInfinity()
    {
        var graphname = await CreateTempGraphAsync();

        await using var connection = await DataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(
            $@"SELECT * FROM cypher('{graphname}', $$
        RETURN 'Infinity'::float
    $$) as (value agtype);",
            connection
        );
        await using var dataReader = await command.ExecuteReaderAsync();
        Assert.NotNull(dataReader);
        Assert.True(await dataReader.ReadAsync());
        var agResult = await dataReader.GetFieldValueAsync<Agtype?>(0);

        Assert.Equal(double.PositiveInfinity, agResult?.GetDouble());

        await DropTempGraphAsync(graphname);
    }

    [Fact]
    public async Task GetDouble_Should_ReturnNaN_When_AGEOutputsNaN()
    {
        var graphname = await CreateTempGraphAsync();

        await using var connection = await DataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(
            $@"SELECT * FROM cypher('{graphname}', $$
            RETURN 'NaN'::float
        $$) as (value agtype);",
            connection
        );
        await using var dataReader = await command.ExecuteReaderAsync();
        Assert.NotNull(dataReader);
        Assert.True(await dataReader.ReadAsync());
        var agResult = await dataReader.GetFieldValueAsync<Agtype?>(0);

        Assert.Equal(double.NaN, agResult?.GetDouble());

        await DropTempGraphAsync(graphname);
    }

    [Fact]
    public async Task GetVertex_Should_ReturnCorrectVertex()
    {
        var graphname = await CreateTempGraphAsync();
        ulong id = 234323;
        var label = "Person";
        var i = 3;

        await using var connection = await DataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(
            $@"SELECT * FROM cypher('{graphname}', $$
            WITH {{id: {id}, label: ""{label}"", properties: {{i: {i}}}}}::vertex as v
            RETURN v
        $$) as (value agtype);",
            connection
        );
        await using var dataReader = await command.ExecuteReaderAsync();
        Assert.NotNull(dataReader);
        Assert.True(await dataReader.ReadAsync());
        var agResult = await dataReader.GetFieldValueAsync<Agtype?>(0);
        var vertex = agResult?.GetVertex();

        Assert.NotNull(vertex);
        Assert.Equal(id, vertex?.Id.Value);
        Assert.Equal(label, vertex?.Label);
        Assert.Equal(i, vertex?.Properties["i"]);

        await DropTempGraphAsync(graphname);
    }

    [Fact]
    public async Task GetList_Should_CorrectlyParseNullValues()
    {
        var graphname = await CreateTempGraphAsync();
        var list = new List<object?> { 1, 2, 3, 2, null };

        await using var connection = await DataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(
            $@"SELECT * FROM cypher('{graphname}', $$
            WITH [1, 2, 3, 2, NULL] AS list
            RETURN list
        $$) as (value agtype);",
            connection
        );
        await using var dataReader = await command.ExecuteReaderAsync();
        Assert.NotNull(dataReader);
        Assert.True(await dataReader.ReadAsync());
        var agResult = await dataReader.GetFieldValueAsync<Agtype?>(0);

        Assert.Equal(list, agResult?.GetList());

        await DropTempGraphAsync(graphname);
    }

    [Fact]
    public async Task ExecuteCypherQueryAsync_With_NoParameters_Should_ReturnDataReader()
    {
        var graphName = await CreateTempGraphAsync();
        await using var connection = await DataSource.OpenConnectionAsync();
        await using var command = connection.CreateCypherCommand(graphName, "RETURN 1");
        await using var dataReader = await command.ExecuteReaderAsync();

        Assert.NotNull(dataReader);
        Assert.True(dataReader.HasRows);

        await DropTempGraphAsync(graphName);
    }

    [Fact]
    public async Task ExecuteCypherQueryAsync_ReturnsExpectedResults()
    {
        var graphName = await CreateTempGraphAsync();
        await using var connection = await DataSource.OpenConnectionAsync();
        await using var command = connection.CreateCypherCommand(graphName, "RETURN 1");
        await using var dataReader = await command.ExecuteReaderAsync();

        Assert.NotNull(dataReader);
        Assert.True(dataReader.HasRows);

        var schema = await dataReader.GetColumnSchemaAsync();

        Assert.True(await dataReader.ReadAsync());
        var agResult = await dataReader.GetFieldValueAsync<Agtype?>(0);
        Assert.NotNull(agResult);

        await DropTempGraphAsync(graphName);
    }

    [Fact]
    public async Task ExecuteInvalidCypherQueryAsync_Should_ThrowException()
    {
        var graphName = await CreateTempGraphAsync();
        await Assert.ThrowsAsync<PostgresException>(async () =>
        {
            await using var connection = await DataSource.OpenConnectionAsync();
            await using var command = connection.CreateCypherCommand(graphName, "INVALID QUERY");
            await command.ExecuteReaderAsync();
        });
        await DropTempGraphAsync(graphName);
    }
}
