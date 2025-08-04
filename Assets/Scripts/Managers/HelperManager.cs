using System.Collections.Generic;

public static class HelperManager
{
    private static Dictionary<string, int> helperLevels = new();

    public static int GetLevel(string helperName)
    {
        if (!helperLevels.ContainsKey(helperName))
            helperLevels[helperName] = 1;
        return helperLevels[helperName];
    }

    public static void Upgrade(string helperName)
    {
        helperLevels[helperName] = GetLevel(helperName) + 1;
    }

    public static void SetLevel(string helperName, int level)
    {
        helperLevels[helperName] = level;
    }

    public static Dictionary<string, int> GetAllLevels()
    {
        return helperLevels;
    }
}
