using System;
using Newtonsoft.Json;

namespace Demo
{
    /// <summary>
    /// Small utility wrapper around Newtonsoft.Json deserialization used across the project.
    /// Kept intentionally simple - public API is unchanged.
    /// </summary>
    public class JsonLoad
    {
        /// <summary>
        /// Deserialize a JSON string to a dynamic object. Throws ArgumentException when input is null/empty.
        /// </summary>
        /// <param name="jsonStr">JSON content</param>
        /// <returns>Deserialized dynamic object</returns>
        public static dynamic ReadJsonString(string jsonStr)
        {
            if (string.IsNullOrWhiteSpace(jsonStr))
                throw new ArgumentException("JSON string cannot be empty or whitespace.", nameof(jsonStr));

            return JsonConvert.DeserializeObject(jsonStr);
        }
    }
}