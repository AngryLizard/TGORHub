using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Frontend.Models;

namespace Frontend.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new HomeViewModel());
        }

        [HttpGet]
        public IActionResult Project()
        {
            return View(new ProjectViewModel());
        }

        [HttpGet]
        public IActionResult Create(long categoryId)
        {
            return View(new CreateViewModel() { CategoryId = categoryId });
        }

        [HttpGet]
        public IActionResult Edit(long projectId, IEnumerable<int> target)
        {
            return View(new EditViewModel() { ProjectId = projectId, Target = target });
        }
    }
}