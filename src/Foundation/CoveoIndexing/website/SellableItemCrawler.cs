using Coveo.AbstractLayer.Communication.Index;
using Coveo.AbstractLayer.FieldManagement;
using Coveo.AbstractLayer.RepositoryItem;
using Coveo.AbstractLayer.Security;
using Coveo.CloudPlatformClient.Conversion;
using Coveo.CloudPlatformClient.DocumentManagement;
using Coveo.CloudPlatformClient.DocumentManagement.Permissions;
using Coveo.CloudPlatformClient.FieldsManagement;
using Coveo.CloudPlatformClient.IndexAdministration;
using Coveo.CloudPlatformClient.SecurityCacheManagement;
using Coveo.CloudPlatformClient.SecurityProviderManagement;
using Coveo.CloudPlatformClient.SourceManagement;
using Coveo.CloudPlatformClient.ThreadPool;
using Coveo.CloudPlatformClientBase;
using Coveo.CloudPlatformClientBase.Communication;
using Coveo.CloudPlatformClientBase.DocumentManagement.Permissions;
using Coveo.Framework;
using Coveo.Framework.Caching;
using Coveo.Framework.CNL;
using Coveo.Framework.Compression;
using Coveo.Framework.Configuration;
using Coveo.Framework.Databases;
using Coveo.Framework.Fields;
using Coveo.Framework.Pipelines;
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
        private readonly IFieldNameTranslator m_FieldNameTranslator;
        private readonly ICloudPlatformAdminModule m_CloudPlatformAdminModule;
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
            ICoveoFieldMap fieldMap = (ICoveoFieldMap) configuration.FieldMap;
            m_FieldNameTranslator = new CoveoFieldNameTranslator {
                FieldMap = fieldMap,
                IndexNamesBuilder = indexNamesBuilder
            };
            ICloudPlatformClient cloudPlatformClient = new CloudPlatformClientFactory().GetCloudPlatformClient(configuration.CloudPlatformConfiguration);

            IEnumerable<IDatabaseWrapper> databaseWrappers = new IDatabaseWrapper[] { new DatabaseWrapper("master") };
            SitecoreContextWrapper sitecoreContextWrapper = new SitecoreContextWrapper();
            IFacetItemsFetcher facetItemsFetcher = new FacetItemsFetcher(sitecoreContextWrapper);
            PipelineRunner pipelineRunner = new PipelineRunner();
            PipelineRunnerHandler pipelineRunnerHandler = new PipelineRunnerHandler(pipelineRunner);
            IFieldFetcherFactory fieldFetcherFactory = new FieldFetcherFactory(pipelineRunnerHandler,
                                                                               true);
            ISitecoreFactory sitecoreFactoryWrapper = new SitecoreFactoryWrapper();
            FieldsCacheHandler fieldsCacheHandler = new FieldsCacheHandler(sitecoreFactoryWrapper);
            IFieldsHandlerUtility fieldsHandlerUtility = new FieldsHandlerUtility(configuration,
                                                                                  databaseWrappers, 
                                                                                  m_IndexName,
                                                                                  facetItemsFetcher,
                                                                                  fieldFetcherFactory,
                                                                                  m_FieldNameTranslator,
                                                                                  fieldMap,
                                                                                  pipelineRunnerHandler,
                                                                                  fieldsCacheHandler);
            IFieldsHandlerUtility cachedFieldsHandlerUtility = new CachedFieldsHandlerUtility(fieldsHandlerUtility);
            ICloudFieldConfigConverter cloudFieldConfigConverter = new CloudFieldConfigConverter(";");
            IndexDatabaseProperties indexDatabaseProperties = new IndexDatabaseProperties(p_Index.PropertyStore);
            ICloudPlatformFieldsHandler cloudPlatformFieldsHandler = new CloudPlatformFieldsHandler(cachedFieldsHandlerUtility,
                                                                                                    cloudFieldConfigConverter,
                                                                                                    indexDatabaseProperties,
                                                                                                    cloudPlatformClient,
                                                                                                    100);
            ICloudPlatformSecurityProviderHandler cloudPlatformSecurityProviderHandler = new CloudPlatformSecurityProviderHandler(cloudPlatformClient,
                                                                                                                                  indexDatabaseProperties);
            ICloudPlatformSecurityCacheHandler cloudPlatformSecurityCacheHandler = new CloudPlatformSecurityCacheHandler(cloudPlatformClient);
            ICloudPlatformSourceHandler cloudPlatformSourceHandler = new CloudPlatformSourceHandler(cloudPlatformClient);

            m_CloudPlatformAdminModule = new CloudPlatformAdminModule(configuration,
                                                                      cloudPlatformFieldsHandler,
                                                                      cloudPlatformSecurityProviderHandler,
                                                                      cloudPlatformSecurityCacheHandler,
                                                                      cloudPlatformSourceHandler,
                                                                      m_FieldNameTranslator);

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
                                                                   fieldMap,
                                                                   cloudPlatformClient,
                                                                   cloudIndexableDocumentFactory,
                                                                   threadHandlerFactory,
                                                                   threadHandlerUtility,
                                                                   threadContextFactory);
        }

        public IEnumerable<string> Crawl()
        {
            List<string> logs = new List<string>();

            logs.Add("Setup Requirements...");
            m_CloudPlatformAdminModule.SetupRequirements();
            logs.Add("Done Setup Requirements");

            RebuildContext rebuildContext = CreateRebuildContext();

            logs.Add("Starting Rebuild...");
            m_DocumentsHandler.StartRebuild(rebuildContext);
            logs.Add("Rebuild Started");

            logs.Add("Pushing items...");
            IEnumerable<string> itemLogs = GetSellableItems().Select(sellableItem => CrawlSellableItem(sellableItem, rebuildContext));
            logs.AddRange(itemLogs);
            logs.Add("Done Pushing items");

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
                FieldNameTranslator = m_FieldNameTranslator,
                LastRebuildDate = DateTime.Now.AddSeconds(-10)
            };
            return rebuildContext;
        }
    }
}