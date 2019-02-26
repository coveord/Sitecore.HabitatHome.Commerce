using Newtonsoft.Json.Linq;

namespace Sitecore.HabitatHome.Foundation.CoveoIndexing.Extractors
{
    public class PropertyExtractor : IExtractor
    {
        private readonly string m_InputPropertyName;

        public string OutputMetadataName { get; }

        public PropertyExtractor(string p_OutputMetadataName, string p_InputPropertyName)
        {
            OutputMetadataName = p_OutputMetadataName;
            m_InputPropertyName = p_InputPropertyName;
        }

        public object Extract(JToken p_CommerceEntity)
        {
            return p_CommerceEntity[m_InputPropertyName].Value<string>();
        }
    }
}