using Newtonsoft.Json;
using System;

namespace TitanicHook.GUI
{
    internal sealed class ClientRelease
    {
        [JsonProperty("name")]
        public string Name { get; set; } = "";

        [JsonProperty("version")]
        public int Version { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; } = "";

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("known_bugs")]
        public string? KnownBugs { get; set; }

        [JsonProperty("preview")]
        public bool IsPreview { get; set; }

        [JsonProperty("recommended")]
        public bool IsRecommended { get; set; }

        [JsonProperty("download_url")]
        public string? DownloadUrl { get; set; }

        [JsonProperty("checksums")]
        public string[]? Checksums { get; set; }

        [JsonProperty("screenshot")]
        public string? ScreenshotPath { get; set; }

        [JsonProperty("created_at")]
        public string? CreatedAt { get; set; }

        public string? ScreenshotUrl
        {
            get
            {
                string? path = ScreenshotPath;
                if (path == null || path.Length == 0)
                    return null;

                if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    return path;

                return "https://osu.titanic.sh" + path;
            }
        }

        public bool Matches(int version)
        {
            return Version == version;
        }

        public bool Matches(string checksum)
        {
            if (Checksums == null)
                return false;

            foreach (string candidate in Checksums)
                if (string.Equals(candidate, checksum, StringComparison.OrdinalIgnoreCase))
                    return true;

            return false;
        }

        public static ClientRelease[] ParseReleases(string json)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings { DateParseHandling = DateParseHandling.None };
            ClientRelease[]? result = JsonConvert.DeserializeObject<ClientRelease[]>(json, settings);

            if (result == null)
                throw new FormatException("The response contains no releases");

            foreach (ClientRelease release in result)
            {
                if (release == null || string.IsNullOrEmpty(release.Name))
                    throw new FormatException("The response contains an invalid release");

                if (string.IsNullOrEmpty(release.Category))
                    release.Category = "Other";
            }
            return result;
        }
    }
}
