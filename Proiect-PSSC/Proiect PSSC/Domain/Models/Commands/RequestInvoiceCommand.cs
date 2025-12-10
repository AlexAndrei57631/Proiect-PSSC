using Domain.Models.Entities;

namespace Domain.Models.Commands
{
    // Comanda primește datele brute necesare facturării
    // Poți adapta UnvalidatedInvoice în funcție de constructorul tău din Invoice.cs
    public record RequestInvoiceCommand(UnvalidatedInvoice InputInvoice);
}