using MassperoTVAPI.Core.DTOs;
using MassperoTVAPI.Core.Entities;
using MassperoTVAPI.Core.Helpers;
using MassperoTVAPI.Core.Interfaces;
using MassperoTVAPI.Core.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MassperoTVAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,HR")]
public class CategoriesController : ControllerBase
{
    private readonly IUnitOfWork _uow;
     
    public CategoriesController(IUnitOfWork uow) => _uow = uow;

    /// <summary>Retrieve all job categories with pagination and filtering.</summary>
    /// <returns>Paged list of categories.</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<CategoryDto>>>> GetAll([FromQuery] GetCategoriesQueryDto query)
    {
        var pagedList = await _uow.Categories.GetPagedAsync(query);
        
        var dtoItems = pagedList.Items.Select(c => c.ToDto()).ToList();
        var result = new PagedResult<CategoryDto>(
            dtoItems,
            pagedList.TotalCount,
            pagedList.Page,
            pagedList.PageSize,
            pagedList.TotalPages
        );

        return Ok(ApiResponse<PagedResult<CategoryDto>>.SuccessResponse(result));
    }

    /// <summary>Get a single category by ID.</summary>
    /// <param name="id">Category ID.</param>
    /// <returns>The category if found; otherwise 404.</returns>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> GetById(int id)
    {
        var entity = await _uow.Categories.GetByIdAsync(id);
        if (entity is null) return NotFound(ApiResponse<CategoryDto>.NotFoundResponse());
        return Ok(ApiResponse<CategoryDto>.SuccessResponse(entity.ToDto()));
    }

    /// <summary>Create a new category. Admin or HR required.</summary>
    /// <param name="dto">Category name.</param>
    /// <returns>The created category.</returns>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> Create([FromBody] CreateCategoryDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<CategoryDto>.ErrorResponse("Validation failed."));

        var entity = new Category 
        { 
            Name = dto.Name,
            CategoryCode = dto.CategoryCode,
            IsActive = dto.IsActive
        };
        await _uow.Categories.AddAsync(entity);
        await _uow.SaveChangesAsync();

        return StatusCode(201, ApiResponse<CategoryDto>.CreatedResponse(entity.ToDto()));
    }

    /// <summary>Update an existing category. Admin or HR required.</summary>
    /// <param name="id">Category ID.</param>
    /// <param name="dto">Updated category name.</param>
    /// <returns>The updated category.</returns>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> Update(int id, [FromBody] UpdateCategoryDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<CategoryDto>.ErrorResponse("Validation failed."));

        var entity = await _uow.Categories.GetByIdAsync(id);
        if (entity is null) return NotFound(ApiResponse<CategoryDto>.NotFoundResponse());

        entity.Name = dto.Name;
        entity.CategoryCode = dto.CategoryCode;
        entity.IsActive = dto.IsActive;
        _uow.Categories.Update(entity);
        await _uow.SaveChangesAsync();

        return Ok(ApiResponse<CategoryDto>.SuccessResponse(entity.ToDto()));
    }

    /// <summary>Delete a category. Admin only.</summary>
    /// <param name="id">Category ID.</param>
    /// <returns>Success status.</returns>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {

        try
        {
            var entity = await _uow.Categories.GetByIdAsync(id);
            if (entity is null) return NotFound(ApiResponse<object>.NotFoundResponse());
            _uow.Categories.Delete(entity);
            await _uow.SaveChangesAsync();
        }
        catch(Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse($"Can not delete this one {ex.Message }"));
        }
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Deleted succesffuly"));
    }
}
