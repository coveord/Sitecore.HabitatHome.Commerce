using System.Collections.Generic;

namespace Sitecore.HabitatHome.Foundation.CoveoIndexing
{
    public class CoveoIndexableCommerceItem : ICoveoIndexableCommerceItem
    {
        public IDictionary<string, object> Metadata { get; } = new Dictionary<string, object>();
    }
}