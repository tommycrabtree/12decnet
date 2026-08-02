using api.Data;
using api.DTOs;
using api.Entities;
using api.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers;

public class ProductsController(AppDbContext context) : BaseApiController
{
    [HttpPost] // api
    public async Task<ActionResult<ProductResponseDto>> AddProduct(CreateProductDto createProductDto)
    {
        if (await ProductExists(createProductDto.DisplayName)) return Conflict("Product already exists, playa");

        var product = new Product
        {
            Name = createProductDto.DisplayName,
            UnitsPerCase = createProductDto.UnitsPerCase,
            ShelfCapacity = createProductDto.ShelfCapacity,
            ShelfDaysAllowed = createProductDto.ShelfDaysAllowed
        };

        context.Products.Add(product);
        await context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetProduct),
            new { id = product.Id },
            product.ToDto()
        );
    }

    [HttpGet] // api/products
    public async Task<ActionResult<IReadOnlyList<ProductResponseDto>>> GetProducts()
    {
        var products = await context.Products
            .AsNoTracking()
            .Select(p => new ProductResponseDto
            {
                Id = p.Id,
                DisplayName = p.Name,
                UnitsPerCase = p.UnitsPerCase,
                FreezerUnits = p.FreezerUnits,
                ShelfCapacity = p.ShelfCapacity,
                ShelfDaysAllowed = p.ShelfDaysAllowed
            })
            .ToListAsync();

        return products;
    }

    [HttpGet("{id}")] // api/products/product-id
    public async Task<ActionResult<ProductResponseDto>> GetProduct(int id)
    {
        var product = await context.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

        if (product == null)
        {
            return NotFound("Product not found");            
        }

        return product.ToDto();
    }

    [HttpPost("{id}/receive-case")] // api/products/product-id/receive-case
    public async Task<ActionResult<ProductResponseDto>> ReceiveCase(int id)
    {
        var product = await context.Products.FindAsync(id);

        if (product == null)
        {
            return NotFound("Yellow card for no product");
        }

        product.FreezerUnits += product.UnitsPerCase;

        await context.SaveChangesAsync();

        return Ok(product.ToDto());
    }

    [HttpPost("{id}/subtract-case")] // api/products/product-id/subtract-case
    public async Task<ActionResult<ProductResponseDto>> SubtractCase(int id)
    {
        var product = await context.Products.FindAsync(id);

        if (product == null)
        {
            return NotFound("Yellow card for no subtractable product");
        }

        if (product.FreezerUnits < product.UnitsPerCase)
        {
            return BadRequest("Not enough units to subtract an entire case");
        }

        product.FreezerUnits -= product.UnitsPerCase;

        await context.SaveChangesAsync();

        return Ok(product.ToDto());
    }


    private async Task<bool> ProductExists(string name)
    {
        var normalizedName = name.Trim().ToLower();

        return await context.Products.AnyAsync(x => x.Name.ToLower() == normalizedName);
    }
}
