using InventoryService.Models;
using InventoryService.Services;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.Controllers;

[ApiController]
[Route("api/inventory")]
public sealed class InventoryController(InventoryStore inventoryStore) : ControllerBase
{
    [HttpGet("reservations")]
    public ActionResult<IEnumerable<InventoryReservation>> GetReservations()
    {
        return Ok(inventoryStore.GetAll());
    }
}
