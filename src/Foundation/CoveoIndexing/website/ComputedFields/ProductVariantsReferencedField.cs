using Coveo.Framework.CNL;
using Coveo.Framework.Databases;
using Coveo.Framework.Items;
using Coveo.Framework.Log;
using Coveo.SearchProvider.ComputedFields;
using Sitecore.ContentSearch;
using Sitecore.Data.Items;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;

namespace Sitecore.HabitatHome.Foundation.CoveoIndexing.ComputedFields
{
#pragma warning disable CS0618 // Type or member is obsolete
    // Usage: <field fieldName="variantscolor" variantFieldName="Color" type="Sitecore.HabitatHome.Foundation.CoveoIndexing.ComputedFields.ProductVariantsReferencedField, Sitecore.HabitatHome.Foundation.CoveoIndexing" />
    public class ProductVariantsReferencedField : ConfigurableComputedField
    {
        private static readonly ILogger s_Logger = CoveoLogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [ThreadStatic]
        private const string VARIANT_FIELD_NAME_ATTRIBUTE_NAME = "variantFieldName";
        private const string PRODUCT_TEMPLATE_ID = "225F8638-2611-4841-9B89-19A5440A1DA1";

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

        public ProductVariantsReferencedField(XmlNode p_Configuration) : base(p_Configuration)
        {
        }

        public ProductVariantsReferencedField(ISitecoreFactory p_SitecoreFactory) : base(p_SitecoreFactory)
        {
        }

        public override object ComputeFieldValue(IIndexable p_Indexable)
        {
            Precondition.NotNull(p_Indexable, () => () => p_Indexable);

            IItem item = new ItemWrapper(new IndexableWrapper(p_Indexable));

            object value = null;

            if (item.TemplateID.ToString().ToUpper() == PRODUCT_TEMPLATE_ID)
            {
                value = GetVariantItemsFieldValues(item);
            }
            return value;
        }

        private List<string> GetVariantItemsFieldValues(IItem p_Item)
        {
            List<string> variantFieldValues = new List<string>();
            Item[] variants = p_Item.SitecoreItem.Axes.GetDescendants();

            foreach (Item variant in variants) {
                s_Logger.Debug("Variant ID = " + variant.ID.ToString());
                string variantFieldValue = variant.Fields[GetAttributeValue(VARIANT_FIELD_NAME_ATTRIBUTE_NAME)].Value;
                s_Logger.Debug("Coveo computed field: variant field value = " + variantFieldValue);
                if (!variantFieldValues.Contains(variantFieldValue)) {
                    variantFieldValues.Add(variantFieldValue);
                }
            }

            return variantFieldValues;
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete
}