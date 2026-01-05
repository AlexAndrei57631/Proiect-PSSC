using System;
using System.Collections.Generic;
using Domain.Models.Entities;

namespace Domain.Events
{
    public static class OrderDeliveredEvent
    {
        public interface IOrderDeliveredEvent { }

        public record OrderDeliveredSucceededEvent(DeliveredShipment Shipment, DateTime DeliveredDate, string Csv) : IOrderDeliveredEvent;

        public record OrderDeliveredFailedEvent(IReadOnlyCollection<string> Reasons) : IOrderDeliveredEvent;

        public static IOrderDeliveredEvent ToEvent(this IShipment shipment) => shipment switch
        {
            DeliveredShipment delivered => new OrderDeliveredSucceededEvent(
                delivered,
                delivered.DeliveredAt,
                $"{delivered.TrackingNumber},{delivered.OrderId.Value},{delivered.RecipientName}"),

            InvalidShipment invalid => new OrderDeliveredFailedEvent(invalid.Reasons),

            _ => new OrderDeliveredFailedEvent(new[] { $"Unexpected state: {shipment.GetType().Name}" })
        };
    }
}