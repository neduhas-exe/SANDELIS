using Domain.Models;
using Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class SitesController : ControllerBase
{
    private readonly ISiteRepository _siteRepository;
    private const string ADMIN_USERNAME = "admin";

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

    [HttpGet("customer/{customerId}")]
    public IActionResult GetCustomerSites(long customerId)
    {
        var sites = _siteRepository.GetByCustomerId(customerId);
        return Ok(sites);
    }

    [HttpPost]
    public IActionResult Create(SiteCreateRequest request)
    {
        var site = new Site
        {
            CustomerId = request.CustomerId,
            Name = request.Name,
            Address = request.Address,
            ContactPerson = request.ContactPerson,
            ContactPhone = request.ContactPhone,
            Comments = string.Empty,
            IsActive = true,
            CreatedBy = request.UserName,
            CreatedDate = DateTime.UtcNow
        };

        var siteHistory = new SiteHistory
        {
            SiteId = site.Id,
            UserName = request.UserName,
            ChangeDate = DateTime.UtcNow,
            ChangeType = "Created",
            ChangeSummary = $"Site '{site.Name}' was created",
            IsAdminAction = request.UserName == ADMIN_USERNAME
        };

        site.History.Add(siteHistory);
        var newSite = _siteRepository.Create(site);
        return CreatedAtAction(nameof(Get), new { id = newSite.Id }, newSite);
    }

    [HttpPut("{id}")]
    public IActionResult Update(long id, [FromBody] SiteUpdateRequest request)
    {
        var existingSite = _siteRepository.Get(id);
        if (existingSite == null)
            return NotFound($"Site with ID {id} not found");

        if (request.UserName != ADMIN_USERNAME && request.IsActiveChanged)
            return Forbid("Only administrators can change site status");

        var changes = new List<string>();
        if (existingSite.Name != request.Name)
            changes.Add($"Name changed from '{existingSite.Name}' to '{request.Name}'");
        if (existingSite.Address != request.Address)
            changes.Add($"Address changed from '{existingSite.Address}' to '{request.Address}'");
        if (existingSite.ContactPerson != request.ContactPerson)
            changes.Add($"Contact person changed from '{existingSite.ContactPerson}' to '{request.ContactPerson}'");
        if (existingSite.ContactPhone != request.ContactPhone)
            changes.Add($"Contact phone changed from '{existingSite.ContactPhone}' to '{request.ContactPhone}'");
        if (existingSite.IsActive != request.IsActive)
            changes.Add($"Active status changed from '{existingSite.IsActive}' to '{request.IsActive}'");

        existingSite.Name = request.Name;
        existingSite.Address = request.Address;
        existingSite.ContactPerson = request.ContactPerson;
        existingSite.ContactPhone = request.ContactPhone;
        existingSite.IsActive = request.IsActive;
        existingSite.ModifiedBy = request.UserName;
        existingSite.ModifiedDate = DateTime.UtcNow;

        var siteHistory = new SiteHistory
        {
            SiteId = id,
            UserName = request.UserName,
            ChangeDate = DateTime.UtcNow,
            ChangeType = "Updated",
            ChangeSummary = string.Join("\n", changes),
            IsAdminAction = request.UserName == ADMIN_USERNAME
        };

        existingSite.History.Add(siteHistory);
        _siteRepository.Update(existingSite);

        return NoContent();
    }

    [HttpPost("{id}/comment")]
    public IActionResult AddComment(long id, [FromBody] CommentRequest request)
    {
        var site = _siteRepository.Get(id);
        if (site == null)
            return NotFound();

        var previousComments = site.Comments;
        site.Comments += $"\n[{DateTime.UtcNow}] {request.UserName}: {request.Comment}";
        site.LastCommentDate = DateTime.UtcNow;
        site.ModifiedBy = request.UserName;
        site.ModifiedDate = DateTime.UtcNow;

        var siteHistory = new SiteHistory
        {
            SiteId = id,
            UserName = request.UserName,
            ChangeDate = DateTime.UtcNow,
            ChangeType = "Comment Added",
            ChangeSummary = "New comment was added",
            PreviousValues = previousComments,
            NewValues = site.Comments,
            IsAdminAction = request.UserName == ADMIN_USERNAME
        };

        site.History.Add(siteHistory);
        var updatedSite = _siteRepository.Update(site);
        return Ok(updatedSite);
    }

    [HttpGet("{id}/history")]
    public IActionResult GetHistory(long id)
    {
        var site = _siteRepository.Get(id);
        if (site == null)
            return NotFound();

        return Ok(site.History.OrderByDescending(h => h.ChangeDate));
    }

    [HttpGet("search")]
    public IActionResult Search([FromQuery] SiteSearchRequest request)
    {
        var sites = _siteRepository.Search(
            request.CustomerId,
            request.Name,
            request.Address,
            request.IsActive,
            request.HasComments);

        return Ok(sites);
    }

    [HttpGet("{id}/summary")]
    public IActionResult GetSiteSummary(long id)
    {
        var site = _siteRepository.Get(id);
        if (site == null)
            return NotFound();

        var summary = new SiteSummary
        {
            Id = site.Id,
            Name = site.Name,
            TotalProductCount = site.TotalProductCount ?? 0,
            TotalProductValue = site.TotalProductValue ?? 0,
            LastModifiedDate = site.ModifiedDate,
            LastModifiedBy = site.ModifiedBy,
            LastComment = site.Comments?.Split('\n').LastOrDefault(),
            LastCommentDate = site.LastCommentDate,
            IsActive = site.IsActive
        };

        return Ok(summary);
    }
}

public class SiteCreateRequest
{
    public long CustomerId { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string ContactPerson { get; set; }
    public string ContactPhone { get; set; }
    public string UserName { get; set; }
}

public class SiteUpdateRequest
{
    public string Name { get; set; }
    public string Address { get; set; }
    public string ContactPerson { get; set; }
    public string ContactPhone { get; set; }
    public bool IsActive { get; set; }
    public string UserName { get; set; }
    public bool IsActiveChanged { get; set; }
}

public class CommentRequest
{
    public string UserName { get; set; }
    public string Comment { get; set; }
}

public class SiteSearchRequest
{
    public long? CustomerId { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public bool? IsActive { get; set; }
    public bool? HasComments { get; set; }
}

public class SiteSummary
{
    public long Id { get; set; }
    public string Name { get; set; }
    public int TotalProductCount { get; set; }
    public decimal TotalProductValue { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public string LastModifiedBy { get; set; }
    public string LastComment { get; set; }
    public DateTime? LastCommentDate { get; set; }
    public bool IsActive { get; set; }
}