using Coveo.Framework.CNL;
using Newtonsoft.Json.Linq;
using Sitecore.Commerce.Engine.Connect.DataProvider;
using System.Collections.Generic;

namespace Sitecore.HabitatHome.Foundation.CoveoIndexing
{
    public class SellableItemConnector
    {
        private const string LIST_NAME = "SellableItems";
        private const string ENTITY_TYPE_NAME = "Sitecore.Commerce.Plugin.Catalog.SellableItem, Sitecore.Commerce.Plugin.Catalog";
        private const int NUMBER_OF_ENTITIES_TO_TAKE = 2;

        private readonly CatalogRepository m_CatalogRepository;

        public SellableItemConnector(CatalogRepository p_CatalogRepository)
        {
            Precondition.NotNull(p_CatalogRepository, () => () => p_CatalogRepository);

            m_CatalogRepository = p_CatalogRepository;
        }

        public IEnumerable<JToken> GetSellableItems()
        {
            var catalogItemIds = m_CatalogRepository.GetListItemSitecoreIds(LIST_NAME,
                                                                            LIST_NAME,
                                                                            ENTITY_TYPE_NAME,
                                                                            NUMBER_OF_ENTITIES_TO_TAKE);

            foreach (string catalogItemId in catalogItemIds) {
                yield return m_CatalogRepository.GetEntity(catalogItemId);
            }
        }
    }
}