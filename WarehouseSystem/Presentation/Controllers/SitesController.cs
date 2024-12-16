using Application.Services.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    /// <summary>
    /// Kontroleris skirtas objektų (Site) valdymui
    /// </summary>
    [ApiController]  // Žymi, kad tai yra API kontroleris
    [Route("sites")]  // API endpoint pradžia, pvz: /sites
    public class SitesController : Controller
    {
        private readonly ISitesService _sitesService;

        /// <summary>
        /// Konstruktorius su dependency injection
        /// </summary>
        /// <param name="sitesService">Objektų serviso implementacija</param>
        public SitesController(ISitesService sitesService)
        {
            _sitesService = sitesService;
        }

        /// <summary>
        /// Gauti konkretų objektą pagal ID
        /// </summary>
        /// <param name="id">Objekto ID</param>
        /// <returns>Objekto informacija</returns>
        [HttpGet("{id}")]  // GET /sites/{id}
        public IActionResult Get(long id)
        {
            var site = _sitesService.Get(id);
            return Ok(site);  // Grąžina 200 OK su objekto duomenimis
        }

        /// <summary>
        /// Gauti visų objektų sąrašą
        /// </summary>
        /// <returns>Objektų sąrašas</returns>
        [HttpGet]  // GET /sites
        public IActionResult List()
        {
            var sites = _sitesService.List();
            return Ok(sites);  // Grąžina 200 OK su objektų sąrašu
        }

        /// <summary>
        /// Sukurti naują objektą
        /// </summary>
        /// <param name="site">Naujo objekto duomenys</param>
        /// <returns>Sukurto objekto informacija</returns>
        [HttpPost]  // POST /sites
        public IActionResult Create(Site site)
        {
            var newSite = _sitesService.Create(sitew);
            return Ok(newSite);  // Grąžina 200 OK su sukurto objekto duomenimis
        }
    }
}
