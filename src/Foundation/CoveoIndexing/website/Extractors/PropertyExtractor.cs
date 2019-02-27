using Coveo.Framework.CNL;
using Newtonsoft.Json.Linq;

namespace Sitecore.HabitatHome.Foundation.CoveoIndexing.Extractors
{
    public class PropertyExtractor : IExtractor
    {
        private readonly string m_InputPropertyName;

        public string OutputMetadataName { get; }

        public PropertyExtractor(string p_InputPropertyName,
                                 string p_OutputMetadataName)
        {
            Precondition.NotEmpty(p_InputPropertyName, () => () => p_InputPropertyName);
            Precondition.NotEmpty(p_OutputMetadataName, () => () => p_OutputMetadataName);

            m_InputPropertyName = p_InputPropertyName;
            OutputMetadataName = p_OutputMetadataName;
        }

        public object Extract(JToken p_CommerceEntity)
        {
            Precondition.NotNull(p_CommerceEntity, () => () => p_CommerceEntity);

            return p_CommerceEntity[m_InputPropertyName].Value<string>();
        }
    }
}