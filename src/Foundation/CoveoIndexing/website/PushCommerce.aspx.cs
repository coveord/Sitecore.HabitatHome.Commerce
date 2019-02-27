using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using Newtonsoft.Json.Linq;
using Sitecore.HabitatHome.Foundation.CoveoIndexing.Extractors;

namespace Sitecore.HabitatHome.Foundation.CoveoIndexing
{
    public partial class PushCommerce : Page
    {
        private SellableItemCrawler m_Crawler;
        private CommerceDocumentBuilder m_DocumentBuilder;

        public PushCommerce()
        {
            m_Crawler = new SellableItemCrawler();
            m_DocumentBuilder = new CommerceDocumentBuilder(new List<IExtractor> {
                new PropertyExtractor(SellableItemEntityProperties.Name, CoveoIndexableCommerceItemFields.Name),
                new PropertyExtractor(SellableItemEntityProperties.Brand, CoveoIndexableCommerceItemFields.Brand),
                new PropertyExtractor(SellableItemEntityProperties.Description, CoveoIndexableCommerceItemFields.Description),
                new PropertyExtractor(SellableItemEntityProperties.ProductId, CoveoIndexableCommerceItemFields.ProductId),
                new PropertyExtractor(SellableItemEntityProperties.SitecoreId, CoveoIndexableCommerceItemFields.SitecoreId),
                new PropertyExtractor(SellableItemEntityProperties.Manufacturer, CoveoIndexableCommerceItemFields.Manufacturer)
            });
        }

        protected void Page_Load(object p_Sender,
                                 EventArgs p_Args)
        {
        }

        protected void HandlePushButtonClick(object p_Sender,
                                             EventArgs p_Args)
        {
            IEnumerable<JToken> sellableItems = m_Crawler.GetSellableItems();

            IEnumerable<ICoveoIndexableCommerceItem> indexableSellableItems = sellableItems.Select(sellableItem => m_DocumentBuilder.Build(sellableItem));
            LogIndexableItems(indexableSellableItems);
        }

        private void LogSellableItems(IEnumerable<JToken> p_Items)
        {
            foreach (JToken item in p_Items)
            {
                LogLine(item.ToString());
            }
        }

        private void LogIndexableItems(IEnumerable<ICoveoIndexableCommerceItem> p_Items)
        {
            foreach (ICoveoIndexableCommerceItem item in p_Items) {
                LogLine(item.ToString());
            }
        }

        private void LogLine(string p_Text)
        {
            LogPanel.InnerHtml += "<div>" + p_Text + "</div>";
        }
    }
}