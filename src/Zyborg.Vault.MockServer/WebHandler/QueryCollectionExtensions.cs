using Microsoft.AspNetCore.Http;

namespace Zyborg.Vault.MockServer.WebHandler
{
    public static class QueryCollectionExtensions
    {
        public static int GetInt(this IQueryCollection q, string name, int defaultValue = 0)
        {
            if (q.TryGetValue(name, out var values)
                    && int.TryParse(values[0], out var intValue))
                return intValue;
            return defaultValue;
        }

        public static long GetLong(this IQueryCollection q, string name, long defaultValue = 0L)
        {
            if (q.TryGetValue(name, out var values)
                    && long.TryParse(values[0], out var longValue))
                return longValue;
            return defaultValue;
        }

        public static bool GetBool(this IQueryCollection q, string name, bool defaultValue = false)
        {
            if (q.TryGetValue(name, out var values)
                    && bool.TryParse(values[0], out var boolValue))
                return boolValue;
            return defaultValue;
        }

        public static bool GetBoolOrInt(this IQueryCollection q, string name, bool defaultValue = false)
        {
            if (q.TryGetValue(name, out var values))
            {
                if (bool.TryParse(values[0], out var boolValue))
                    return boolValue;
                if (int.TryParse(values[0], out var intValue))
                    return intValue == 0 ? false : true;
            }
            return defaultValue;
        }
    }
}