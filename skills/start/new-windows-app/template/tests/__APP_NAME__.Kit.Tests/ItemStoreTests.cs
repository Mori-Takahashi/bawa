using __APP_NAME__.Kit.Data;
using Xunit;

namespace __APP_NAME__.Kit.Tests;

public sealed class ItemStoreTests : IDisposable
{
    private readonly string _dir = Directory.CreateTempSubdirectory("__APP_NAME__-tests-").FullName;
    private string DbPath => Path.Combine(_dir, "test.db");

    public void Dispose()
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        Directory.Delete(_dir, recursive: true);
    }

    [Fact]
    public void New_store_has_schema_version_1_and_no_items()
    {
        using var store = new ItemStore(DbPath);
        Assert.Equal(1, store.SchemaVersion);
        Assert.Equal(0, store.Count());
    }

    [Fact]
    public void Add_star_delete_round_trip()
    {
        using var store = new ItemStore(DbPath);
        var item = store.Add("Milk");
        Assert.Single(store.All());
        Assert.Equal("Milk", store.All()[0].Title);

        store.SetStarred(item.Id, true);
        Assert.True(store.All()[0].IsStarred);

        store.Delete(item.Id);
        Assert.Equal(0, store.Count());
    }

    [Fact]
    public void Reopening_keeps_items()
    {
        using (var store = new ItemStore(DbPath)) store.Add("Milk");
        using var again = new ItemStore(DbPath);
        Assert.Equal(1, again.Count());
    }

    [Fact]
    public void All_lists_newest_first()
    {
        using var store = new ItemStore(DbPath);
        store.Add("First");
        store.Add("Second");
        Assert.Equal(["Second", "First"], store.All().Select(i => i.Title));
    }
}
