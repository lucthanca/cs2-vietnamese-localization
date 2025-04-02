using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CSII_Vietnamese_Localization.Models
{
    internal class Locale
    {
        [JsonInclude]
        public Dictionary<string, int> IndexCounts { get; set; }
        [JsonInclude]
        public Dictionary<string, string> Entries { get; set; }
        [JsonIgnore]
        public string SystemLocale { get; set; }
        public string LocaleId { get; set; }
        public string LocaleName { get; set; }
        public string Version { get; set; }

        public Locale()
        {
            IndexCounts = new Dictionary<string, int>();
            SystemLocale = string.Empty;
            LocaleId = string.Empty;
            LocaleName = string.Empty;
            Entries = new Dictionary<string, string>();
        }
    }
}
