using Newtonsoft.Json;
using Sitecore.Diagnostics;
using System.Collections.Generic;

namespace Sitecore.HabitatHome.Feature.CoveoSearch.UsageAnalytics
{
    public class AnalyticsEvent
    {
        private JsonSerializer _Serializer = new JsonSerializer();

        [JsonProperty("eventType")]
        public string EventType { get; set; }

        [JsonProperty("eventValue")]
        public string EventValue { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("originLevel1")]
        public string OriginLevel1 { get; set; }

        [JsonProperty("originLevel2")]
        public string OriginLevel2 { get; set; }

        [JsonProperty("originLevel3")]
        public string OriginLevel3 { get; set; }

        [JsonProperty("customData")]
        public Dictionary<string, object> CustomData { get; set; }

        public AnalyticsEvent(string productId,
                              string productName,
                              string language,
                              string referrerUrl)
        {
            Assert.ArgumentNotNullOrEmpty(productId, "productId");
            Assert.ArgumentNotNullOrEmpty(productName, "productName");
            Assert.ArgumentNotNullOrEmpty(language, "language");

            EventValue = productId;
            Language = language;
            OriginLevel1 = "shop";
            OriginLevel2 = "Default";
            CustomData = new Dictionary<string, object>
            {
                { "contentIDKey", "@z95xname" },
                { "contentIDValue", productId },
                { "name", productName }
            };

            if (!string.IsNullOrEmpty(referrerUrl))
            {
                OriginLevel3 = referrerUrl;
            }
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}