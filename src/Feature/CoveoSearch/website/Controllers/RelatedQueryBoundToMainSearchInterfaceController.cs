using Coveo.UI.Components.Sxa.Areas.CoveoHiveSxa.Controllers.SearchInterfaces;
using Coveo.UI.Components.Sxa.Areas.CoveoHiveSxa.Repositories.SearchInterfaces;

namespace Sitecore.HabitatHome.Feature.CoveoSearch.Controllers
{
    public class RelatedQueryBoundToMainSearchInterfaceController : RelatedQueryController
    {
        public RelatedQueryBoundToMainSearchInterfaceController(IRelatedQueryRepository p_Repository)
            : base(p_Repository)
        {
        }

        protected override string GetIndexViewName()
        {
            return "~/Views/Commerce/CoveoSearch/Coveo Related Query Bound to Main Search Interface.cshtml";
        }
    }
}