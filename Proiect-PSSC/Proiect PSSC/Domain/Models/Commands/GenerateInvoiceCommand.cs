using Domain.Models.Entities;

namespace Domain.Models.Commands
{
    public record GenerateInvoiceCommand(UnvalidatedInvoice InputInvoice);
}