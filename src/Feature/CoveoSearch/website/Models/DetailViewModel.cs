using Sitecore.Commerce;
using Sitecore.Commerce.XA.Feature.Catalog.Models;
using Sitecore.Commerce.XA.Foundation.Common.Models;
using System.Linq;
using System.Web.Helpers;

namespace Sitecore.HabitatHome.Feature.CoveoSearch.Models
{
    public class DetailViewModel : BaseCommerceRenderingModel
    {
        private CatalogItemRenderingModel p_CatalogItemRenderingModel;

        public string ProductId
        {
            get
            {
                return p_CatalogItemRenderingModel.ProductId;
            }
        }

        public string DisplayName
        {
            get
            {
                return p_CatalogItemRenderingModel.DisplayName;
            }
        }

        public decimal? ListPrice
        {
            get
            {
                return p_CatalogItemRenderingModel.ListPrice;
            }
        }

        public decimal? AdjustedPrice
        {
            get
            {
                return p_CatalogItemRenderingModel.AdjustedPrice;
            }
        }

        public string[] CategoryIds
        {
            get
            {
                return p_CatalogItemRenderingModel.CatalogItem.Fields["ParentCategoryList"].Value.Split('|');
            }
        }

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

        public string ReportingBrand
        {
            get
            {
                return p_CatalogItemRenderingModel.CatalogItem.Fields["Brand"].Value;
            }
        }

        public DetailViewModel(CatalogItemRenderingModel catalogItemRenderingModel)
        {
            Assert.ArgumentNotNull(catalogItemRenderingModel, "catalogItemRenderingModel");

            p_CatalogItemRenderingModel = catalogItemRenderingModel;
        }
    }
}