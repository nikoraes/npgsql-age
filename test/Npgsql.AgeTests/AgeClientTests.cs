using System.ComponentModel;
using Npgsql;
using Npgsql.Age;
using Npgsql.Age.Types;
using NpgsqlTypes;

namespace Npgsql.AgeTests;

public class AgeClientTests : TestBase
{

    [Fact]
    public async Task OpenConnectionAsync_ExtensionExists()
    {
        // Check if the extension exists in the database.
        var command = DataSource.CreateCommand("SELECT extname FROM pg_extension WHERE extname = 'age';");
        var result = await command.ExecuteScalarAsync();

        Assert.NotNull(result);
    }


    [Fact]
    public async Task GraphExistsAsync_Should_ReturnTrueIfGraphExists()
    {
        var graphName = await CreateTempGraphAsync();
        var graphExistsCommand = DataSource.GraphExistsCommand(graphName);
        var graphExists = await graphExistsCommand.ExecuteScalarAsync();
        Assert.True((bool)graphExists!);
    }

    [Fact]
    public async Task GraphExistsAsync_Should_ReturnFalseIfGraphExists()
    {
        var graphName = "sidjfa23knlsd9a8dfndfhjbnzxeunjakssdf3sdmvns_asdjfk";
        var graphExistsCommand = DataSource.GraphExistsCommand(graphName);
        var graphExists = await graphExistsCommand.ExecuteScalarAsync();
        Assert.False((bool)graphExists!);
    }

    [Fact]
    public async Task ExecuteQueryAsync_With_NoParameters_Should_ReturnDataReader()
    {
        var graphName = await CreateTempGraphAsync();
        await using var command = DataSource.CreateCypherCommand(graphName, "RETURN 1");
        await using var dataReader = await command.ExecuteReaderAsync();

        Assert.NotNull(dataReader);
        Assert.True(dataReader.HasRows);

        await DropTempGraphAsync(graphName);
    }


    [Fact]
    public async Task ExecuteQueryAsync_ReturnsExpectedResults()
    {
        var graphName = await CreateTempGraphAsync();
        await using var command = DataSource.CreateCypherCommand(graphName, "RETURN 1");
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
    public async Task ExecuteInvalidQueryAsync_Should_ThrowException()
    {
        var graphName = await CreateTempGraphAsync();
        await Assert.ThrowsAsync<PostgresException>(async () =>
        {
            await using var command = DataSource.CreateCypherCommand(graphName, "INVALID QUERY");
            await command.ExecuteReaderAsync();
        });
        await DropTempGraphAsync(graphName);
    }

}