public static class CardTooltipBuilder
{
    public static bool TryBuild(CardData cardData, out string title, out string body)
    {
        body = null;
        title = null;
        if (cardData == null || string.IsNullOrWhiteSpace(cardData.tooltipText))
            return false;
        body = cardData.tooltipText;
        return true;
    }
}
