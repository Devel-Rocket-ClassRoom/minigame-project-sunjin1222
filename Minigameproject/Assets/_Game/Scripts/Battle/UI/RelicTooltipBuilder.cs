public static class RelicTooltipBuilder
{
    private const string RelicNameColor = "#FFD35A";

    public static bool TryBuild(RelicData relic, out string title, out string body)
    {
        title = null;
        body = null;

        if (relic == null)
            return false;

        string relicName = string.IsNullOrWhiteSpace(relic.relicName) ? "유물" : relic.relicName;
        string coloredName = $"<color={RelicNameColor}><b>{relicName}</b></color>";

        body = string.IsNullOrWhiteSpace(relic.description)
            ? coloredName
            : $"{coloredName}\n{relic.description}";
        return true;
    }
}
