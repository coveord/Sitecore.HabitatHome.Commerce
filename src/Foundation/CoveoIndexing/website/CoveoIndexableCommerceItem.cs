using System.Collections.Generic;
using Newtonsoft.Json;

namespace Sitecore.HabitatHome.Foundation.CoveoIndexing
{
    public class CoveoIndexableCommerceItem : ICoveoIndexableCommerceItem
    {
        public IDictionary<string, object> Metadata { get; } = new Dictionary<string, object>();

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}