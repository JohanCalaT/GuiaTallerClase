using AutoMapper;
using DevWorkshop.TaskAPI.Application.DTOs.Users;
using DevWorkshop.TaskAPI.Application.Interfaces;
using DevWorkshop.TaskAPI.Domain.Entities;
using Microsoft.Extensions.Logging;
using BCrypt.Net;

namespace DevWorkshop.TaskAPI.Application.Services;

/// <summary>
/// Servicio para la gestión de usuarios
/// </summary>
public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;

    public UserService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UserService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// TODO: ESTUDIANTE - Implementar la obtención de todos los usuarios activos
    /// 
    /// Pasos a seguir:
    /// 1. Consultar la base de datos para obtener usuarios donde IsActive = true
    /// 2. Mapear las entidades User a UserDto usando AutoMapper
    /// 3. Retornar la lista de usuarios
    /// 
    /// Tip: Usar async/await y ToListAsync() para operaciones asíncronas
    /// </summary>
    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        // TODO: ESTUDIANTE - Implementar lógica
        throw new NotImplementedException("Método pendiente de implementación por el estudiante");
    }

    /// <summary>
    /// TODO: ESTUDIANTE - Implementar la búsqueda de usuario por ID
    /// 
    /// Pasos a seguir:
    /// 1. Buscar el usuario en la base de datos por UserId
    /// 2. Verificar que el usuario existe y está activo
    /// 3. Mapear la entidad a UserDto
    /// 4. Retornar el usuario o null si no existe
    /// </summary>
    public async Task<UserDto?> GetUserByIdAsync(int userId)
    {
        // TODO: ESTUDIANTE - Implementar lógica
        throw new NotImplementedException("Método pendiente de implementación por el estudiante");
    }

    /// <summary>
    /// TODO: ESTUDIANTE - Implementar la búsqueda de usuario por email
    /// 
    /// Pasos a seguir:
    /// 1. Buscar el usuario en la base de datos por Email
    /// 2. Verificar que el usuario existe y está activo
    /// 3. Mapear la entidad a UserDto
    /// 4. Retornar el usuario o null si no existe
    /// </summary>
    public async Task<UserDto?> GetUserByEmailAsync(string email)
    {
        // TODO: ESTUDIANTE - Implementar lógica
        throw new NotImplementedException("Método pendiente de implementación por el estudiante");
    }

    /// <summary>
    /// Implementa la creación de un nuevo usuario
    ///
    /// Pasos implementados:
    /// 1. Validar que el email no esté en uso
    /// 2. Hashear la contraseña usando BCrypt
    /// 3. Crear una nueva entidad User con los datos del DTO
    /// 4. Establecer CreatedAt = DateTime.UtcNow
    /// 5. Guardar en la base de datos
    /// 6. Mapear la entidad creada a UserDto y retornar
    /// </summary>
    public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
    {
        try
        {
            _logger.LogInformation("Iniciando creación de usuario con email: {Email}", createUserDto.Email);

            // 1. Validar que el email no esté en uso (ya se hace en el controlador, pero doble verificación)
            var normalizedEmail = createUserDto.Email.Trim().ToLower();
            var existingUser = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);
            if (existingUser != null)
            {
                _logger.LogWarning("Intento de crear usuario con email existente: {Email}", createUserDto.Email);
                throw new InvalidOperationException("El email ya está en uso");
            }

            // 2. Hashear la contraseña usando BCrypt
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password);

            // 3. Crear una nueva entidad User con los datos del DTO usando AutoMapper
            var user = _mapper.Map<User>(createUserDto);
            user.Email = normalizedEmail; // Asegurar que el email se guarde normalizado
            user.PasswordHash = passwordHash;
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow; // Asignar fecha actual ya que la BD no permite NULL
            user.LastTokenIssueAt = DateTime.UtcNow; // Asignar fecha actual ya que la BD no permite NULL
            user.RoleId = 4; // Asignar rol "User without Team" por defecto

            // 4. Guardar en la base de datos
            var createdUser = await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Usuario creado exitosamente con ID: {UserId}", createdUser.UserId);

            // 5. Mapear la entidad creada a UserDto y retornar
            return _mapper.Map<UserDto>(createdUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear usuario con email: {Email}", createUserDto.Email);
            throw;
        }
    }
    /// <summary>
    /// TODO: ESTUDIANTE - Implementar la actualización de un usuario
    /// 
    /// Pasos a seguir:
    /// 1. Buscar el usuario existente por ID
    /// 2. Verificar que el usuario existe
    /// 3. Si se actualiza el email, validar que no esté en uso por otro usuario
    /// 4. Actualizar solo los campos que no sean null en el DTO
    /// 5. Establecer UpdatedAt = DateTime.UtcNow
    /// 6. Guardar cambios en la base de datos
    /// 7. Mapear y retornar el usuario actualizado
    /// </summary>
    public async Task<UserDto?> UpdateUserAsync(int userId, UpdateUserDto updateUserDto)
    {
        // TODO: ESTUDIANTE - Implementar lógica
        throw new NotImplementedException("Método pendiente de implementación por el estudiante");
    }

    /// <summary>
    /// TODO: ESTUDIANTE - Implementar la eliminación lógica de un usuario
    /// 
    /// Pasos a seguir:
    /// 1. Buscar el usuario por ID
    /// 2. Verificar que el usuario existe
    /// 3. Establecer IsActive = false (soft delete)
    /// 4. Establecer UpdatedAt = DateTime.UtcNow
    /// 5. Guardar cambios en la base de datos
    /// 6. Retornar true si se eliminó correctamente
    /// </summary>
    public async Task<bool> DeleteUserAsync(int userId)
    {
        // TODO: ESTUDIANTE - Implementar lógica
        throw new NotImplementedException("Método pendiente de implementación por el estudiante");
    }

    /// <summary>
    /// TODO: ESTUDIANTE - Implementar la verificación de email existente
    /// 
    /// Pasos a seguir:
    /// 1. Buscar usuarios con el email especificado
    /// 2. Si se proporciona excludeUserId, excluir ese usuario de la búsqueda
    /// 3. Retornar true si existe algún usuario con ese email
    /// </summary>
    public async Task<bool> EmailExistsAsync(string email, int? excludeUserId = null)
    {
        // TODO: ESTUDIANTE - Implementar lógica
        throw new NotImplementedException("Método pendiente de implementación por el estudiante");
    }
}
