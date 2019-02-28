using System.Collections.Generic;
using Coveo.AbstractLayer.RepositoryItem;
using Coveo.Framework.CNL;
using Newtonsoft.Json.Linq;
using Sitecore.HabitatHome.Foundation.CoveoIndexing.Extractors;

namespace Sitecore.HabitatHome.Foundation.CoveoIndexing
{
    public class CommerceDocumentBuilder
    {
        private readonly IEnumerable<IExtractor> m_Extractors;

        public CommerceDocumentBuilder(IEnumerable<IExtractor> p_Extractors)
        {
            m_Extractors = p_Extractors;
        }

        public CoveoIndexableItem Build(JToken p_SellableItem)
        {
            Precondition.NotNull(p_SellableItem, () => () => p_SellableItem);

            var indexableItem = new CoveoIndexableItem();
            RunExtractors(p_SellableItem, indexableItem);

            return indexableItem;
        }

        private void RunExtractors(JToken p_SellableItem,
                                   CoveoIndexableItem p_IndexableItem)
        {
            Precondition.NotNull(p_SellableItem, () => () => p_SellableItem);
            Precondition.NotNull(p_IndexableItem, () => () => p_IndexableItem);

            if (m_Extractors != null) {
                foreach (IExtractor extractor in m_Extractors) {
                    if (extractor != null) {
                        RunExtractor(p_SellableItem, p_IndexableItem, extractor);
                    }
                }
            }
        }

        private void RunExtractor(JToken p_SellableItem,
                                  CoveoIndexableItem p_IndexableItem,
                                  IExtractor p_Extractor)
        {
            Precondition.NotNull(p_SellableItem, () => () => p_SellableItem);
            Precondition.NotNull(p_IndexableItem, () => () => p_IndexableItem);
            Precondition.NotNull(p_Extractor, () => () => p_Extractor);

            object extractedValue = p_Extractor.Extract(p_SellableItem);
            if (extractedValue != null) {
                p_IndexableItem.SetMetadata(p_Extractor.OutputMetadataName, extractedValue);
            }
        }
    }
}