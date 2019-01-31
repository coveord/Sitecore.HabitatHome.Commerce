using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Sitecore.HabitatHome.Foundation.CoveoIndexing.ComputedFields
{
#pragma warning disable CS0618 // Type or member is obsolete
    // Usage: <field fieldName="computedcategories" sourceField="ParentCategoryList" referencedFieldName="DisplayName" remove="DefaultRecommendation;Featured Product;NextCube Performance Gaming Accessories;NextCube_InGame;Fitness Products;Kitchen Appliances" type="Sitecore.HabitatHome.Foundation.CoveoIndexing.ComputedFields.RemoveValuesReferencedFieldComputedField, Sitecore.HabitatHome.Foundation.CoveoIndexing" />
    public class RemoveValuesReferencedFieldComputedField : TransformableReferencedFieldComputedField
    {
        private const string VALUES_TO_REMOVE_ATTRIBUTE_NAME = "remove";

        public RemoveValuesReferencedFieldComputedField(XmlNode p_Configuration) : base(p_Configuration)
        {
        }

        public override List<string> TransformValue(List<string> value)
        {
            List<string> baseValue = base.TransformValue(value);
            List<string> valuesToRemove = new List<string>(GetAttributeValue(VALUES_TO_REMOVE_ATTRIBUTE_NAME).Split(';'));
            return RemoveValues(baseValue, valuesToRemove);
        }

        public virtual List<string> RemoveValues(List<string> baseValue, List<string> valuesToRemove)
        {
            List<string> transformedValue = new List<string>();

            baseValue.ForEach((string value) =>
            {
                if (!valuesToRemove.Any((string valueToRemove) => valueToRemove == value))
                {
                    transformedValue.Add(value);
                }
            });

            return transformedValue;
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete
}