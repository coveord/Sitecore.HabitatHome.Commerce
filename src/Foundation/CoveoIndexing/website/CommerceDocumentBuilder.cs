using Newtonsoft.Json.Linq;

namespace Sitecore.HabitatHome.Foundation.CoveoIndexing
{
    public class CommerceDocumentBuilder
    {
        public ICoveoIndexableCommerceItem Build(JToken p_SellableItem)
        {
            return new CoveoIndexableCommerceItem {
                Name = ExtractProperty(p_SellableItem, SellableItemEntityProperties.Name),
                Brand = ExtractProperty(p_SellableItem, SellableItemEntityProperties.Brand)
            };
        }

        private string ExtractProperty(JToken p_SellableItem, string p_PropertyName)
        {
            return p_SellableItem[p_PropertyName].Value<string>();
        }
    }
}