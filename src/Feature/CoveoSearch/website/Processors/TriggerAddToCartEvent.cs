using Sitecore.Commerce.Engine.Connect.Entities;
using Sitecore.Commerce.Entities.Carts;
using Sitecore.Commerce.Pipelines;
using Sitecore.Commerce.Services.Carts;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.HabitatHome.Feature.CoveoSearch.UsageAnalytics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.HabitatHome.Feature.CoveoSearch.Processors
{
    public class TriggerAddToCartEvent : TriggerEventBase
    {
        private Cart _Cart;
        private IEnumerable<CartLine> _AddedCartLines;

        public override void Process(ServicePipelineArgs args)
        {
            base.Process(args);
            Assert.IsTrue(args.Request is CartLinesRequest, "args.Request must be of type CartLinesRequest");
            Assert.IsTrue(args.Result is CartResult, "args.Result must be of type CartResult");

            _Cart = GetCartFromArgs();
            if (_Cart == null)
            {
                LogMustNotBeNullDebugMessage("_Cart");
                return;
            }

            CartLinesRequest cartLinesRequest = GetRequestFromArgs();
            if (cartLinesRequest == null)
            {
                LogMustNotBeNullDebugMessage("CartLinesRequest");
                return;
            }
            _AddedCartLines = cartLinesRequest.Lines;
            if (_AddedCartLines == null)
            {
                LogMustNotBeNullDebugMessage("_AddedCartLines");
                return;
            }

            ProcessCartLines();
        }

        private Cart GetCartFromArgs()
        {
            return ((CartResult) ServicePipelineArgs.Result).Cart;
        }

        private CartLinesRequest GetRequestFromArgs()
        {
            return (CartLinesRequest) ServicePipelineArgs.Request;
        }

        private void ProcessCartLines()
        {
            foreach (CartLine addedCartLine in _AddedCartLines)
            {
                if (addedCartLine == null)
                {
                    LogMustNotBeNullDebugMessage("addedCartLine");
                    break;
                }
                if (addedCartLine.Product == null)
                {
                    LogMustNotBeNullDebugMessage("addedCartLine.Product");
                    break;
                }

                CartLine cartLineFromCart = GetAddedCartLineFromCart(addedCartLine.Product);
                if (cartLineFromCart == null)
                {
                    LogMustNotBeNullDebugMessage("cartLineFromCart");
                    break;
                }

                Guid productVariantItemId = cartLineFromCart.Product.SitecoreProductItemId;
                if (productVariantItemId == null)
                {
                    LogMustNotBeNullDebugMessage("productVariantItemId");
                    break;
                }

                Database database = Context.Database;
                if (database == null)
                {
                    LogMustNotBeNullDebugMessage("database");
                    break;
                }

                Language language = Context.Language;
                if (language == null)
                {
                    LogMustNotBeNullDebugMessage("language");
                    break;
                }

                Item productVariantSellableItem = ItemUtilities.ResolveReferencedItem(productVariantItemId, database, language);
                if (productVariantSellableItem == null)
                {
                    LogMustNotBeNullDebugMessage("productVariantSellableItem");
                    break;
                }

                Item productSellableItem = ItemUtilities.GetProductSellableItem(productVariantSellableItem);
                if (productSellableItem == null)
                {
                    LogMustNotBeNullDebugMessage("productSellableItem");
                    break;
                }

                AddToCartEvent analyticsEvent = new AddToCartEvent(
                    addedCartLine.Product.ProductId,
                    productSellableItem.DisplayName,
                    addedCartLine.Quantity,
                    cartLineFromCart.Product.Price.Amount,
                    _Cart.ExternalId,
                    ServicePipelineArgs.Request.SelectedUICulture,
                    HttpContext.Current.Request.UrlReferrer.ToString()
                );

                IEnumerable<string> categoryIds = ItemUtilities.GetSellableItemCategoryIds(productSellableItem);
                if (categoryIds.Any())
                {
                    analyticsEvent.Categories = categoryIds;
                    string reportingCategory = ItemUtilities.GetCategoryDisplayName(categoryIds.Last(), database, language);
                    if (!string.IsNullOrEmpty(reportingCategory))
                    {
                        analyticsEvent.ReportingCategory = reportingCategory;
                    }
                }

                string brand = ItemUtilities.GetSellableItemBrand(productSellableItem);
                if (!string.IsNullOrEmpty(brand))
                {
                    analyticsEvent.Brands = new string[] { brand };
                    analyticsEvent.ReportingBrand = brand;
                }

                // TODO: Discounted Price

                CoveoUsageAnalyticsClient.Send(analyticsEvent);
            }
        }

        private CartLine GetAddedCartLineFromCart(CartProduct p_AddedCartProduct)
        {
            Assert.ArgumentNotNull(p_AddedCartProduct, "p_AddedCartProduct");

            return _Cart.Lines.Where(cartLine =>
            {
                CartProduct cartLineProduct = cartLine.Product;
                if (cartLineProduct != null)
                {
                    bool isSameProductId = cartLineProduct.ProductId.Equals(p_AddedCartProduct.ProductId, StringComparison.OrdinalIgnoreCase);
                    if (cartLineProduct is CommerceCartProduct && p_AddedCartProduct is CommerceCartProduct)
                    {
                        return isSameProductId && ((CommerceCartProduct) cartLineProduct).ProductVariantId.Equals(((CommerceCartProduct) p_AddedCartProduct).ProductVariantId, StringComparison.OrdinalIgnoreCase);
                    }
                    else
                    {
                        return isSameProductId;
                    }
                }
                return false;
            }).FirstOrDefault();
        }
    }
}