using Sitecore.Commerce.Pipelines;
using Sitecore.Diagnostics;
using Sitecore.HabitatHome.Feature.CoveoSearch.UsageAnalytics;
using System.Globalization;

namespace Sitecore.HabitatHome.Feature.CoveoSearch.Processors
{
    public class TriggerEventBase : PipelineProcessor<ServicePipelineArgs>
    {
        public ServicePipelineArgs ServicePipelineArgs { get; private set; }

        public CoveoUsageAnalyticsClient CoveoUsageAnalyticsClient { get; private set; }

        public override void Process(ServicePipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            Assert.ArgumentNotNull(args.Request, "args.Request");
            Assert.ArgumentNotNull(args.Result, "args.Result");

            ServicePipelineArgs = args;

            if (!IsCommerceOperationSuccessful())
            {
                LogDebugMessage("Commerce operation must be successful");
                return;
            }

            CoveoUsageAnalyticsClient = new CoveoUsageAnalyticsClient();
        }

        private bool IsCommerceOperationSuccessful()
        {
            return ServicePipelineArgs.Result.Success;
        }

        protected void LogMustNotBeNullDebugMessage(string p_Object)
        {
            Assert.ArgumentNotNullOrEmpty(p_Object, "p_Object");

            LogDebugMessage(string.Format(CultureInfo.InvariantCulture, "{0} must not be null", p_Object));
        }

        protected void LogDebugMessage(string p_Condition)
        {
            Assert.ArgumentNotNullOrEmpty(p_Condition, "p_Condition");

            Log.Debug(string.Format(CultureInfo.InvariantCulture, "{0} to track Sitecore Commerce addToCart event in Coveo Usage Analytics.", p_Condition), this);
        }
    }
}