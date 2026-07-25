using api.DTOs;
using api.Entities;

namespace api.Extensions;

public static class ProductExtensions
{
    public static ProductResponseDto ToDto(this Product product)
    {
        return new ProductResponseDto
        {
            Id = product.Id,
            DisplayName = product.Name,
            UnitsPerCase = product.UnitsPerCase,
            ShelfCapacity = product.ShelfCapacity,
            ShelfDaysAllowed = product.ShelfDaysAllowed
        };
    }
}
