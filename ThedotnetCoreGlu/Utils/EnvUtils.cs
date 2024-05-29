using System;

namespace ThedotnetCoreGlu.Utils
{
    public static class TheEnv
    {
        public static string Get(string keyName, string defaultValue = null)
        {
            var value = Environment.GetEnvironmentVariable(keyName);
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }
            return value;
        }
    }
}
