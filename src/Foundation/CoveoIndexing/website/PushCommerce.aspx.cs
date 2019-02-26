using System;
using System.Web.UI;

namespace Sitecore.HabitatHome.Foundation.CoveoIndexing
{
    public partial class PushCommerce : Page
    {
        private SellableItemCrawler m_Crawler;

        public PushCommerce()
        {
            m_Crawler = new SellableItemCrawler();
        }

        protected void Page_Load(object p_Sender, EventArgs p_Args)
        {
        }

        protected void HandlePushButtonClick(object p_Sender,
                                             EventArgs p_Args)
        {
            CrawlSellableItems();
        }

        private void CrawlSellableItems()
        {
            foreach (string sellableItem in m_Crawler.GetSellableItems()) {
                LogLine(sellableItem);
            }
        }

        private void LogLine(string p_Text)
        {
            LogPanel.InnerHtml += "<div>" + p_Text + "</div>";
        }
    }
}