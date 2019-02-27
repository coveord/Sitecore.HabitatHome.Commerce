using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Sitecore.Commerce.Engine.Connect.DataProvider;

namespace Sitecore.HabitatHome.Foundation.CoveoIndexing
{
    public class SellableItemCrawler
    {
        public IEnumerable<JToken> GetSellableItems()
        {
            var repo = new CatalogRepository();
            var catalogItemIds = repo.GetListItems("SellableItems", "SellableItems", "Sitecore.Commerce.Plugin.Catalog.SellableItem, Sitecore.Commerce.Plugin.Catalog", 2);
            foreach (string catalogItemId in catalogItemIds) {
                yield return repo.GetEntity(catalogItemId);
            }
        }
    }
}