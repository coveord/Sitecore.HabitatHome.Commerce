using Coveo.Framework.CNL;
using Coveo.Framework.Databases;
using Coveo.Framework.Items;
using Coveo.Framework.Log;
using Coveo.SearchProvider.ComputedFields;
using Sitecore.ContentSearch;
using System;
using System.Reflection;
using System.Xml;

namespace Sitecore.HabitatHome.Foundation.CoveoIndexing.ComputedFields
{
#pragma warning disable CS0618 // Type or member is obsolete
    // Usage: <field fieldName="computedDescription" sourceFieldName="Description" type="Sitecore.HabitatHome.Foundation.CoveoIndexing.ComputedFields.FieldValueComputedField, Sitecore.HabitatHome.Foundation.CoveoIndexing" />
    public class FieldValueComputedField : ConfigurableComputedField
    {
        private static readonly ILogger s_Logger = CoveoLogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [ThreadStatic]
        private const string SOURCE_FIELD_NAME_ATTRIBUTE_NAME = "sourceFieldName";

        public new string ReturnType
        {
            get
            {
                return "string";
            }
            set
            {
            }
        }

        public FieldValueComputedField(XmlNode p_Configuration) : base(p_Configuration)
        {
        }

        public FieldValueComputedField(ISitecoreFactory p_SitecoreFactory) : base(p_SitecoreFactory)
        {
        }

        public override object ComputeFieldValue(IIndexable p_Indexable)
        {
            Precondition.NotNull(p_Indexable, () => () => p_Indexable);

            IItem item = new ItemWrapper(new IndexableWrapper(p_Indexable));

            string value = GetFieldValue(item);

            if (!string.IsNullOrEmpty(value)) {
                value = TransformValue(value);
            }

            return value;
        }

        private string GetFieldValue(IItem p_Item)
        {
            return p_Item.GetFieldValue(GetAttributeValue(SOURCE_FIELD_NAME_ATTRIBUTE_NAME));
        }

        protected virtual string TransformValue(string p_Value)
        {
            return p_Value;
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete
}