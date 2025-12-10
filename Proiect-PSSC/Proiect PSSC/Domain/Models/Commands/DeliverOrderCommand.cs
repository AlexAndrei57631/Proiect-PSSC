using Domain.Models.Entities;

namespace Domain.Models.Commands
{
    public record DeliverOrderCommand(UnvalidatedShipment InputShipment);
}