
using System;
using Newtonsoft.Json;

namespace Demo
{

    public class JsonLoad
    {
        public static dynamic ReadJsonString(string jsonStr)
        {
            if (string.IsNullOrWhiteSpace(jsonStr))
            {
                throw new ArgumentException("JSON string cannot be empty or whitespace.", nameof(jsonStr));
            }
            return JsonConvert.DeserializeObject(jsonStr);
        }
    }
}