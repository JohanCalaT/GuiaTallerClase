using DevWorkshop.TaskAPI.Application.DTOs.Common;
using DevWorkshop.TaskAPI.Application.DTOs.Roles;
using DevWorkshop.TaskAPI.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DevWorkshop.TaskAPI.Api.Controllers;

/// <summary>
/// Controlador para la gestión de roles - IMPLEMENTADO COMO EJEMPLO
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;
    private readonly ILogger<RolesController> _logger;

    public RolesController(IRoleService roleService, ILogger<RolesController> logger)
    {
        _roleService = roleService;
        _logger = logger;
    }

    /// <summary>
    /// EJEMPLO IMPLEMENTADO: Obtiene todos los roles del sistema
    ///
    /// Este endpoint está completamente implementado como ejemplo para que los estudiantes
    /// vean cómo debe estructurarse un controlador completo con manejo de errores,
    /// logging y respuestas consistentes.
    /// </summary>
    /// <returns>Lista de todos los roles activos</returns>
    [HttpGet("getAll")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<RoleDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<IEnumerable<RoleDto>>>> GetAllRoles()
    {
        try
        {
            _logger.LogInformation("Solicitud para obtener todos los roles");

            var roles = await _roleService.GetAllRolesAsync();

            var response = ApiResponse<IEnumerable<RoleDto>>.SuccessResponse(
                roles, 
                $"Se obtuvieron {roles.Count()} roles correctamente"
            );

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener los roles");
            
            var errorResponse = ApiResponse<IEnumerable<RoleDto>>.ErrorResponse(
                "Error interno del servidor al obtener los roles",
                "Contacte al administrador del sistema"
            );

            return StatusCode(500, errorResponse);
        }
    }

    /// <summary>
    /// EJEMPLO IMPLEMENTADO: Obtiene un rol específico por su ID
    ///
    /// Este endpoint también está implementado como ejemplo de validaciones,
    /// manejo de casos de "no encontrado" y respuestas apropiadas.
    /// </summary>
    /// <param name="id">ID del rol a buscar</param>
    /// <returns>Información del rol solicitado</returns>
    [HttpGet("getById/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> GetRoleById(int id)
    {
        try
        {
            // Validación de entrada
            if (id <= 0)
            {
                _logger.LogWarning("ID de rol inválido: {Id}", id);
                
                var badRequestResponse = ApiResponse<RoleDto>.ErrorResponse(
                    "ID de rol inválido",
                    "El ID debe ser un número mayor a 0"
                );

                return BadRequest(badRequestResponse);
            }

            _logger.LogInformation("Solicitud para obtener rol con ID: {Id}", id);

            var role = await _roleService.GetRoleByIdAsync(id);

            if (role == null)
            {
                _logger.LogWarning("Rol no encontrado con ID: {Id}", id);
                
                var notFoundResponse = ApiResponse<RoleDto>.ErrorResponse(
                    "Rol no encontrado",
                    $"No existe un rol con el ID {id}"
                );

                return NotFound(notFoundResponse);
            }

            var response = ApiResponse<RoleDto>.SuccessResponse(
                role, 
                $"Rol '{role.RoleName}' obtenido correctamente"
            );

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el rol con ID: {Id}", id);
            
            var errorResponse = ApiResponse<RoleDto>.ErrorResponse(
                "Error interno del servidor al obtener el rol",
                "Contacte al administrador del sistema"
            );

            return StatusCode(500, errorResponse);
        }
    }

    /// <summary>
    /// EJEMPLO IMPLEMENTADO: Obtiene estadísticas de los roles
    ///
    /// Endpoint adicional que muestra cómo crear endpoints personalizados
    /// con lógica de negocio más compleja.
    /// </summary>
    /// <returns>Estadísticas de roles en el sistema</returns>
    [HttpGet("getStatistics")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<object>>> GetRoleStatistics()
    {
        try
        {
            _logger.LogInformation("Solicitud para obtener estadísticas de roles");

            var roles = await _roleService.GetAllRolesAsync();

            var statistics = new
            {
                TotalRoles = roles.Count(),
                ActiveRoles = roles.Count(r => r.IsActive),
                TotalUsers = roles.Sum(r => r.UserCount),
                RoleDistribution = roles.Select(r => new
                {
                    r.RoleName,
                    r.UserCount,
                    Percentage = roles.Sum(x => x.UserCount) > 0 
                        ? Math.Round((double)r.UserCount / roles.Sum(x => x.UserCount) * 100, 2)
                        : 0
                }).OrderByDescending(x => x.UserCount),
                MostPopularRole = roles.OrderByDescending(r => r.UserCount).FirstOrDefault()?.RoleName ?? "N/A"
            };

            var response = ApiResponse<object>.SuccessResponse(
                statistics, 
                "Estadísticas de roles obtenidas correctamente"
            );

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadísticas de roles");
            
            var errorResponse = ApiResponse<object>.ErrorResponse(
                "Error interno del servidor al obtener estadísticas",
                "Contacte al administrador del sistema"
            );

            return StatusCode(500, errorResponse);
        }
    }

    /// <summary>
    /// Crear un nuevo rol
    ///
    /// Este endpoint permite crear nuevos roles en el sistema.
    /// Valida que el nombre del rol sea único antes de crearlo.
    /// </summary>
    /// <param name="createRoleDto">Datos del rol a crear</param>
    /// <returns>Rol creado con código 201</returns>
    [HttpPost("create")]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 409)] // Conflict para nombres duplicados
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> CreateRole([FromBody] CreateRoleDto createRoleDto)
    {
        try
        {
            // Validar el modelo de entrada
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Intento de crear rol con datos inválidos: {Errors}",
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));

                return BadRequest(ApiResponse<RoleDto>.ErrorResponse(
                    "Datos de entrada inválidos",
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                ));
            }

            // Crear el rol usando el servicio
            var role = await _roleService.CreateRoleAsync(createRoleDto);

            // Crear respuesta exitosa
            var response = ApiResponse<RoleDto>.SuccessResponse(
                role,
                "Rol creado correctamente"
            );

            _logger.LogInformation("Rol creado exitosamente: {RoleName} con ID: {RoleId}",
                role.RoleName, role.RoleId);

            // Retornar 201 Created con la ubicación del recurso creado
            return CreatedAtAction(
                nameof(GetRoleById),
                new { id = role.RoleId },
                response
            );
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Ya existe un rol"))
        {
            // Manejar conflicto de nombres duplicados
            _logger.LogWarning("Conflicto al crear rol: {Message}", ex.Message);

            return Conflict(ApiResponse<RoleDto>.ErrorResponse(
                "Conflicto de datos",
                ex.Message
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error interno al crear rol: {RoleName}", createRoleDto?.RoleName ?? "Unknown");

            return StatusCode(500, ApiResponse<RoleDto>.ErrorResponse(
                "Error interno del servidor",
                "Ocurrió un error inesperado al crear el rol"
            ));
        }
    }
    /// <summary>
    /// TODO: ESTUDIANTE - Actualizar un rol existente
    ///
    /// Este endpoint debe:
    /// 1. Validar el ID y el modelo
    /// 2. Verificar que el rol existe
    /// 3. Actualizar usando el servicio
    /// 4. Retornar el rol actualizado
    ///
    /// Ejemplo de implementación:
    /// - Usar [HttpPut("{id:int}")]
    /// - Validar ID > 0 y ModelState
    /// - Llamar _roleService.UpdateRoleAsync()
    /// - Manejar caso de rol no encontrado
    /// </summary>
    /// <param name="id">ID del rol a actualizar</param>
    /// <param name="updateRoleDto">Datos a actualizar</param>
    /// <returns>Rol actualizado</returns>
    [HttpPut("update/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> UpdateRole(int id, [FromBody] UpdateRoleDto updateRoleDto)
    {
        // TODO: ESTUDIANTE - Implementar actualización de rol
        return StatusCode(501, ApiResponse<RoleDto>.ErrorResponse(
            "Método no implementado",
            "Este endpoint debe ser implementado por el estudiante"
        ));
    }

    /// <summary>
    /// TODO: ESTUDIANTE - Eliminar un rol
    ///
    /// Este endpoint debe:
    /// 1. Validar el ID
    /// 2. Verificar que el rol existe
    /// 3. Verificar que no tenga usuarios asignados
    /// 4. Eliminar usando el servicio
    /// 5. Retornar confirmación
    ///
    /// Ejemplo de implementación:
    /// - Usar [HttpDelete("{id:int}")]
    /// - Validar ID > 0
    /// - Llamar _roleService.DeleteRoleAsync()
    /// - Retornar NoContent() si se eliminó correctamente
    /// </summary>
    /// <param name="id">ID del rol a eliminar</param>
    /// <returns>Confirmación de eliminación</returns>
    [HttpDelete("delete/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 204)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 409)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteRole(int id)
    {
        // TODO: ESTUDIANTE - Implementar eliminación de rol
        return StatusCode(501, ApiResponse<object>.ErrorResponse(
            "Método no implementado",
            "Este endpoint debe ser implementado por el estudiante"
        ));
    }
}
