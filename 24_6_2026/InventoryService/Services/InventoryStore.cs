using System.Collections.Concurrent;
using InventoryService.Models;

namespace InventoryService.Services;

public sealed class InventoryStore
{
    private readonly ConcurrentDictionary<Guid, InventoryReservation> _reservations = new();

    public IReadOnlyCollection<InventoryReservation> GetAll() =>
        _reservations.Values.OrderByDescending(item => item.ProcessedAtUtc).ToList();

    public void AddReservation(InventoryReservation reservation) =>
        _reservations[reservation.OrderId] = reservation;
}
