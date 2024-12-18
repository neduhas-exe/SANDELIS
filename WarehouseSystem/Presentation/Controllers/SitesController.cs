using Domain.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class SitesController : ControllerBase
{
    private readonly ISiteRepository _siteRepository;

    public SitesController(ISiteRepository siteRepository)
    {
        _siteRepository = siteRepository;
    }

    [HttpGet("{id}")]
    public IActionResult Get(long id)
    {
        var site = _siteRepository.Get(id);
        if (site == null)
            return NotFound();

        return Ok(site);
    }

    [HttpGet]
    public IActionResult List()
    {
        var sites = _siteRepository.List();
        return Ok(sites);
    }

    [HttpPost]
    public IActionResult Create(Site site)
    {
        var newSite = _siteRepository.Create(site);
        return CreatedAtAction(nameof(Get), new { id = newSite.Id }, newSite);
    }
}