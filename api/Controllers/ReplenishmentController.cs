using api.Data;
using api.DTOs;
using api.Entities;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

public class ReplenishmentController(AppDbContext context) : BaseApiController
{
    [HttpPost]
    public async Task<ActionResult<ReplenishmentSessionDto>> CreateReplenishmentSession(CreateReplenishmentSessionDto dto)
    {
        var replenishmentSession = new ReplenishmentSession();

        var now = DateTimeOffset.UtcNow;

        foreach (var dateBatchDto in dto.DateBatches)
        {
            var product = await context.Products.FindAsync(dateBatchDto.ProductId);

            if (product == null)
            {
                return BadRequest($"Product with id {dateBatchDto.ProductId} was not found");
            }

            var expectedExpirationDate = now.Date.AddDays(product.ShelfDaysAllowed);

            var dateBatch = new DateBatch
            {
                ProductId = product.Id,
                RequestUnits = dateBatchDto.RequestUnits,
                DateUnits = dateBatchDto.DateUnits,
                CreatedOn = now,
                ExpirationDate = expectedExpirationDate
            };

            replenishmentSession.DateBatches.Add(dateBatch);
        }

        context.ReplenishmentSessions.Add(replenishmentSession);

        await context.SaveChangesAsync();

        var result = new ReplenishmentSessionDto
        {
            Id = replenishmentSession.Id,
            DateBatches = replenishmentSession.DateBatches
                .Select(dateBatch => new DateBatchDto
                {
                    Id = dateBatch.Id,
                    ProductId = dateBatch.ProductId,
                    ReplenishmentSessionId = replenishmentSession.Id,
                    RequestUnits = dateBatch.RequestUnits,
                    DateUnits = dateBatch.DateUnits,
                    CreatedOn = dateBatch.CreatedOn,
                    ExpirationDate = dateBatch.ExpirationDate,
                    FiftyPercentOffUnits = dateBatch.FiftyPercentOffUnits,
                    DonationUnits = dateBatch.DonationUnits
                })
                .ToList()
        };

        return Ok(result);
    }
}
