namespace __APP_NAME__.Kit.Data;

/// <summary>One item. Rename it to the brief's main type.</summary>
public sealed record Item(long Id, string Title, bool IsStarred, DateTimeOffset CreatedAt);
