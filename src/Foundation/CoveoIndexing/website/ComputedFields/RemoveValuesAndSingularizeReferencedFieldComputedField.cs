using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;

namespace Sitecore.HabitatHome.Foundation.CoveoIndexing.ComputedFields
{
#pragma warning disable CS0618 // Type or member is obsolete
    // Usage: <field fieldName="computedsingularcategories" sourceField="ParentCategoryList" referencedFieldName="DisplayName" remove="DefaultRecommendation;Featured Product;NextCube Performance Gaming Accessories;NextCube_InGame;Fitness Products;Kitchen Appliances" type="Sitecore.HabitatHome.Foundation.CoveoIndexing.ComputedFields.RemoveValuesAndSingularizeReferencedFieldComputedField, Sitecore.HabitatHome.Foundation.CoveoIndexing" />
    public class RemoveValuesAndSingularizeReferencedFieldComputedField : RemoveValuesReferencedFieldComputedField
    {
        private static Regex IES_REGEX = new Regex("ies$");
        private static Regex S_REGEX = new Regex("s$");

        public RemoveValuesAndSingularizeReferencedFieldComputedField(XmlNode p_Configuration) : base(p_Configuration)
        {
        }

        public override List<string> RemoveValues(List<string> baseValue, List<string> valuesToRemove)
        {
            List<string> value = base.RemoveValues(baseValue, valuesToRemove);
            return SingularizeBaseValue(value);
        }

        public List<string> SingularizeBaseValue(List<string> baseValue)
        {
            List<string> transformedValue = new List<string>();

            baseValue.ForEach((string value) =>
            {
                transformedValue.Add(SingularizeValue(value));
            });

            return transformedValue;
        }

        public string SingularizeValue(string value)
        {
            List<string> words = new List<string>(value.Split(' '));
            List<string> singularWords = new List<string>();

            words.ForEach((string word) =>
            {
                singularWords.Add(SingularizeWord(word));
            });

            return string.Join(" ", singularWords.ToArray());
        }

        public string SingularizeWord(string word)
        {
            string singularWord = IES_REGEX.Replace(word, "y");
            return S_REGEX.Replace(singularWord, "");
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete
}