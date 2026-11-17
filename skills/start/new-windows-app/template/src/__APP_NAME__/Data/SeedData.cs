using __APP_NAME__.Kit.Data;

namespace __APP_NAME__.Data;

/// <summary>
/// Sample data for snapshots: APP_SEED=1 fills an empty test store.
/// Grow it with the app: long and short titles, every state, and only what
/// a person can create in the app.
/// </summary>
public static class SeedData
{
    private static readonly (string Title, bool Starred)[] Items =
    [
        ("Buy milk", false),
        ("Call the dentist about the appointment on Thursday afternoon", true),
        ("Read chapter 4", false),
        ("Plan the weekend trip", true),
        ("Pay the electricity bill", false),
        ("Water the plants", false),
        ("Back up the photos from the summer holiday to the external drive", false),
        ("Renew passport", true),
        ("Fix the squeaky door", false),
        ("Send birthday card to Anna", false),
        ("Sort the downloads folder", false),
        ("Try the new pasta recipe", true),
    ];

    public static void FillIfEmpty(ItemStore store)
    {
        if (store.Count() > 0) return;
        foreach (var (title, starred) in Items)
        {
            var item = store.Add(title);
            if (starred) store.SetStarred(item.Id, true);
        }
    }
}
