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
using Coveo.Framework.CNL;
using Coveo.Framework.Compression;
using Coveo.Framework.Configuration;
using Coveo.Framework.Fields;
using Coveo.Framework.Utils;
using Coveo.SearchProvider;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sitecore.ContentSearch;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sitecore.HabitatHome.Foundation.CoveoIndexing
{
    public class SellableItemCrawler
    {
        private readonly SellableItemConnector m_Connector;
        private readonly CommerceDocumentBuilder m_DocumentBuilder;
        private readonly ICoveoFieldMap m_FieldMap;
        private readonly CloudPlatformDocumentsHandler m_DocumentsHandler;
        private readonly string m_IndexName;
        private readonly string m_SourceName;

        public SellableItemCrawler(SellableItemConnector p_Connector,
                                   CommerceDocumentBuilder p_DocumentBuilder,
                                   ISearchIndex p_Index)
        {
            Precondition.NotNull(p_Connector, () => () => p_Connector);
            Precondition.NotNull(p_DocumentBuilder, () => () => p_DocumentBuilder);
            Precondition.NotNull(p_Index, () => () => p_Index);

            m_Connector = p_Connector;
            m_DocumentBuilder = p_DocumentBuilder;

            m_IndexName = p_Index.Name;
            IUrlUtilities urlUtilities = new UrlUtilities();
            ProviderIndexHelperFactory providerIndexHelperFactory = new ProviderIndexHelperFactory();
            IIndexNamesBuilder indexNamesBuilder = providerIndexHelperFactory.CreateIndexNamesBuilder(m_IndexName, urlUtilities);
            m_SourceName = indexNamesBuilder.BuildSourceName();

            CoveoIndexConfiguration configuration = (CoveoIndexConfiguration) p_Index.Configuration;
            m_FieldMap = (ICoveoFieldMap) configuration.FieldMap;
            ICloudPlatformClient cloudPlatformClient = new CloudPlatformClientFactory().GetCloudPlatformClient(configuration.CloudPlatformConfiguration);
            ICompressedStreamHandler compressedStreamHandler = new CompressedStreamHandler();
            IPermissionsProvider permissionsProvider = new PermissionsProvider(configuration.SecurityConfiguration.AnonymousUsers);
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

        public IEnumerable<string> Crawl()
        {
            List<string> logs = new List<string>();

            RebuildContext rebuildContext = CreateRebuildContext();

            logs.Add("Starting Rebuild...");
            m_DocumentsHandler.StartRebuild(rebuildContext);

            IEnumerable<string> itemLogs = GetSellableItems().Select(sellableItem => CrawlSellableItem(sellableItem, rebuildContext));
            logs.AddRange(itemLogs);

            logs.Add("Stopping Rebuild...");
            m_DocumentsHandler.StopRebuild(rebuildContext);
            logs.Add("Rebuild Done");

            return logs;
        }

        private string CrawlSellableItem(JToken p_SellableItem,
                                         RebuildContext p_RebuildContext)
        {
            CoveoIndexableItem indexableSellableItem = m_DocumentBuilder.Build(p_SellableItem);

            string productId = (string) indexableSellableItem.Metadata[CoveoIndexableCommerceItemFields.ProductId];
            string uri = $"http://commerce/{productId}";

            indexableSellableItem.ClickableUri = uri;
            indexableSellableItem.Id = uri;
            indexableSellableItem.Title = (string) indexableSellableItem.Metadata[CoveoIndexableCommerceItemFields.Name];
            indexableSellableItem.UniqueId = productId;
            indexableSellableItem.Uri = uri;
            indexableSellableItem.SetMetadata(PropertyStoreConstants.LAST_REBUILD_METADATA_KEY,
                                              p_RebuildContext.LastRebuildDate.ToIndexFormat());

            m_DocumentsHandler.AddDocument(indexableSellableItem,
                                           m_SourceName,
                                           uri);

            return JsonConvert.SerializeObject(indexableSellableItem, Formatting.Indented);
        }

        private IEnumerable<JToken> GetSellableItems()
        {
            return m_Connector.GetSellableItems();
        }

        protected RebuildContext CreateRebuildContext()
        {
            RebuildContext rebuildContext = new RebuildContext {
                SourceName = m_SourceName,
                IndexName = m_IndexName,
                FieldNameTranslator = new CoveoFieldNameTranslator {
                    FieldMap = m_FieldMap
                },
                LastRebuildDate = DateTime.Now.AddSeconds(-10)
            };
            return rebuildContext;
        }
    }
}