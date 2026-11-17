using System.Runtime.InteropServices;
using System.Text;
using __APP_NAME__.Kit.Data;
using Xunit;

namespace __APP_NAME__.Kit.Tests;

public partial class StorePathTests
{
    private static readonly string Temp = Path.GetTempPath();

    [Fact]
    public void Default_path_is_under_local_app_data()
        => Assert.Equal(@"C:\L\__APP_NAME__\__APP_NAME__.db", StorePath.Resolve("__APP_NAME__", null, @"C:\L"));

    [Fact]
    public void Override_wins_over_the_default()
        => Assert.Equal(Path.Combine(Temp, "x.db"), StorePath.Resolve("__APP_NAME__", Path.Combine(Temp, "x.db"), @"C:\L"));

    [Fact]
    public void Temp_store_is_a_test_store()
        => Assert.True(StorePath.IsTestStore(Path.Combine(Temp, "x.db"), Temp));

    [Fact]
    public void Store_outside_temp_is_not_a_test_store()
        => Assert.False(StorePath.IsTestStore(@"C:\Users\me\Documents\x.db", Temp));

    [Fact]
    public void Relative_path_escaping_temp_is_not_a_test_store()
        => Assert.False(StorePath.IsTestStore(Path.Combine(Temp, @"..\x.db"), Temp));

    [Fact]
    public void Folder_that_only_starts_like_temp_is_not_a_test_store()
        => Assert.False(StorePath.IsTestStore(Temp.TrimEnd('\\') + "Other\\x.db", Temp));

    [Fact]
    public void Short_8dot3_temp_path_is_a_test_store()
    {
        var longDir = Directory.CreateTempSubdirectory("long folder name for 8dot3 ").FullName;
        try
        {
            var buffer = new StringBuilder(1024);
            GetShortPathName(longDir, buffer, buffer.Capacity);
            var shortDir = buffer.ToString();
            Assert.SkipWhen(shortDir.Length == 0 || shortDir == longDir, "8.3 names are off on this volume");
            Assert.True(StorePath.IsTestStore(Path.Combine(shortDir, "x.db"), Temp));
        }
        finally { Directory.Delete(longDir); }
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern uint GetShortPathName(string longPath, StringBuilder shortPath, int size);
}
