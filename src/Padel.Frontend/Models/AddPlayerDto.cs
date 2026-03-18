using System.ComponentModel.DataAnnotations;

namespace Padel.Frontend.Models;

public class AddPlayerDto
{
    [Required(ErrorMessage = "Le matricule du joueur est requis.")]
    public string Matricule { get; set; } = string.Empty;

    [Required]
    public string OrganizerMatricule { get; set; } = string.Empty;
}
