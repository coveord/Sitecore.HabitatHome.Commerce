using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Coveo.Framework.CNL;
using Coveo.Framework.Databases;
using Coveo.Framework.Items;
using Coveo.Framework.Log;
using Coveo.Framework.Utils;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.ComputedFields;
using Sitecore.Globalization;

namespace Sitecore.HabitatHome.Foundation.CoveoIndexing.ComputedFields
{
    // Usage: <field fieldName="productImages" type="Sitecore.HabitatHome.Foundation.CoveoIndexing.ComputedFields.ProductImagesUrlComputedField, Sitecore.HabitatHome.Foundation.CoveoIndexing" />
    public class ProductImagesUrlComputedField : IComputedIndexField
    {
        private static readonly ILogger s_Logger = CoveoLogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly string[] s_ImagesSeperator = { IMAGES_FIELD_VALUE_SEPARATOR };
        private readonly IUrlUtilities m_UrlUtilities = UrlUtilitiesProvider.GetInstance();
        [ThreadStatic]
        private static IDatabaseWrapper s_Database;
        private const string IMAGES_FIELD_NAME = "Images";
        private const string IMAGES_FIELD_VALUE_SEPARATOR = "|";
        public string FieldName { get; set; }
        public string ReturnType
        {
            get
            {
                return "string";
            }
            set
            {
            }
        }
        public object ComputeFieldValue(IIndexable p_Indexable)
        {
            s_Logger.Trace("Coveo computed field: Resolving images URL");
            Precondition.NotNull(p_Indexable, () => () => p_Indexable);
            IItem item = new ItemWrapper(new IndexableWrapper(p_Indexable));
            object value = GetItemURL(item);

            return value;
        }
        private object GetItemURL(IItem p_Item)
        {
            Precondition.NotNull(p_Item, () => () => p_Item);
            s_Database = p_Item.Database;
            object itemURL = null;
            IEnumerable<string> imagesItemsIds = GetImagesItemsIds(p_Item);
            if (imagesItemsIds.Any())
            {
                itemURL = GetImagesNames(imagesItemsIds, p_Item.Language);
            }
            return itemURL;
        }
        private IEnumerable<string> GetImagesItemsIds(IItem p_Item)
        {
            IEnumerable<string> ImagesItemsIds = new List<string>();
            string imagesFieldValue = p_Item.GetFieldValue(IMAGES_FIELD_NAME);
            if (!String.IsNullOrEmpty(imagesFieldValue))
            {
                ImagesItemsIds = imagesFieldValue.Split(s_ImagesSeperator, StringSplitOptions.RemoveEmptyEntries).Distinct();
            }
            return ImagesItemsIds;
        }
        private List<string> GetImagesNames(IEnumerable<string> p_ImagesItemsIds, Language p_SourceItemLanguage)
        {
            List<string> ImagesURL = new List<string>();
            foreach (string imageItemId in p_ImagesItemsIds)
            {
                string itemName = GetItemName(imageItemId, p_SourceItemLanguage);
                if (!String.IsNullOrEmpty(itemName))
                {
                    ImagesURL.Add(itemName);
                }
            }
            s_Logger.Trace(ImagesURL.ToString());
            return ImagesURL.Any() ? ImagesURL : null;
        }
        private string GetItemName(string p_Id, Language p_SourceItemLanguage)
        {
            string imageUrl = null;
            IItem item = ResolveReferencedItem(p_Id, p_SourceItemLanguage);
            if (item != null)
            {
                imageUrl = m_UrlUtilities.GetMediaUrl(item);
                imageUrl = imageUrl.Replace("/sitecore/shell", "");
            }
            return imageUrl;
        }
        private IItem ResolveReferencedItem(string p_Id, Language p_SourceItemLanguage)
        {
            IItem item = s_Database.GetItem(p_Id, p_SourceItemLanguage);
            if (item == null || !item.Versions.HasVersion())
            {
                item = s_Database.GetItem(p_Id);
            }
            return item;
        }
    }
}
