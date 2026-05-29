using Sport.Core.Structure;

namespace Sport.Disciplines.JUD;

// Paris 2024 categories. "Over/+" categories use an 'O' prefix because EventTypeCode is A-Z0-9.
internal static class JudWeightCategories
{
    public static readonly string[] Men =
        { "60KG", "66KG", "73KG", "81KG", "90KG", "100KG", "O100KG" };

    public static readonly string[] Women =
        { "48KG", "52KG", "57KG", "63KG", "70KG", "78KG", "O78KG" };

    public const string TeamEventType = "TEAM6";

    public static readonly SubunitCode[] TeamContests =
    {
        SubunitCode.From("01"), SubunitCode.From("02"), SubunitCode.From("03"),
        SubunitCode.From("04"), SubunitCode.From("05"), SubunitCode.From("06"),
    };
}
