using System.Collections.Generic;

public static class HelperBotsManager
{
    private static Dictionary<HelperType, int> helperLevels = new();

    public static int GetLevel(HelperType helperParam)
    {
        if (!helperLevels.ContainsKey(helperParam))
            helperLevels[helperParam] = 1;
        return helperLevels[helperParam];
    }

    public static void Upgrade(HelperType helperParam)
    {
        helperLevels[helperParam] = GetLevel(helperParam) + 1;
    }

    public static void SetLevel(HelperType helperParam, int level)
    {
        helperLevels[helperParam] = level;
    }

    public static Dictionary<HelperType, int> GetAllLevels()
    {
        return helperLevels;
    }
}
