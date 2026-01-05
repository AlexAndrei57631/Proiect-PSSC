using Domain.Models.Entities;

namespace Domain.Models.Commands
{
	// Command pentru procesul complet de expediere (Validare -> Pregãtire -> Livrare)
	public record ShipOrderCommand(UnvalidatedShipment InputShipment);
}