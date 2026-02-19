using System.ComponentModel.DataAnnotations;

namespace Padel.Frontend.Models;

public class UpdateSiteDto
{
    [Required(ErrorMessage = "Le nom est requis.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'adresse est requise.")]
    public string Address { get; set; } = string.Empty;
}
