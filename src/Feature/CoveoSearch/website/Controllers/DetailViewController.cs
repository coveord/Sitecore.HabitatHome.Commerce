using Microsoft.Extensions.DependencyInjection;
using Sitecore.Commerce.XA.Feature.Catalog.Controllers;
using Sitecore.Commerce.XA.Feature.Catalog.Models;
using Sitecore.Commerce.XA.Feature.Catalog.Repositories;
using Sitecore.Commerce.XA.Foundation.Common.Context;
using Sitecore.Commerce.XA.Foundation.Common.Models;
using Sitecore.Commerce.XA.Foundation.Connect;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.DependencyInjection;
using Sitecore.Globalization;
using Sitecore.HabitatHome.Feature.CoveoSearch.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Sitecore.HabitatHome.Feature.CoveoSearch.Controllers
{
    public class DetailViewController : CatalogController
    {
        public DetailViewController(IModelProvider modelProvider,
            IProductListHeaderRepository productListHeaderRepository,
            IProductListRepository productListRepository,
            IPromotedProductsRepository promotedProductsRepository,
            IProductInformationRepository productInformationRepository,
            IProductImagesRepository productImagesRepository,
            IProductInventoryRepository productInventoryRepository,
            IProductPriceRepository productPriceRepository,
            IProductVariantsRepository productVariantsRepository,
            IProductListPagerRepository productListPagerRepository,
            IProductFacetsRepository productFacetsRepository,
            IProductListSortingRepository productListSortingRepository,
            IProductListPageInfoRepository productListPageInfoRepository,
            IProductListItemsPerPageRepository productListItemsPerPageRepository,
            ICatalogItemContainerRepository catalogItemContainerRepository,
            IVisitedCategoryPageRepository visitedCategoryPageRepository,
            IVisitedProductDetailsPageRepository visitedProductDetailsPageRepository,
            ISearchInitiatedRepository searchInitiatedRepository,
            IStorefrontContext storefrontContext,
            ISiteContext siteContext,
            IContext context)
            : base(modelProvider, productListHeaderRepository, productListRepository, promotedProductsRepository, productInformationRepository, productImagesRepository, productInventoryRepository, productPriceRepository, productVariantsRepository, productListPagerRepository, productFacetsRepository, productListSortingRepository, productListPageInfoRepository, productListItemsPerPageRepository, catalogItemContainerRepository, visitedCategoryPageRepository, visitedProductDetailsPageRepository,  searchInitiatedRepository, storefrontContext, siteContext, context)
        {
        }

        public ActionResult DetailView()
        {
            CatalogItemRenderingModel catalogItemRenderingModel = ProductInformationRepository.GetProductInformationRenderingModel(ServiceLocator.ServiceProvider.GetService<IVisitorContext>());
            DetailViewModel model = new DetailViewModel(catalogItemRenderingModel);

            List<string> categoryNames = new List<string>();
            string[] categoryIds = model.CategoryIds;
            Database database = catalogItemRenderingModel.PageItem.Database;
            Language language = catalogItemRenderingModel.PageItem.Language;

            if (categoryIds.Any())
            {
                Item item = ItemUtilities.ResolveReferencedItem(categoryIds.Last(), database, language);
                if (item != null)
                {
                    model.ReportingCategory = item.DisplayName;
                }
            }

            return View("~/Views/Commerce/CoveoSearch/DetailView.cshtml", model);
        }
    }
}