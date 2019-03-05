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
using Coveo.Framework.Collections;

namespace Sitecore.HabitatHome.Foundation.CoveoIndexing
{
    public class SellableItemCrawler
    {
        private readonly SellableItemConnector m_Connector;
        private readonly CommerceDocumentBuilder m_ProductDocumentBuilder;
        private readonly CommerceDocumentBuilder m_VariantDocumentBuilder;
        private readonly IFieldNameTranslator m_FieldNameTranslator;
        private readonly ICloudPlatformAdminModule m_CloudPlatformAdminModule;
        private readonly CloudPlatformDocumentsHandler m_DocumentsHandler;
        private readonly string m_IndexName;
        private readonly string m_SourceName;

        public SellableItemCrawler(SellableItemConnector p_Connector,
                                   CommerceDocumentBuilder p_ProductDocumentBuilder,
                                   CommerceDocumentBuilder p_VariantDocumentBuilder,
                                   ISearchIndex p_Index)
        {
            Precondition.NotNull(p_Connector, () => () => p_Connector);
            Precondition.NotNull(p_ProductDocumentBuilder, () => () => p_ProductDocumentBuilder);
            Precondition.NotNull(p_VariantDocumentBuilder, () => () => p_VariantDocumentBuilder);
            Precondition.NotNull(p_Index, () => () => p_Index);

            m_Connector = p_Connector;
            m_ProductDocumentBuilder = p_ProductDocumentBuilder;
            m_VariantDocumentBuilder = p_VariantDocumentBuilder;

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
            IEnumerable<string> itemLogs = CrawlSellableItems(rebuildContext);
            logs.AddRange(itemLogs);
            logs.Add("Done Pushing items");

            logs.Add("Stopping Rebuild...");
            m_DocumentsHandler.StopRebuild(rebuildContext);
            logs.Add("Rebuild Done");

            return logs;
        }

        private RebuildContext CreateRebuildContext()
        {
            RebuildContext rebuildContext = new RebuildContext
            {
                SourceName = m_SourceName,
                IndexName = m_IndexName,
                FieldNameTranslator = m_FieldNameTranslator,
                LastRebuildDate = DateTime.Now.AddSeconds(-10)
            };
            return rebuildContext;
        }

        private IEnumerable<string> CrawlSellableItems(RebuildContext p_RebuildContext)
        {
            return GetSellableItems().Select(sellableItem => CrawlSellableItem(sellableItem, p_RebuildContext));
        }

        private IEnumerable<JToken> GetSellableItems()
        {
            return m_Connector.GetSellableItems();
        }

        private string CrawlSellableItem(JToken p_SellableItem,
                                         RebuildContext p_RebuildContext)
        {
            CoveoIndexableItem indexableSellableItem = m_ProductDocumentBuilder.Build(p_SellableItem);

            string name = (string) indexableSellableItem.Metadata[CoveoIndexableCommerceItemFields.Name];
            string productId = (string) indexableSellableItem.Metadata[CoveoIndexableCommerceItemFields.ProductId];
            string uri = $"http://commerce/{productId}";
            string brand = "";
            try {
                brand = (string) indexableSellableItem.Metadata[CoveoIndexableCommerceItemFields.Brand];
            } catch {
                // Ignore exceptions
            }

            indexableSellableItem.ClickableUri = uri;
            indexableSellableItem.Id = uri;
            indexableSellableItem.Title = name;
            indexableSellableItem.UniqueId = productId;
            indexableSellableItem.Uri = uri;
            indexableSellableItem.SetMetadata(PropertyStoreConstants.LAST_REBUILD_METADATA_KEY,
                                              p_RebuildContext.LastRebuildDate.ToIndexFormat());

            m_DocumentsHandler.AddDocument(indexableSellableItem,
                                           m_SourceName,
                                           uri);


            string crawledVariants = CrawlSellableItemVariants(p_SellableItem, p_RebuildContext, productId, brand);

            return JsonConvert.SerializeObject(indexableSellableItem, Formatting.Indented) + crawledVariants;
        }

        private string CrawlSellableItemVariants(JToken p_SellableItem,
                                                 RebuildContext p_RebuildContext,
                                                 string p_ProductId,
                                                 string p_Brand)
        {
            string crawledVariants = "";

            GetSellableItemVariants(p_SellableItem).ForEach(sellableItemVariant => {
                crawledVariants += CrawlSellableItemVariant(sellableItemVariant, p_RebuildContext, p_ProductId, p_Brand);
            });

            return crawledVariants;
        }

        private IEnumerable<JToken> GetSellableItemVariants(JToken p_SellableItem)
        {
            return p_SellableItem.SelectTokens("$.Components[?(@['@odata.type'] == '#Sitecore.Commerce.Plugin.Catalog.ItemVariationsComponent')].ChildComponents[0]", false);
        }

        private string CrawlSellableItemVariant(JToken p_SellableItemVariant,
                                                RebuildContext p_RebuildContext,
                                                string p_ProductId,
                                                string p_Brand)
        {
            CoveoIndexableItem indexableSellableItemVariant = m_VariantDocumentBuilder.Build(p_SellableItemVariant);

            string variantName = (string) indexableSellableItemVariant.Metadata[CoveoIndexableCommerceItemFields.Name];
            string variantId = (string) indexableSellableItemVariant.Metadata[CoveoIndexableCommerceItemFields.VariantId];
            string variantUri = $"http://commerce/{variantId}";

            indexableSellableItemVariant.ClickableUri = variantUri;
            indexableSellableItemVariant.Id = variantUri;
            indexableSellableItemVariant.Title = variantName;
            indexableSellableItemVariant.UniqueId = variantId;
            indexableSellableItemVariant.Uri = variantUri;
            indexableSellableItemVariant.SetMetadata(PropertyStoreConstants.LAST_REBUILD_METADATA_KEY,
                                                     p_RebuildContext.LastRebuildDate.ToIndexFormat());

            indexableSellableItemVariant.SetMetadata(CoveoIndexableCommerceItemFields.ProductId, p_ProductId);
            indexableSellableItemVariant.SetMetadata(CoveoIndexableCommerceItemFields.Brand, p_Brand);

            m_DocumentsHandler.AddDocument(indexableSellableItemVariant,
                                           m_SourceName,
                                           variantUri);

            return JsonConvert.SerializeObject(indexableSellableItemVariant, Formatting.Indented);
        }
    }
}