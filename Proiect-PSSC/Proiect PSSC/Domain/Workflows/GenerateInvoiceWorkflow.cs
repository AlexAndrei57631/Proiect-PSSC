using Domain.Models.Commands;
using Domain.Models.Entities;
using Domain.Operations.Invoice;
using Domain.Events;
using static Domain.Events.InvoiceSentEvent;
using System;

namespace Domain.Workflows
{
    public class GenerateInvoiceWorkflow
    {
        /// <summary>
        /// Execute invoice generation workflow
        /// Flow: Validate → Generate → Send
        /// </summary>
        public IInvoiceSentEvent Execute(
            GenerateInvoiceCommand command,
            Func<string, bool> orderExists,
            Func<string, bool> customerExists,
            Func<string, bool> productExists,
            Func<string> generateInvoiceId,
            Func<string> generateInvoiceNumber,
            Func<string, string, bool> sendInvoice,
            Func<string, string> getCustomerEmail)
        {
            // Step 1: Start with unvalidated invoice from command
            IInvoice invoice = command.InputInvoice;

            // Step 2: Validate (UnvalidatedInvoice → ValidatedInvoice or InvalidInvoice)
            invoice = new ValidateInvoiceOperation(orderExists, customerExists, productExists)
                .Transform(invoice);

            // Step 3: Generate (ValidatedInvoice → GeneratedInvoice or InvalidInvoice)
            invoice = new GenerateInvoiceOperation(generateInvoiceId, generateInvoiceNumber)
                .Transform(invoice);

            // Step 4: Send (GeneratedInvoice → SentInvoice or InvalidInvoice)
            invoice = new SendInvoiceOperation(sendInvoice, getCustomerEmail)
                .Transform(invoice);

            // Step 5: Convert final state to event
            return invoice.ToEvent();
        }
    }
}