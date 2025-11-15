// --- This is the correct code for your .NET 8 HomeController.cs ---

// 1. We are now using 'Microsoft.AspNetCore.Mvc', NOT 'System.Web.Mvc'
using Microsoft.AspNetCore.Mvc;

// 2. Use your .NET 8 project's namespace
namespace BasicBlog.Yarp.Controllers
{
    public class HomeController : Controller
    {
        // 3. The return type is 'IActionResult'
        public IActionResult Index()
        {
            // This redirect to the proxied blog controller is correct
            return Redirect("/Blog");
        }

        public IActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public IActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }
    }
}