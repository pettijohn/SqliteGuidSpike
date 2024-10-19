using System.Diagnostics;
using Microsoft.Data.Sqlite;

var max = 1_000_000;

var loops = new List<string>() { "Blob", "Text"};
foreach (var columnType in loops)
{
    var guids = new List<Guid>();
    using (var connection = new SqliteConnection($"Data Source={columnType}.db"))
    {
        connection.Open();

        var createCommand = connection.CreateCommand();
        createCommand.CommandText = $"Create table Guids (ID {columnType} primary key) without rowid, strict";
        createCommand.ExecuteNonQuery();

        var sw = Stopwatch.StartNew();

        using (var insertCommand = connection.CreateCommand())
        {
            insertCommand.CommandText = @"INSERT into Guids (id) values ($id)";
            insertCommand.Parameters.AddWithValue("$id", Guid.Empty);
            insertCommand.Parameters["$id"].SqliteType = columnType == "Blob" ? SqliteType.Blob : SqliteType.Text;
            for (var i = 0; i < max; i++)
            {
                var g = Guid.NewGuid();
                insertCommand.Parameters["$id"].Value = g;
                guids.Add(g);
                insertCommand.ExecuteNonQuery();
            }
        }
        Console.WriteLine($"Insert {columnType}: " + sw.Elapsed);

        sw.Restart();
        var selectCommand = connection.CreateCommand();
        selectCommand.CommandText = @"SELECT id from Guids where id = $id";
        selectCommand.Parameters.AddWithValue("$id", Guid.Empty);
        selectCommand.Parameters["$id"].SqliteType = columnType == "Blob" ? SqliteType.Blob : SqliteType.Text;

        foreach (var g in guids)
        {
            selectCommand.Parameters["$id"].Value = g;
            using (var reader = selectCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (columnType == "Blob")
                        new Guid((byte[])reader[0]);
                    else
                        new Guid((string)reader[0]);
                }
            }
        }
        Console.WriteLine($"Select {columnType}: " + sw.Elapsed);
    }
}