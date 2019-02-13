using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using System;
using System.Collections.Generic;

namespace Sitecore.HabitatHome.Feature.CoveoSearch
{
    public static class ItemUtilities
    {
        public static Item ResolveReferencedItem(Guid p_Id,
                                                 Database p_Database,
                                                 Language p_SourceItemLanguage)
        {
            Assert.ArgumentNotNull(p_Id, "p_Id");
            Assert.ArgumentNotNull(p_Database, "p_Database");
            Assert.ArgumentNotNull(p_SourceItemLanguage, "p_SourceItemLanguage");

            return ResolveReferencedItem(new ID(p_Id), p_Database, p_SourceItemLanguage);
        }

        public static Item ResolveReferencedItem(string p_Id,
                                                 Database p_Database,
                                                 Language p_SourceItemLanguage)
        {
            Assert.ArgumentNotNullOrEmpty(p_Id, "p_Id");
            Assert.ArgumentNotNull(p_Database, "p_Database");
            Assert.ArgumentNotNull(p_SourceItemLanguage, "p_SourceItemLanguage");

            return ResolveReferencedItem(new ID(p_Id), p_Database, p_SourceItemLanguage);
        }

        public static Item ResolveReferencedItem(ID p_Id,
                                                 Database p_Database,
                                                 Language p_SourceItemLanguage)
        {
            Assert.ArgumentNotNull(p_Id, "p_Id");
            Assert.ArgumentNotNull(p_Database, "p_Database");
            Assert.ArgumentNotNull(p_SourceItemLanguage, "p_SourceItemLanguage");

            Item item = p_Database.GetItem(p_Id, p_SourceItemLanguage);

            // When an item does exist but not in the requested language, Sitecore returns an
            // incomplete item. We must then verify if the item
            // has a version to know whether it is a real item or not.
            if (item == null || item.Versions.Count == 0)
            {
                item = p_Database.GetItem(p_Id);
            }

            return item;
        }

        public static IEnumerable<string> GetSellableItemCategoryIds(Item p_SellableItem)
        {
            Assert.ArgumentNotNull(p_SellableItem, "p_SellableItem");

            Field categoriesField = p_SellableItem.Fields[Constants.SELLABLE_ITEM_CATEGORIES_FIELD_NAME];
            if (categoriesField != null && categoriesField.HasValue)
            {
                return categoriesField.Value.Split(Constants.SELLABLE_ITEM_CATEGORIES_FIELD_SEPARATOR);
            }
            return new string[] { };
        }

        public static string GetSellableItemBrand(Item p_SellableItem)
        {
            Assert.ArgumentNotNull(p_SellableItem, "p_SellableItem");

            Field brandField = p_SellableItem.Fields[Constants.SELLABLE_ITEM_BRAND_FIELD_NAME];
            if (brandField != null && brandField.HasValue)
            {
                return brandField.Value;
            }
            return null;
        }

        public static Item GetProductSellableItem(Item p_PossibleVariantSellableItem)
        {
            Assert.ArgumentNotNull(p_PossibleVariantSellableItem, "p_PossibleVariantSellableItem");

            if (p_PossibleVariantSellableItem.TemplateID.Equals(new ID(Constants.PRODUCT_VARIANT_TEMPLATE_ID)))
            {
                return p_PossibleVariantSellableItem.Parent;
            }
            return p_PossibleVariantSellableItem;
        }

        public static string GetCategoryDisplayName(string p_CategoryId, Database p_Database, Language p_ItemLanguage)
        {
            Assert.ArgumentNotNullOrEmpty(p_CategoryId, "p_CategoryId");
            Assert.ArgumentNotNull(p_Database, "p_Database");
            Assert.ArgumentNotNull(p_ItemLanguage, "p_ItemLanguage");

            Item item = ResolveReferencedItem(p_CategoryId, p_Database, p_ItemLanguage);
            if (item != null)
            {
                return item.DisplayName;
            }
            return null;
        }
    }
}