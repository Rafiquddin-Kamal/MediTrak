using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediTrack.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    // GET / — only reachable after login; otherwise the cookie handler redirects to /Account/Login
    public IActionResult Index() => View();

    [AllowAnonymous]
    public IActionResult Error() => View();
}
