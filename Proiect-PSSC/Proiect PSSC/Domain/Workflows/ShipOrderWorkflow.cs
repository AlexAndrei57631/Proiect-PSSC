using Domain.Models.Commands;
using Domain.Models.Entities;
using Domain.Operations;
using Domain.Operations.Shipment; // Asigură-te că namespace-ul este corect
using static Domain.Events.OrderDeliveredEvent; // Schimbat din ShipmentDeliveredEvent
using System;

namespace Domain.Workflows
{
    public class ShipOrderWorkflow
    {
        public IOrderDeliveredEvent Execute( // Schimbat din IShipmentDeliveredEvent
            ShipOrderCommand command,
            Func<string, bool> checkOrderExists,
            Func<string, bool> checkCustomerExists,
            Func<string, bool> checkProductExists,
            Func<string> generateTrackingNumber,
            Func<string> assignCarrier,
            Func<string, string, bool> confirmDelivery,
            Func<string, string> getRecipientName
        )
        {
            IShipment shipment = command.InputShipment;

            shipment = new ValidateShipmentOperation(checkOrderExists, checkCustomerExists, checkProductExists)
                .Transform(shipment);

            shipment = new PrepareShipmentOperation(generateTrackingNumber, assignCarrier)
                .Transform(shipment);

            shipment = new DeliverShipmentOperation(confirmDelivery, getRecipientName)
                .Transform(shipment);

            return shipment.ToEvent();
        }
    }
}