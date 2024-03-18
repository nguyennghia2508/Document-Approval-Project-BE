using System.Web;
using System.Web.Mvc;

namespace Document_Approval_Project_BE
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
