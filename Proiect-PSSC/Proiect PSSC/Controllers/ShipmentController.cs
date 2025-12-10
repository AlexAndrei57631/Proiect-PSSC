using Microsoft.AspNetCore.Mvc;
using Domain.Workflows;
using Domain.Models.Entities;
using Domain.Models.Commands;
using static Domain.Events.OrderDeliveredEvent;
using Proiect_PSSC.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Proiect_PSSC.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ShipmentController : ControllerBase
    {
        private readonly DeliverOrderWorkflow _workflow;
        private readonly ApplicationDbContext _dbContext;

        public ShipmentController(DeliverOrderWorkflow workflow, ApplicationDbContext dbContext)
        {
            _workflow = workflow;
            _dbContext = dbContext;
        }

        [HttpPost("deliver-order")]
        public async Task<IActionResult> DeliverOrder([FromBody] ShipmentRequestDto request)
        {
            // 1. Transform DTO to UnvalidatedShipment
            var unvalidatedItems = new List<UnvalidatedShipmentItem>();
            foreach (var item in request.Items)
            {
                unvalidatedItems.Add(new UnvalidatedShipmentItem(item.ProductId, item.Quantity));
            }

            var unvalidatedShipment = new UnvalidatedShipment(
                request.OrderId,
                request.CustomerId,
                unvalidatedItems,
                request.DeliveryAddress
            );

            var command = new DeliverOrderCommand(unvalidatedShipment);

            // 2. Mock dependencies
            Func<string, bool> checkOrderExists = (id) => id.StartsWith("ORD");
            Func<string, bool> checkCustomerExists = (id) => id.StartsWith("CUST");
            Func<string, bool> checkProductExists = (id) => true;
            Func<string, string> generateTrackingNumber = (carrier) => $"TRK-{Guid.NewGuid().ToString().Substring(0, 8)}";
            Func<string, string> assignCarrier = (address) => "FedEx";
            Func<string, string, bool> confirmDelivery = (tracking, recipient) => true;
            Func<string, string> getRecipientName = (customerId) => "John Doe";

            // 3. Execute workflow
            var result = _workflow.Execute(
                command,
                checkOrderExists,
                checkCustomerExists,
                checkProductExists,
                generateTrackingNumber,
                assignCarrier,
                confirmDelivery,
                getRecipientName
            );

            // 4. Process result
            return result switch
            {
                OrderDeliveredSucceededEvent success => await SaveShipmentAndReturnResponse(success),
                OrderDeliveredFailedEvent failed => BadRequest(new
                {
                    Message = "Shipment delivery failed.",
                    Reasons = failed.Reasons
                }),
                _ => StatusCode(500, "Unknown state")
            };
        }

        private async Task<IActionResult> SaveShipmentAndReturnResponse(OrderDeliveredSucceededEvent successEvent)
        {
            try
            {
                _dbContext.DeliveredShipments.Add(successEvent.Shipment);
                await _dbContext.SaveChangesAsync();

                return Ok(new
                {
                    Message = "Order delivered successfully!",
                    OrderId = successEvent.Shipment.OrderId.Value,
                    TrackingNumber = successEvent.Shipment.TrackingNumber,
                    DeliveredAt = successEvent.DeliveredDate,
                    CsvData = successEvent.Csv
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "Shipment processed but failed to save to database.",
                    Error = ex.Message
                });
            }
        }
    }

    // DTOs
    public class ShipmentRequestDto
    {
        public string OrderId { get; set; }
        public string CustomerId { get; set; }
        public string DeliveryAddress { get; set; }
        public List<ShipmentItemDto> Items { get; set; }
    }

    public class ShipmentItemDto
    {
        public string ProductId { get; set; }
        public int Quantity { get; set; }
    }
}