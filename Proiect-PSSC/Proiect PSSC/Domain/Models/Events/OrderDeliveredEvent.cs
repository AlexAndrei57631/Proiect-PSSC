using Domain.Models.Entities;
using System;
using System.Collections.Generic;

namespace Domain.Events
{
    public static class OrderDeliveredEvent
    {
        // Interface marker
        public interface IOrderDeliveredEvent { }

        // Success event
        public record OrderDeliveredSucceededEvent : IOrderDeliveredEvent
        {
            public string Csv { get; }
            public DateTime DeliveredDate { get; }
            public DeliveredShipment Shipment { get; }

            internal OrderDeliveredSucceededEvent(DeliveredShipment shipment, DateTime deliveredDate)
            {
                Shipment = shipment;
                DeliveredDate = deliveredDate;
                Csv = $"{shipment.OrderId},{shipment.CustomerId},{shipment.TrackingNumber},{shipment.DeliveredAt}";
            }
        }

        // Failure event
        public record OrderDeliveredFailedEvent : IOrderDeliveredEvent
        {
            public IEnumerable<string> Reasons { get; }

            internal OrderDeliveredFailedEvent(IEnumerable<string> reasons)
            {
                Reasons = reasons;
            }
        }

        // Extension method - THIS IS SARCINA 3.3!
        public static IOrderDeliveredEvent ToEvent(this IShipment shipment) => shipment switch
        {
            // Success path
            DeliveredShipment deliveredShipment => new OrderDeliveredSucceededEvent(deliveredShipment, DateTime.Now),

            // Failure paths
            InvalidShipment invalidShipment => new OrderDeliveredFailedEvent(invalidShipment.Reasons),

            // Unexpected states
            _ => new OrderDeliveredFailedEvent(new[] { $"Unexpected state: {shipment.GetType().Name}" })
        };
    }
}