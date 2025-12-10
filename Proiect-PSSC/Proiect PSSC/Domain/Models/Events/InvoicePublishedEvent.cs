using Domain.Models.Entities;
using System;
using System.Collections.Generic;

namespace Domain.Events
{
    public static class InvoicePublishedEvent
    {
        public interface IInvoicePublishedEvent { }

        // Eveniment de succes
        public record InvoicePublishedSucceededEvent : IInvoicePublishedEvent
        {
            public string Csv { get; }
            public DateTime PublishedDate { get; }
            public string InvoiceId { get; }

            internal InvoicePublishedSucceededEvent(string invoiceId, string csv)
            {
                InvoiceId = invoiceId;
                Csv = csv;
                PublishedDate = DateTime.Now;
            }
        }

        // Eveniment de eșec
        public record InvoicePublishedFailedEvent : IInvoicePublishedEvent
        {
            public IEnumerable<string> Reasons { get; }
            internal InvoicePublishedFailedEvent(IEnumerable<string> reasons) => Reasons = reasons;
        }
    }
}