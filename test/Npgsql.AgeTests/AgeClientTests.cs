using System.ComponentModel;
using Npgsql;
using Npgsql.Age;
using Npgsql.Age.Types;
using NUnit.Framework;

namespace Npgsql.AgeTests;

internal class AgeClientTests : TestBase
{

    [Test]
    public async Task OpenConnectionAsync_ExtensionExists()
    {
        // Check if the extension exists in the database.
        var command = DataSource.CreateCommand("SELECT extname FROM pg_extension WHERE extname = 'age';");
        var result = await command.ExecuteScalarAsync();

        Assert.That(result, Is.Not.Null);
    }


    [Test]
    public async Task GraphExistsAsync_Should_ReturnTrueIfGraphExists()
    {
        var graphName = await CreateTempGraphAsync();
        var graphExistsCommand = DataSource.GraphExistsCommand(graphName);
        var graphExists = await graphExistsCommand.ExecuteScalarAsync();
        Assert.That(graphExists, Is.True);
    }

    [Test]
    public async Task GraphExistsAsync_Should_ReturnFalseIfGraphExists()
    {
        var graphName = "sidjfa23knlsd9a8dfndfhjbnzxeunjakssdf3sdmvns_asdjfk";
        var graphExistsCommand = DataSource.GraphExistsCommand(graphName);
        var graphExists = await graphExistsCommand.ExecuteScalarAsync();
        Assert.That(graphExists, Is.False);
    }

    [Test]
    public async Task ExecuteQueryAsync_With_NoParameters_Should_ReturnDataReader()
    {
        var graphName = await CreateTempGraphAsync();
        await using var command = DataSource.CreateCypherCommand(graphName, "RETURN 1");
        await using var dataReader = await command.ExecuteReaderAsync();

        Assert.That(dataReader, Is.Not.Null);
        Assert.That(dataReader.HasRows, Is.True);

        await DropTempGraphAsync(graphName);
    }


    [Test]
    public async Task ExecuteQueryAsync_ReturnsExpectedResults()
    {
        var graphName = await CreateTempGraphAsync();
        await using var command = DataSource.CreateCypherCommand(graphName, "RETURN 1");
        await using var dataReader = await command.ExecuteReaderAsync();

        Assert.That(dataReader, Is.Not.Null);
        Assert.That(dataReader.HasRows, Is.True);

        var schema = await dataReader.GetColumnSchemaAsync();

        Assert.That(await dataReader.ReadAsync(), Is.True);
        var agResult = await dataReader.GetFieldValueAsync<Agtype?>(0);
        Assert.That(agResult, Is.Not.Null);

        await DropTempGraphAsync(graphName);
    }
}