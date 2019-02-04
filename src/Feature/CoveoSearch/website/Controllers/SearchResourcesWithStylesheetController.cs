using Coveo.UI.Components.Sxa.Areas.CoveoHiveSxa.Controllers.Resources;
using Coveo.UI.Components.Sxa.Areas.CoveoHiveSxa.Repositories.Resources;

namespace Sitecore.HabitatHome.Feature.CoveoSearch.Controllers
{
    public class SearchResourcesWithStylesheetController : SearchResourcesController
    {
        public SearchResourcesWithStylesheetController(ISearchResourcesRepository p_Repository)
            : base(p_Repository)
        {
        }

        protected override string GetIndexViewName()
        {
            return "~/Views/Commerce/CoveoSearch/Coveo Search Resources With Stylesheet.cshtml";
        }
    }
}