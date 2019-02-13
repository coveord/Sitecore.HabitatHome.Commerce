namespace Sitecore.HabitatHome.Feature.CoveoSearch.UsageAnalytics
{
    public class AddToCartEvent : AbstractAddWithQuantityEvent
    {
        public AddToCartEvent(string productId,
                              string productName,
                              decimal quantity,
                              decimal price,
                              string cartId,
                              string language,
                              string referrerUrl)
            : base(productId, productName, quantity, price, cartId, language, referrerUrl)
        {
            EventType = "addToCart";
        }
    }
}