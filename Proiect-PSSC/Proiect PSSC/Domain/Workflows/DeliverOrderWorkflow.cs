using Domain.Models.Commands;
using Domain.Models.Entities;
using Domain.Operations.Shipment;
using Domain.Events;
using static Domain.Events.OrderDeliveredEvent;
using System;

namespace Domain.Workflows
{
    public class DeliverOrderWorkflow
    {
        /// <summary>
        /// Execute order delivery workflow
        /// Flow: Validate → Prepare → Deliver
        /// </summary>
        public IOrderDeliveredEvent Execute(
            DeliverOrderCommand command,
            Func<string, bool> orderExists,
            Func<string, bool> customerExists,
            Func<string, bool> productExists,
            Func<string, string> generateTrackingNumber,
            Func<string, string> assignCarrier,
            Func<string, string, bool> confirmDelivery,
            Func<string, string> getRecipientName)
        {
            // Step 1: Start with unvalidated shipment from command
            IShipment shipment = command.InputShipment;

            // Step 2: Validate (UnvalidatedShipment → ValidatedShipment or InvalidShipment)
            shipment = new ValidateShipmentOperation(orderExists, customerExists, productExists)
                .Transform(shipment);

            // Step 3: Prepare (ValidatedShipment → PreparedShipment or InvalidShipment)
            shipment = new PrepareShipmentOperation(generateTrackingNumber, assignCarrier)
                .Transform(shipment);

            // Step 4: Deliver (PreparedShipment → DeliveredShipment or InvalidShipment)
            shipment = new DeliverShipmentOperation(confirmDelivery, getRecipientName)
                .Transform(shipment);

            // Step 5: Convert final state to event
            return shipment.ToEvent();
        }
    }
}