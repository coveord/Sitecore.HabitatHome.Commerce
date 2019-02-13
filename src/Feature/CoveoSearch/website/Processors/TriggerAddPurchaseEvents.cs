using Sitecore.Commerce.Entities.Carts;
using Sitecore.Commerce.Entities.Orders;
using Sitecore.Commerce.Pipelines;
using Sitecore.Commerce.Services.Orders;
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
    public class TriggerAddPurchaseEvents : TriggerEventBase
    {
        private Order _Order;
        
        public override void Process(ServicePipelineArgs args)
        {
            base.Process(args);
            Assert.IsTrue(args.Request is SubmitVisitorOrderRequest, "args.Request must be of type SubmitVisitorOrderRequest");
            Assert.IsTrue(args.Result is SubmitVisitorOrderResult, "args.Result must be of type SubmitVisitorOrderResult");

            _Order = GetOrderFromArgs();
            if (_Order == null)
            {
                LogMustNotBeNullDebugMessage("_Order");
                return;
            }
            if (_Order.Lines == null)
            {
                LogMustNotBeNullDebugMessage("_Order.Lines");
                return;
            }

            ProcessOrderLines();
        }

        private Order GetOrderFromArgs()
        {
            return ((SubmitVisitorOrderResult) ServicePipelineArgs.Result).Order;
        }

        private void ProcessOrderLines()
        {
            foreach (CartLine orderLine in _Order.Lines)
            {
                if (orderLine == null)
                {
                    LogMustNotBeNullDebugMessage("orderLine");
                    break;
                }
                if (orderLine.Product == null)
                {
                    LogMustNotBeNullDebugMessage("orderLine.Product");
                    break;
                }

                Guid productVariantItemId = orderLine.Product.SitecoreProductItemId;
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
                
                AddPurchaseEvent analyticsEvent = new AddPurchaseEvent(
                    orderLine.Product.ProductId,
                    productSellableItem.DisplayName,
                    orderLine.Quantity,
                    orderLine.Product.Price.Amount,
                    _Order.OrderID,
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
    }
}