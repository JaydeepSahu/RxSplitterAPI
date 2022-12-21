using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace WebAPI.Localization
{
    public class JsonStringLocalizer : IStringLocalizer
    {
        private readonly IDistributedCache _distributedCache;
        private readonly JsonSerializer _jsonSerializer = new JsonSerializer();
        public JsonStringLocalizer(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        public LocalizedString this[string name]
        {
            get
            {
                // Get the value from the localization JSON file
                string value = GetLocalizedString(name);

                // return that localized string
                return new LocalizedString(name, value ?? name, value == null);
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                // Set the name of the localized string as the actual value
                var theActualValue = this[name];

                // Check if the string was not found. If true an alternate string was found
                return !theActualValue.ResourceNotFound
                    ? new LocalizedString(name, string.Format(theActualValue.Value, arguments), false)
                    : theActualValue;
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            // Get file path for JSON file
            string filePath = $"Resources/{Thread.CurrentThread.CurrentCulture.Name}.json";

            // Let's read some text
            using (var str = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var sr = new StreamReader(str))
            using (var reader = new JsonTextReader(sr))
            {
                // While we got more lines to read
                while (reader.Read())
                {
                    // Check if the token matches the property name
                    if (reader.TokenType != JsonToken.PropertyName)
                        continue;

                    // Read the key value as a string (might return null)
                    string? key = reader.Value as string;

                    // Read
                    reader.Read();

                    // Deserialize the found string (might return null)
                    string? value = _jsonSerializer.Deserialize<string>(reader);

                    // return an IEnumerable<> of LocalizedStrings containing the cache key and the strings. false = string was found
                    yield return new LocalizedString(key, value, false);
                }
            }
        }
        private string? GetJsonValue(string propertyName, string filePath)
        {
            // If the properte and filepath is null, return null
            if (propertyName == null) return default;
            if (filePath == null) return default;

            // Let's read some text from the JSON file
            using (var str = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var sReader = new StreamReader(str))
            using (var reader = new JsonTextReader(sReader))
            {
                // While we still got more lines in the JSON file
                while (reader.Read())
                {
                    // Check if the property name matches the current line
                    if (reader.TokenType == JsonToken.PropertyName && reader.Value as string == propertyName)
                    {
                        // If it's the right line, then read it and deserialize it into a string and return that
                        reader.Read();
                        return _jsonSerializer.Deserialize<string>(reader);
                    }
                }

                return default;
            }
        }

        private string GetLocalizedString(string key)
        {

            // Set path for JSON files
            string relativeFilePath = $"Resources/{Thread.CurrentThread.CurrentCulture.Name}.json";
            string fullFilePath = Path.GetFullPath(relativeFilePath);

            // Check if the file exists
            if (File.Exists(fullFilePath))
            {
                // Declare cache key and the cache value to the distributed cache
                string cacheKey = $"locale_{Thread.CurrentThread.CurrentCulture.Name}_{key}";
                string cacheValue = _distributedCache.GetString(cacheKey);

                // If the string is not null/empty then return the already cached value
                if (!string.IsNullOrEmpty(cacheValue)) return cacheValue;

                // If the string was null, then we look up the property in the JSON file
                string result = GetJsonValue(key, filePath: Path.GetFullPath(relativeFilePath));

                // If we find the property inside the JSON file we update the cache with that result
                if (!string.IsNullOrEmpty(result)) _distributedCache.SetString(cacheKey, result);

                // Return the found string
                return result;
            }

            // If file was not found, return null
            return default;

        }
    }
}
