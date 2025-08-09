using System.ComponentModel.DataAnnotations;

namespace DevWorkshop.TaskAPI.Application.DTOs.Roles;

/// <summary>
/// DTO para crear un nuevo rol
/// </summary>
public class CreateRoleDto
{
    /// <summary>
    /// Nombre del rol a crear
    /// </summary>
    [Required(ErrorMessage = "El nombre del rol es obligatorio")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre del rol debe tener entre 2 y 100 caracteres")]
    [RegularExpression(@"^[a-zA-ZÀ-ÿ\u00f1\u00d1\s]+$", ErrorMessage = "El nombre del rol solo puede contener letras y espacios")]
    public string RoleName { get; set; } = string.Empty;
}
