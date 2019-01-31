using Coveo.Framework.CNL;
using Coveo.Framework.Items;
using Coveo.Framework.Log;
using Coveo.SearchProvider.InboundFilters;
using Coveo.SearchProvider.Pipelines;
using System.Reflection;

namespace Sitecore.HabitatHome.Foundation.CoveoIndexing.Processors
{
    public class ExcludeDuplicateProductsCoveoInboundFilter : AbstractCoveoInboundFilterProcessor
    {
        private static readonly ILogger s_Logger = CoveoLogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const string UPPERCASE_PRODUCT_TEMPLATE_ID = "225F8638-2611-4841-9B89-19A5440A1DA1";
        private const string LOWERCASE_ITEM_PATH_TO_INCLUDE = "/sitecore/content/habitat sites/habitat home/home/catalogs/habitat_master/habitat_master-departments/";
        private const int NUMBER_OF_SLASHES_IN_FULL_PATH_OF_ITEMS_TO_INCLUDE = 10;
            
        public override void Process(CoveoInboundFilterPipelineArgs p_Args)
        {
            s_Logger.TraceEntering("Process(CoveoInboundFilterPipelineArgs)");
            Precondition.NotNull(p_Args, () => () => p_Args);

            if (p_Args.IndexableToIndex != null &&
                    p_Args.IndexableToIndex.Item != null &&
                    ShouldExecute(p_Args) &&
                    ShouldExcludeItem(p_Args.IndexableToIndex.Item)) {
                s_Logger.Debug("The item \"{0}\" will not be indexed because it is explicitely excluded.", p_Args.IndexableToIndex.Item.ID.ToString());
                p_Args.IsExcluded = true;
            }

            s_Logger.TraceExiting("Process(CoveoInboundFilterPipelineArgs)");
        }

        private bool ShouldExcludeItem(IItem p_Item)
        {
            bool isCommerceProduct = IsCommerceProduct(p_Item);

            if (isCommerceProduct)
            {
                bool isUnderDepartmentItem = IsUnderDepartmentsItem(p_Item);
                int numberOfSlashesInFullPath = GetNumberOfSlashesInFullPath(p_Item);

                return !isUnderDepartmentItem || numberOfSlashesInFullPath > NUMBER_OF_SLASHES_IN_FULL_PATH_OF_ITEMS_TO_INCLUDE;
            }

            return false;
        }

        private bool IsCommerceProduct(IItem p_Item)
        {
            return p_Item.TemplateID.ToUpper() == UPPERCASE_PRODUCT_TEMPLATE_ID;
        }

        private bool IsUnderDepartmentsItem(IItem p_Item)
        {
            return p_Item.Paths.FullPath.ToLower().StartsWith(LOWERCASE_ITEM_PATH_TO_INCLUDE);
        }

        private int GetNumberOfSlashesInFullPath(IItem p_Item)
        {
            return p_Item.Paths.FullPath.Split('/').Length - 1;
        }
    }
}
