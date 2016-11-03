using Newtonsoft.Json;
using System.Collections.Generic;
using System;

namespace Aida.Services.GitHub
{
    public class Release : IComparable<Release>
    {
        public static Release Default
        {
            get
            {
                return new Release()
                {
                    TagName = "Default",
                    Name = "0.0.0.0"
                };
            }
        }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("tag_name")]
        public string TagName { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("assets")]
        public IEnumerable<Asset> Assets { get; set; }

        public int CompareTo(Release other)
        {
            return CompareTo(other.Name);
        }

        public int CompareTo(string other)
        {
            return new VersionComparer().Compare(Name, other);
        }
    }
}
