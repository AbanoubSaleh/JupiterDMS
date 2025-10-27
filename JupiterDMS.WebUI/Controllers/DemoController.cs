using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JupiterDMS.WebUI.Controllers;

[Authorize]
public class DemoController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Admin()
    {
        ViewBag.Message = "This page is only accessible to Admins.";
        ViewBag.Role = "Admin";
        return View("RoleDemo");
    }

    [Authorize(Roles = "Admin,Editor")]
    public IActionResult Editor()
    {
        ViewBag.Message = "This page is accessible to Admins and Editors.";
        ViewBag.Role = "Editor";
        return View("RoleDemo");
    }

    public IActionResult Viewer()
    {
        ViewBag.Message = "This page is accessible to all authenticated users.";
        ViewBag.Role = "Viewer";
        return View("RoleDemo");
    }
}
