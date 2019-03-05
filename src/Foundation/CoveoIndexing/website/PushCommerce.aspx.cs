using Sitecore.Commerce.Engine.Connect.DataProvider;
using Sitecore.ContentSearch;
using Sitecore.HabitatHome.Foundation.CoveoIndexing.Extractors;
using System;
using System.Collections.Generic;
using System.Web.UI;

namespace Sitecore.HabitatHome.Foundation.CoveoIndexing
{
    public partial class PushCommerce : Page
    {
        private readonly SellableItemCrawler m_SellableItemCrawler;

        public PushCommerce()
        {
            CatalogRepository catalogRepository = new CatalogRepository();
            var connector = new SellableItemConnector(catalogRepository);
            var productDocumentBuilder = new CommerceDocumentBuilder(new List<IExtractor> {
                new PropertyExtractor(SellableItemEntityProperties.Name,
                                      CoveoIndexableCommerceItemFields.Name),
                new PropertyExtractor(SellableItemEntityProperties.Brand,
                                      CoveoIndexableCommerceItemFields.Brand),
                new PropertyExtractor(SellableItemEntityProperties.Description,
                                      CoveoIndexableCommerceItemFields.Description),
                new PropertyExtractor(SellableItemEntityProperties.ProductId,
                                      CoveoIndexableCommerceItemFields.ProductId),
                new PropertyExtractor(SellableItemEntityProperties.SitecoreId,
                                      CoveoIndexableCommerceItemFields.SitecoreId),
                new PropertyExtractor(SellableItemEntityProperties.Manufacturer,
                                      CoveoIndexableCommerceItemFields.Manufacturer)
            });
            var variantDocumentBuilder = new CommerceDocumentBuilder(new List<IExtractor> {
                new PropertyExtractor(SellableItemEntityProperties.Name,
                                      CoveoIndexableCommerceItemFields.Name),
                new PropertyExtractor(SellableItemEntityProperties.Description,
                                      CoveoIndexableCommerceItemFields.Description),
                new PropertyExtractor(SellableItemEntityProperties.VariantId,
                                      CoveoIndexableCommerceItemFields.VariantId)
            });

            var index = ContentSearchManager.GetIndex("Coveo_sellable_items_index");

            m_SellableItemCrawler = new SellableItemCrawler(connector,
                                                            productDocumentBuilder,
                                                            variantDocumentBuilder,
                                                            index);
        }

        protected void HandlePushButtonClick(object p_Sender,
                                             EventArgs p_Args)
        {
            IEnumerable<string> logs = m_SellableItemCrawler.Crawl();

            foreach (string line in logs)
            {
                LogLine(line);
            }
        }

        private void LogLine(string p_Text)
        {
            LogPanel.InnerHtml += "<div>" + p_Text + "</div>";
        }
    }
}