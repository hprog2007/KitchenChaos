using System.Collections.Generic;

public static class CounterUpgradeManager
{
    private static Dictionary<CounterType, int> counterLevels = new();

    public static int GetLevel(CounterType type)
    {
        if (!counterLevels.ContainsKey(type))
            counterLevels[type] = 1;
        return counterLevels[type];
    }

    public static void Upgrade(CounterType type)
    {
        counterLevels[type] = GetLevel(type) + 1;
    }

    public static void SetLevel(CounterType type, int level)
    {
        counterLevels[type] = level;
    }

    public static Dictionary<CounterType, int> GetAllLevels()
    {
        return counterLevels;
    }
}
