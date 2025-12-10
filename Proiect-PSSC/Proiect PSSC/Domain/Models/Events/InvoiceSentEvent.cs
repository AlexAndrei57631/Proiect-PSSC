using Domain.Models.Entities;
using System;
using System.Collections.Generic;

namespace Domain.Events
{
    public static class InvoiceSentEvent
    {
        // Interface marker
        public interface IInvoiceSentEvent { }

        // Success event
        public record InvoiceSentSucceededEvent : IInvoiceSentEvent
        {
            public string Csv { get; }
            public DateTime SentDate { get; }
            public SentInvoice Invoice { get; }

            internal InvoiceSentSucceededEvent(SentInvoice invoice, DateTime sentDate)
            {
                Invoice = invoice;
                SentDate = sentDate;
                Csv = $"{invoice.CustomerId},{invoice.InvoiceId},{invoice.InvoiceNumber},{invoice.SentAt}";
            }
        }

        // Failure event
        public record InvoiceSentFailedEvent : IInvoiceSentEvent
        {
            public IEnumerable<string> Reasons { get; }

            internal InvoiceSentFailedEvent(IEnumerable<string> reasons)
            {
                Reasons = reasons;
            }
        }

        // Extension method - THIS IS SARCINA 3.3!
        public static IInvoiceSentEvent ToEvent(this IInvoice invoice) => invoice switch
        {
            // Success path
            SentInvoice sentInvoice => new InvoiceSentSucceededEvent(sentInvoice, DateTime.Now),

            // Failure paths
            InvalidInvoice invalidInvoice => new InvoiceSentFailedEvent(invalidInvoice.Reasons),

            // Unexpected states
            _ => new InvoiceSentFailedEvent(new[] { $"Unexpected state: {invoice.GetType().Name}" })
        };
    }
}