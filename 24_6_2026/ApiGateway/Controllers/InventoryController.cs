using ApiGateway.Models;
using ApiGateway.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Controllers;

[ApiController]
[Route("api/inventory")]
public sealed class InventoryController(IInventoryServiceClient inventoryServiceClient) : ControllerBase
{
    [HttpGet("reservations")]
    [ProducesResponseType(typeof(IEnumerable<InventoryReservationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<InventoryReservationDto>>> GetReservations(
        CancellationToken cancellationToken)
    {
        var reservations = await inventoryServiceClient.GetReservationsAsync(cancellationToken);
        return Ok(reservations);
    }
}
