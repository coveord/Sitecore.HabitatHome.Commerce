using System.Collections.Generic;

namespace Sitecore.HabitatHome.Foundation.CoveoIndexing
{
    public interface ICoveoIndexableCommerceItem
    {
        IDictionary<string, object> Metadata { get; }
    }
}