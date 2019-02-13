using Newtonsoft.Json;
using System.Collections.Generic;

namespace Sitecore.HabitatHome.Feature.CoveoSearch.UsageAnalytics
{
    public abstract class AbstractAddEvent : AnalyticsEvent
    {
        [JsonIgnore]
        public decimal Price {
            get
            {
                return (decimal) CustomData["price"];
            }
            set
            {
                CustomData["price"] = value;
            }
        }

        [JsonIgnore]
        public string DiscountedPrice
        {
            get
            {
                return (string) CustomData["discountedPrice"];
            }
            set
            {
                CustomData["discountedPrice"] = value;
            }
        }

        [JsonIgnore]
        public IEnumerable<string> Categories
        {
            get
            {
                return (IEnumerable<string>) CustomData["categories"];
            }
            set
            {
                CustomData["categories"] = value;
            }
        }

        [JsonIgnore]
        public string ReportingCategory
        {
            get
            {
                return (string) CustomData["reportingCategory"];
            }
            set
            {
                CustomData["reportingCategory"] = value;
            }
        }

        [JsonIgnore]
        public IEnumerable<string> Brands
        {
            get
            {
                return (IEnumerable<string>) CustomData["brands"];
            }
            set
            {
                CustomData["brands"] = value;
            }
        }

        [JsonIgnore]
        public string ReportingBrand
        {
            get
            {
                return (string) CustomData["reportingBrand"];
            }
            set
            {
                CustomData["reportingBrand"] = value;
            }
        }

        [JsonIgnore]
        public IEnumerable<IEnumerable<string>> Taxonomy
        {
            get
            {
                return (IEnumerable<IEnumerable<string>>) CustomData["taxonomy"];
            }
            set
            {
                CustomData["taxonomy"] = value;
            }
        }

        public AbstractAddEvent(string productId,
                                string productName,
                                decimal price,
                                string language,
                                string referrerUrl)
            : base(productId, productName, language, referrerUrl)
        {
            Price = price;
        }
    }
}