using Microsoft.AspNetCore.Mvc;
using Domain.Workflows;
using Domain.Models.Entities;
using Domain.Models.Commands;
using static Domain.Events.InvoiceSentEvent;
using Proiect_PSSC.Data;
using Domain.Models.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Proiect_PSSC.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InvoiceController : ControllerBase
    {
        private readonly GenerateInvoiceWorkflow _workflow;
        private readonly ApplicationDbContext _dbContext;

        public InvoiceController(GenerateInvoiceWorkflow workflow, ApplicationDbContext dbContext)
        {
            _workflow = workflow;
            _dbContext = dbContext;
        }

        [HttpPost("generate-invoice")]
        public async Task<IActionResult> GenerateInvoice([FromBody] InvoiceRequestDto request)
        {
            // 1. Transform DTO to UnvalidatedInvoice
            var unvalidatedItems = new List<UnvalidatedInvoiceItem>();
            foreach (var item in request.Items)
            {
                unvalidatedItems.Add(new UnvalidatedInvoiceItem(item.ProductId, item.Quantity, item.UnitPrice));
            }

            var unvalidatedInvoice = new UnvalidatedInvoice(
                request.OrderId,
                request.CustomerId,
                unvalidatedItems,
                request.TotalAmount,
                request.BillingAddress
            );

            var command = new GenerateInvoiceCommand(unvalidatedInvoice);

            // 2. Mock dependencies
            Func<string, bool> checkOrderExists = (id) => id.StartsWith("ORD");
            Func<string, bool> checkCustomerExists = (id) => id.StartsWith("CUST");
            Func<string, bool> checkProductExists = (id) => true;
            Func<string> generateInvoiceId = () => $"INV-{Guid.NewGuid().ToString().Substring(0, 8)}";
            Func<string> generateInvoiceNumber = () => $"INV-2024-{DateTime.Now.Ticks % 10000}";
            Func<string, string, bool> sendInvoice = (id, email) => true;
            Func<string, string> getCustomerEmail = (id) => "customer@example.com";

            // 3. Execute workflow
            var result = _workflow.Execute(
                command,
                checkOrderExists,
                checkCustomerExists,
                checkProductExists,
                generateInvoiceId,
                generateInvoiceNumber,
                sendInvoice,
                getCustomerEmail
            );

            // 4. Process result
            return result switch
            {
                InvoiceSentSucceededEvent success => await SaveInvoiceAndReturnResponse(success),
                InvoiceSentFailedEvent failed => BadRequest(new
                {
                    Message = "Invoice generation failed.",
                    Reasons = failed.Reasons
                }),
                _ => StatusCode(500, "Unknown state")
            };
        }

        private async Task<IActionResult> SaveInvoiceAndReturnResponse(InvoiceSentSucceededEvent successEvent)
        {
            try
            {
                _dbContext.SentInvoices.Add(successEvent.Invoice);
                await _dbContext.SaveChangesAsync();

                return Ok(new
                {
                    Message = "Invoice generated and sent successfully!",
                    InvoiceId = successEvent.Invoice.InvoiceId.Value,
                    InvoiceNumber = successEvent.Invoice.InvoiceNumber,
                    SentDate = successEvent.SentDate,
                    CsvData = successEvent.Csv
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "Invoice processed but failed to save to database.",
                    Error = ex.Message
                });
            }
        }
    }

    // DTOs
    public class InvoiceRequestDto
    {
        public string OrderId { get; set; }
        public string CustomerId { get; set; }
        public string BillingAddress { get; set; }
        public List<InvoiceItemDto> Items { get; set; }
        public string TotalAmount { get; set; }
    }

    public class InvoiceItemDto
    {
        public string ProductId { get; set; }
        public int Quantity { get; set; }
        public string UnitPrice { get; set; }
    }
}