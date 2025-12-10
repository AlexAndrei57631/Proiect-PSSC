using Microsoft.AspNetCore.Mvc;
using Domain.Workflows;
using Domain.Models.Entities;
using Domain.Models.Commands;
using static Domain.Events.InvoicePublishedEvent;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Proiect_PSSC.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InvoiceController : ControllerBase
    {
        private readonly PublishInvoiceWorkflow _workflow;

        public InvoiceController(PublishInvoiceWorkflow workflow)
        {
            _workflow = workflow;
        }

        [HttpPost("publish")]
        public IActionResult PublishInvoice([FromBody] InvoiceDto request)
        {
            // --- 1. Pregătire Date (Mapping) ---
            var invoiceItems = request.Items.Select(i =>
                new UnvalidatedInvoiceItem(i.ProductId, i.Quantity, i.UnitPrice)
            ).ToList();

            var unvalidatedInvoice = new UnvalidatedInvoice(
                request.OrderId,
                request.CustomerId,
                invoiceItems,
                request.TotalAmount,
                request.BillingAddress
            );

            var command = new RequestInvoiceCommand(unvalidatedInvoice);

            // ... în interiorul metodei PublishInvoice ...

            // 2. Dependențe Mock
            Func<string, bool> checkOrder = (id) => true;
            Func<string, bool> checkCustomer = (id) => true;
            Func<string, bool> checkProduct = (id) => true;

            Func<string> generateId = () => Guid.NewGuid().ToString();
            Func<string> generateDate = () => DateTime.Now.ToString("yyyy-MM-dd");

            // CORECTAT: formatBody trebuie să primească doar un parametru (conținutul), nu doi.
            Func<string, string> formatBody = (content) => $"FACTURA: {content}";

            // SendInvoice primește (Adresa, Body) -> returnează bool
            Func<string, string, bool> sendInv = (addr, body) => true;

            // --- 3. Execuție Workflow ---
            // Aici pasăm TOATE funcțiile definite mai sus, în ordinea cerută de Workflow
            var result = _workflow.Execute(
                command,
                checkOrder,
                checkCustomer,
                checkProduct,
                generateId,
                generateDate,
                sendInv,
                formatBody
            );

            // --- 4. Răspuns ---
            return result switch
            {
                InvoicePublishedSucceededEvent success => Ok(new { Message = "Factura procesata cu succes!", Id = success.InvoiceId }),
                InvoicePublishedFailedEvent failed => BadRequest(new { Errors = failed.Reasons }),
                _ => StatusCode(500)
            };
        }
    }

    // DTO-uri (Data Transfer Objects)
    public class InvoiceDto
    {
        public string OrderId { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public string BillingAddress { get; set; } = string.Empty;
        public string TotalAmount { get; set; } = "0";
        public List<InvoiceItemDto> Items { get; set; } = new();
    }

    public class InvoiceItemDto
    {
        public string ProductId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string UnitPrice { get; set; } = "0";
    }
}