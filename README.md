# Npgsql.Age

[![Nuget](https://img.shields.io/nuget/v/Npgsql.Age?color=blue)](https://www.nuget.org/packages/Npgsql.Age/)

## What is Apache AGE?

Apache AGE is an open-source extension for PostgreSQL which provides it with the capabilities of a graph database. This package is a plugin for the Npgsql library which allows you to interact with Apache AGE from C#.

## Quickstart

Here's a simple example to get you started:

```csharp
using Npgsql;
using Npgsql.Age;
using Npgsql.Age.Types;

var connectionString = "Host=server;Port=5432;Username=user;Password=pass;Database=sample1";

var dataSourceBuilder = new NpgsqlDataSourceBuilder(connString);
await using var dataSource = dataSourceBuilder
    .UseAge()
    .Build();

// Create graph
await using (var cmd = dataSource.CreateGraphCommand("graph1"))
{
    await cmd.ExecuteNonQueryAsync();
}

// Add vertices
await using (var cmd = dataSource.CreateCypherCommand("graph1", "CREATE (:Person {age: 23}), (:Person {age: 78})"))
{
    await cmd.ExecuteNonQueryAsync();
}

// Retrieve vertices
await using (var cmd = dataSource.CreateCypherCommand(
    "graph1", "MATCH (n:Person) RETURN n"))
await using (var reader = await cmd.ExecuteReaderAsync())
{
    while (await reader.ReadAsync())
    {
        var agtypeResult = reader.GetValue<Agtype>(0);
        Vertex person = agtypeResult.GetVertex();
        Console.WriteLine(person);
    }
}
```

## Acknowledgements

* This project is a fork of [Apache AGE](https://github.com/Allison-E/pg-age).
* The project relies heavily on the work of the [Npgsql](https://github.com/npgsql/npgsql) team.
