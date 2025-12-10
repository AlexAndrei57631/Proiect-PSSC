using Domain.Models.Entities;
using Domain.Models.Commands;
using Domain.Operations.Invoice;
using Domain.Events;
using static Domain.Events.InvoicePublishedEvent;
using System;
using System.Linq;

namespace Domain.Workflows
{
    public class PublishInvoiceWorkflow
    {
        // Modificăm semnatura Execute să accepte TOATE dependințele cerute de operațiuni
        public IInvoicePublishedEvent Execute(
            RequestInvoiceCommand command,
            Func<string, bool> checkOrderExists,
            Func<string, bool> checkCustomerExists,
            Func<string, bool> checkProductExists,
            Func<string> generateInvoiceId,
            Func<string> generateInvoiceDate,
            Func<string, string, bool> sendInvoice,
            Func<string, string> formatInvoiceBody
            )
        {
            IInvoice invoice = command.InputInvoice;

            // 1. Validare
            // Conform erorii, ValidateInvoiceOperation cere 3 funcții (Order, Customer, Product)
            invoice = new ValidateInvoiceOperation(checkOrderExists, checkCustomerExists, checkProductExists)
                .Transform(invoice);

            // 2. Generare Factură
            // Conform erorii, GenerateInvoiceOperation cere 2 funcții (probabil ID și Data)
            invoice = new GenerateInvoiceOperation(generateInvoiceId, generateInvoiceDate)
                .Transform(invoice);

            // 3. Trimitere/Publicare Factură
            // Conform erorii, SendInvoiceOperation cere 2 funcții (Trimitere efectivă și Formator mesaj)
            invoice = new SendInvoiceOperation(sendInvoice, formatInvoiceBody)
                .Transform(invoice);

            // 4. Returnare rezultat
            return invoice switch
            {
                // CORECTAT: Am sters ".Value" de la sent.InvoiceNumber
                SentInvoice sent => new InvoicePublishedSucceededEvent(sent.InvoiceId.Value, sent.InvoiceNumber),

                InvalidInvoice invalid => new InvoicePublishedFailedEvent(invalid.Reasons),

                _ => new InvoicePublishedFailedEvent(new[] { "Unknown state generated" })
            };
        }
    }
}