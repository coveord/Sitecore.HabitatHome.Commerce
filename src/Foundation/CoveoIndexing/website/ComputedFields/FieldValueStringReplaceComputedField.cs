using Coveo.Framework.Databases;
using System.Xml;

namespace Sitecore.HabitatHome.Foundation.CoveoIndexing.ComputedFields
{
#pragma warning disable CS0618 // Type or member is obsolete
    // Usage: <field fieldName="computedmetakeywords" sourceFieldName="MetaKeywords" replace="," replaceBy=";" type="Sitecore.HabitatHome.Foundation.CoveoIndexing.ComputedFields.FieldValueStringReplaceComputedField, Sitecore.HabitatHome.Foundation.CoveoIndexing" />
    public class FieldValueStringReplaceComputedField : FieldValueComputedField
    {
        private const string REPLACE_TEXT_ATTRIBUTE_NAME = "replace";
        private const string REPLACE_TEXT_BY_ATTRIBUTE_NAME = "replaceBy";

        public FieldValueStringReplaceComputedField(XmlNode p_Configuration) : base(p_Configuration)
        {
        }

        public FieldValueStringReplaceComputedField(ISitecoreFactory p_SitecoreFactory) : base(p_SitecoreFactory)
        {
        }

        protected override string TransformValue(string p_Value)
        {
            return p_Value.Replace(GetAttributeValue(REPLACE_TEXT_ATTRIBUTE_NAME), GetAttributeValue(REPLACE_TEXT_BY_ATTRIBUTE_NAME));
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete
}