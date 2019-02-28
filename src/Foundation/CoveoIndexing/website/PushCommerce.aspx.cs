using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using Coveo.AbstractLayer.Communication.Index;
using Coveo.AbstractLayer.RepositoryItem;
using Coveo.AbstractLayer.Security;
using Coveo.CloudPlatformClient.DocumentManagement;
using Coveo.CloudPlatformClient.DocumentManagement.Permissions;
using Coveo.CloudPlatformClient.ThreadPool;
using Coveo.CloudPlatformClientBase;
using Coveo.CloudPlatformClientBase.Communication;
using Coveo.CloudPlatformClientBase.DocumentManagement.Permissions;
using Coveo.Framework;
using Coveo.Framework.Collections;
using Coveo.Framework.Compression;
using Coveo.Framework.Configuration;
using Coveo.Framework.Fields;
using Coveo.Framework.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sitecore.ContentSearch;
using Sitecore.HabitatHome.Foundation.CoveoIndexing.Extractors;

namespace Sitecore.HabitatHome.Foundation.CoveoIndexing
{
    public partial class PushCommerce : Page
    {
        private readonly SellableItemCrawler m_Crawler;
        private readonly CommerceDocumentBuilder m_DocumentBuilder;
        private CloudPlatformDocumentsHandler m_DocumentsHandler;
        private ICoveoFieldMap m_FieldMap;
        private ISearchIndex m_Index;

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

            m_Index = ContentSearchManager.GetIndex("Coveo_sellable_items_index");
            CoveoIndexConfiguration configuration = (CoveoIndexConfiguration) m_Index.Configuration;
            m_FieldMap = (ICoveoFieldMap) configuration.FieldMap;
            ICloudPlatformClient cloudPlatformClient = new CloudPlatformClientFactory().GetCloudPlatformClient(configuration.CloudPlatformConfiguration);
            ICompressedStreamHandler compressedStreamHandler = new CompressedStreamHandler();
            IPermissionsProvider permissionsProvider = new PermissionsProvider("sitecore\\anonymous");
            ISecurityModelMapper<IList<CloudPermissionLevel>> cloudSecurityMapper = new CloudSecurityMapper();
            ICoveoIndexableItemHelper coveoIndexableItemHelper = new CoveoIndexableItemHelper();
            ICloudIndexableDocumentFactory cloudIndexableDocumentFactory = new CloudIndexableDocumentFactory(compressedStreamHandler,
                                                                                                             permissionsProvider,
                                                                                                             cloudSecurityMapper,
                                                                                                             coveoIndexableItemHelper);
            IThreadHandlerFactory threadHandlerFactory = new ThreadHandlerFactory();
            IThreadHandlerUtility threadHandlerUtility = new ThreadHandlerUtility();
            IThreadContextFactory threadContextFactory = new ThreadContextFactory();
            m_DocumentsHandler = new CloudPlatformDocumentsHandler(configuration,
                                                                   m_FieldMap,
                                                                   cloudPlatformClient,
                                                                   cloudIndexableDocumentFactory,
                                                                   threadHandlerFactory,
                                                                   threadHandlerUtility,
                                                                   threadContextFactory);
        }

        protected void HandlePushButtonClick(object p_Sender,
                                             EventArgs p_Args)
        {
            IEnumerable<JToken> sellableItems = m_Crawler.GetSellableItems();

            IEnumerable<CoveoIndexableItem> indexableSellableItems = sellableItems.Select(sellableItem => m_DocumentBuilder.Build(sellableItem));
            LogIndexableItems(indexableSellableItems);

            LogLine("Starting Rebuild");
            RebuildContext rebuildContext = new RebuildContext {
                SourceName = "Coveo_sellable_items_index - WKS-000615-habitathome.dev.local",
                IndexName = m_Index.Name,
                FieldNameTranslator = new CoveoFieldNameTranslator {
                    FieldMap = m_FieldMap
                },
                LastRebuildDate = DateTime.Now.AddSeconds(-10)
            };
            m_DocumentsHandler.StartRebuild(rebuildContext);

            indexableSellableItems.ForEach(indexableSellableItem => {
                string productId = (string) indexableSellableItem.Metadata[CoveoIndexableCommerceItemFields.ProductId];
                string uri = $"http://commerce/{productId}";

                indexableSellableItem.ClickableUri = uri;
                indexableSellableItem.Id = uri;
                indexableSellableItem.Title = (string) indexableSellableItem.Metadata[CoveoIndexableCommerceItemFields.Name];
                indexableSellableItem.UniqueId = productId;
                indexableSellableItem.Uri = uri;
                indexableSellableItem.SetMetadata(PropertyStoreConstants.LAST_REBUILD_METADATA_KEY, rebuildContext.LastRebuildDate.ToIndexFormat());

                m_DocumentsHandler.AddDocument(indexableSellableItem,
                                               "Coveo_sellable_items_index - WKS-000615-habitathome.dev.local",
                                               uri);
            });

            LogLine("Stopping Rebuild");
            m_DocumentsHandler.StopRebuild(rebuildContext);
        }

        private void LogSellableItems(IEnumerable<JToken> p_Items)
        {
            foreach (JToken item in p_Items)
            {
                LogLine(item.ToString());
            }
        }

        private void LogIndexableItems(IEnumerable<CoveoIndexableItem> p_Items)
        {
            foreach (CoveoIndexableItem item in p_Items) {
                LogLine(JsonConvert.SerializeObject(item, Formatting.Indented));
            }
        }

        private void LogLine(string p_Text)
        {
            LogPanel.InnerHtml += "<div>" + p_Text + "</div>";
        }
    }
}