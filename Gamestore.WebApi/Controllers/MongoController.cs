using Microsoft.AspNetCore.Mvc;

namespace Gamestore.WebApi.Controllers;
public class MongoController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
