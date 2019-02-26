using System.Collections.Generic;
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

        public ICoveoIndexableCommerceItem Build(JToken p_SellableItem)
        {
            var indexableItem = new CoveoIndexableCommerceItem();
            RunExtractors(p_SellableItem, indexableItem);

            return indexableItem;
        }

        private void RunExtractors(JToken p_SellableItem, ICoveoIndexableCommerceItem p_IndexableItem)
        {
            foreach (IExtractor extractor in m_Extractors) {
                object extractedValue = extractor.Extract(p_SellableItem);
                if (extractedValue != null) {
                    p_IndexableItem.Metadata.Add(extractor.OutputMetadataName, extractedValue);
                }
            }
        }
    }
}