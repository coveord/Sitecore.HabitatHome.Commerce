using Newtonsoft.Json.Linq;

namespace Sitecore.HabitatHome.Foundation.CoveoIndexing.Extractors
{
    public interface IExtractor
    {
        string OutputMetadataName { get; }

        object Extract(JToken p_CommerceEntity);
    }
}