using Microsoft.AspNetCore.Mvc;
using QLKho.Api.Models.DTOs;
using QLKho.Api.Services.Interfaces;

namespace QLKho.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BrandsController : ControllerBase
{
    private readonly IBrandService _brandService;

    public BrandsController(IBrandService brandService)
    {
        _brandService = brandService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BrandDto>>> GetBrands()
    {
        return Ok(await _brandService.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BrandDto>> GetBrand(int id)
    {
        var brand = await _brandService.GetByIdAsync(id);
        if (brand == null) return NotFound(new { message = "Không tìm thấy thương hiệu" });
        return Ok(brand);
    }

    [HttpPost]
    public async Task<ActionResult<BrandDto>> CreateBrand([FromBody] CreateBrandDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var created = await _brandService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetBrand), new { id = created.BrandID }, created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<BrandDto>> UpdateBrand(int id, [FromBody] UpdateBrandDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var updated = await _brandService.UpdateAsync(id, dto);
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Không tìm thấy thương hiệu" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBrand(int id)
    {
        try
        {
            var success = await _brandService.DeleteAsync(id);
            if (!success) return NotFound(new { message = "Không tìm thấy thương hiệu" });
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
