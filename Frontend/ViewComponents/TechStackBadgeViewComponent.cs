using Microsoft.AspNetCore.Mvc;

namespace Frontend.ViewComponents;

public sealed class TechStackBadgeViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        var badges = new[]
        {
            "MVC/Razor",
            "Tag Helpers",
            "AJAX",
            "API Gateway",
            "EF Core",
            "Azure Ready"
        };

        return View(badges);
    }
}
