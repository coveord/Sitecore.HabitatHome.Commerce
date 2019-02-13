using Newtonsoft.Json;
using Sitecore.Diagnostics;

namespace Sitecore.HabitatHome.Feature.CoveoSearch.UsageAnalytics
{
    public abstract class AbstractAddWithQuantityEvent : AbstractAddEvent
    {
        [JsonIgnore]
        public decimal Quantity
        {
            get
            {
                return (decimal) CustomData["quantity"];
            }
            set
            {
                CustomData["quantity"] = value;
            }
        }

        [JsonIgnore]
        public string CartId
        {
            get
            {
                return (string) CustomData["cartId"];
            }
            set
            {
                CustomData["cartId"] = value;
            }
        }

        public AbstractAddWithQuantityEvent(string productId,
                                            string productName,
                                            decimal quantity,
                                            decimal price,
                                            string cartId,
                                            string language,
                                            string referrerUrl)
            : base(productId, productName, price, language, referrerUrl)
        {
            Assert.ArgumentNotNullOrEmpty(cartId, "cartId");
            
            Quantity = quantity;
            CartId = cartId;
        }
    }
}