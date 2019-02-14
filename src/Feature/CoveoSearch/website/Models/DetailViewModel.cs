using Sitecore.Commerce;
using Sitecore.Commerce.XA.Feature.Catalog.Models;
using Sitecore.Commerce.XA.Foundation.Common.Models;
using System.Linq;
using System.Web.Helpers;

namespace Sitecore.HabitatHome.Feature.CoveoSearch.Models
{
    public class DetailViewModel : BaseCommerceRenderingModel
    {
        private readonly CatalogItemRenderingModel m_CatalogItemRenderingModel;

        public string ProductId => m_CatalogItemRenderingModel.ProductId;

        public string DisplayName => m_CatalogItemRenderingModel.DisplayName;

        public decimal? ListPrice => m_CatalogItemRenderingModel.ListPrice;

        public decimal? AdjustedPrice => m_CatalogItemRenderingModel.AdjustedPrice;

        public string[] CategoryIds => m_CatalogItemRenderingModel.CatalogItem.Fields["ParentCategoryList"].Value.Split('|');

        public string Categories
        {
            get
            {
                string[] categoryIds = CategoryIds;

                if (categoryIds.Any())
                {
                    return Json.Encode(categoryIds);
                } else
                {
                    return string.Empty;
                }
            }
        }
        
        public string ReportingCategory { get; set; }

        public string Brands
        {
            get
            {
                string[] brands = new string[] { ReportingBrand };
                return Json.Encode(brands);
            }
        }

        public string ReportingBrand => m_CatalogItemRenderingModel.CatalogItem.Fields["Brand"].Value;

        public string Language => m_CatalogItemRenderingModel.CatalogItem.Language.Name;

        public DetailViewModel(CatalogItemRenderingModel catalogItemRenderingModel)
        {
            Assert.ArgumentNotNull(catalogItemRenderingModel, "catalogItemRenderingModel");

            m_CatalogItemRenderingModel = catalogItemRenderingModel;
        }
    }
}