using Newtonsoft.Json;

namespace Zeus.Api.Models.Resources
{
    public abstract class Resource // : Link // Let's inherit this when we decide to do self referencing
    {
        [JsonIgnore]
        public Link? Self { get; set; }
    }
}
