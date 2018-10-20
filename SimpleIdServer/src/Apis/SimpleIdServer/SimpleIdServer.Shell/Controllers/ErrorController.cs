using Microsoft.AspNetCore.Mvc;
using SimpleIdServer.Shell.ViewModels;

namespace SimpleIdServer.Shell.Controllers
{
    [Area("Shell")]
    public class ErrorController : Controller
    {
        public ActionResult Index(string code, string message)
        {
            var viewModel = new ErrorViewModel
            {
                Code = code,
                Message = message
            };
            return View(viewModel);
        }

        public ActionResult Get401()
        {
            return View();
        }
        
        public ActionResult Get404() 
        {
            return View();    
        }

        public ActionResult Get500()
        {
            return View();
        }
    }
}