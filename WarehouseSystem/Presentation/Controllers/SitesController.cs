using Application.Services.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[ApiController]
[Route("sites")]
public class SitesController(ISitesService sitesService) : Controller
{
    private readonly ISitesService _sitesService = sitesService;

    [HttpGet("{id}")]
    public IActionResult Get(long id)
    {
        var site = _sitesService.Get(id);
        return Ok(site);
    }

    [HttpGet()]
    public IActionResult List()
    {
        var sites = _sitesService.List();
        return Ok(sites);
    }

    [HttpPost()]
    public IActionResult Create(Site site)
    {
        var newSite = _sitesService.Create(site);
        return Ok(newSite);
    }
}
