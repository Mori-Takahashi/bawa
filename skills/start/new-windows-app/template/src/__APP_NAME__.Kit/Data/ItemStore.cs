using Microsoft.Data.Sqlite;

namespace __APP_NAME__.Kit.Data;

/// <summary>
/// The SQLite database. One connection, opened for the life of the app.
/// Schema changes go in <see cref="Migrations"/>: append a step, never edit an old one,
/// and give new columns a default so existing databases keep loading.
/// </summary>
public sealed class ItemStore : IDisposable
{
    private static readonly string[] Migrations =
    [
        // 1: items
        """
        CREATE TABLE items (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            title TEXT NOT NULL,
            is_starred INTEGER NOT NULL DEFAULT 0,
            created_at TEXT NOT NULL
        );
        """,
    ];

    private readonly SqliteConnection _db;

    public ItemStore(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        _db = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = path }.ToString());
        _db.Open();
        Execute("PRAGMA journal_mode = WAL;");
        Execute("CREATE TABLE IF NOT EXISTS schema_version (version INTEGER NOT NULL);");
        Migrate();
    }

    public int SchemaVersion => Convert.ToInt32(Scalar("SELECT COALESCE(MAX(version), 0) FROM schema_version;"));

    public IReadOnlyList<Item> All()
    {
        using var cmd = _db.CreateCommand();
        cmd.CommandText = "SELECT id, title, is_starred, created_at FROM items ORDER BY created_at DESC, id DESC;";
        using var reader = cmd.ExecuteReader();
        var items = new List<Item>();
        while (reader.Read())
            items.Add(new Item(reader.GetInt64(0), reader.GetString(1), reader.GetInt64(2) != 0,
                DateTimeOffset.Parse(reader.GetString(3), System.Globalization.CultureInfo.InvariantCulture)));
        return items;
    }

    public Item Add(string title)
    {
        var created = DateTimeOffset.UtcNow;
        using var cmd = _db.CreateCommand();
        cmd.CommandText = "INSERT INTO items (title, created_at) VALUES ($title, $created); SELECT last_insert_rowid();";
        cmd.Parameters.AddWithValue("$title", title);
        cmd.Parameters.AddWithValue("$created", created.ToString("O"));
        var id = Convert.ToInt64(cmd.ExecuteScalar());
        return new Item(id, title, false, created);
    }

    public void SetStarred(long id, bool starred)
        => Execute("UPDATE items SET is_starred = $v WHERE id = $id;", ("$v", starred ? 1 : 0), ("$id", id));

    public void Delete(long id) => Execute("DELETE FROM items WHERE id = $id;", ("$id", id));

    public int Count() => Convert.ToInt32(Scalar("SELECT COUNT(*) FROM items;"));

    public void Dispose() => _db.Dispose();

    private void Migrate()
    {
        for (var version = SchemaVersion; version < Migrations.Length; version++)
        {
            using var tx = _db.BeginTransaction();
            Execute(Migrations[version]);
            Execute("DELETE FROM schema_version; INSERT INTO schema_version (version) VALUES ($v);", ("$v", version + 1));
            tx.Commit();
        }
    }

    private void Execute(string sql, params (string Name, object Value)[] args)
    {
        using var cmd = _db.CreateCommand();
        cmd.CommandText = sql;
        foreach (var (name, value) in args) cmd.Parameters.AddWithValue(name, value);
        cmd.ExecuteNonQuery();
    }

    private object? Scalar(string sql)
    {
        using var cmd = _db.CreateCommand();
        cmd.CommandText = sql;
        return cmd.ExecuteScalar();
    }
}
