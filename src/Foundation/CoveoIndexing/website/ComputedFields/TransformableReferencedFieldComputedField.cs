using Coveo.SearchProvider.ComputedFields;
using Sitecore.ContentSearch;
using System.Collections.Generic;
using System.Xml;

namespace Sitecore.HabitatHome.Foundation.CoveoIndexing.ComputedFields
{
#pragma warning disable CS0618 // Type or member is obsolete
    public class TransformableReferencedFieldComputedField : ReferencedFieldComputedField
    {
        public TransformableReferencedFieldComputedField(XmlNode p_Configuration) : base(p_Configuration)
        {
        }

        public override object ComputeFieldValue(IIndexable p_Indexable)
        {
            List<string> baseValue = base.ComputeFieldValue(p_Indexable) as List<string>;
            if (baseValue == null)
            {
                return null;
            }

            return TransformValue(baseValue);
        }

        public virtual List<string> TransformValue(List<string> value)
        {
            return value;
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete
}