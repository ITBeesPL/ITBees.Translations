using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace ITBees.Translations
{
    class JsonKeyValueService
    {
        public Dictionary<string, string> GenerateKeyValueDictionary(string json)
        {
            JObject jsonObject = JObject.Parse(json);
            var keyValueDictionary = new Dictionary<string, string>();

            foreach (var property in jsonObject.Properties())
            {
                string rootKey = property.Name;
                ParseJsonProperties(property.Value, keyValueDictionary, rootKey);
            }

            return keyValueDictionary;
        }

        private void ParseJsonProperties(JToken token, Dictionary<string, string> keyValueDictionary, string currentPath)
        {
            if (token is JObject obj)
            {
                foreach (var property in obj.Properties())
                {
                    string newPath = $"{property.Name}";
                    ParseJsonProperties(property.Value, keyValueDictionary, newPath);
                }
            }
            else if (token is JValue value && value.Type == JTokenType.String)
            {
                keyValueDictionary.Add(currentPath, value.ToString());
            }
        }
    }
}