using AutoMapper;
using DevWorkshop.TaskAPI.Application.DTOs.Users;
using DevWorkshop.TaskAPI.Application.Interfaces;
using DevWorkshop.TaskAPI.Domain.Entities;
using Microsoft.Extensions.Logging;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;

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

    public async Task<UserDto?> GetUserByEmailAsync(string email)
    {
        try
        {
            _logger.LogInformation("Buscando usuario por email: {Email}", email);

            var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                _logger.LogInformation("No se encontró usuario con email: {Email}", email);
                return null;
            }

            var userDto = _mapper.Map<UserDto>(user);

            _logger.LogInformation("Usuario encontrado con ID: {UserId}", user.UserId);
            return userDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar usuario por email: {Email}", email);
            throw;
        }
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

    public async Task<bool> EmailExistsAsync(string email, int? excludeUserId = null)
    {
        try
        {
            _logger.LogInformation("Verificando si el email existe: {Email}", email);

            var normalizedEmail = email.Trim().ToLower();

            if (excludeUserId.HasValue)
            {
                var exists = await _unitOfWork.Users.AnyAsync(u =>
                    u.Email.ToLower() == normalizedEmail && u.UserId != excludeUserId.Value);
                _logger.LogInformation("Email {Email} existe (excluyendo UserId {ExcludeUserId}): {Exists}",
                    email, excludeUserId.Value, exists);
                return exists;
            }
            else
            {
                var exists = await _unitOfWork.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail);
                _logger.LogInformation("Email {Email} existe: {Exists}", email, exists);
                return exists;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar si el email existe: {Email}", email);
            throw;
        }
    }

    /// <summary>
    /// Obtiene la entidad User completa por email
    /// </summary>
    public async Task<User?> GetUserEntityByEmailAsync(string email)
    {
        try
        {
            _logger.LogInformation("Buscando entidad User por email: {Email}", email);

            var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user != null)
            {
                _logger.LogInformation("Entidad User encontrada para email: {Email}", email);
            }
            else
            {
                _logger.LogInformation("No se encontró entidad User para email: {Email}", email);
            }

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar entidad User por email: {Email}", email);
            throw;
        }
    }

    /// <summary>
    /// Obtiene la entidad User completa por ID
    /// </summary>
    public async Task<User?> GetUserEntityByIdAsync(int userId)
    {
        try
        {
            _logger.LogInformation("Buscando entidad User por ID: {UserId}", userId);

            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            if (user != null)
            {
                _logger.LogInformation("Entidad User encontrada para ID: {UserId}", userId);
            }
            else
            {
                _logger.LogInformation("No se encontró entidad User para ID: {UserId}", userId);
            }

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar entidad User por ID: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Actualiza una entidad User en la base de datos
    /// </summary>
    public async Task<bool> UpdateUserEntityAsync(User user)
    {
        try
        {
            _logger.LogInformation("Actualizando entidad User con ID: {UserId}", user.UserId);

            // Estrategia alternativa para evitar problemas con triggers de SQL Server
            // En lugar de usar Update, vamos a usar una actualización específica del campo LastTokenIssueAt
            var existingUser = await _unitOfWork.Users.GetByIdAsync(user.UserId);
            if (existingUser == null)
            {
                _logger.LogWarning("Usuario no encontrado para actualización: {UserId}", user.UserId);
                return false;
            }

            // Solo actualizar el campo que necesitamos para el logout
            existingUser.LastTokenIssueAt = user.LastTokenIssueAt;

            // Usar Update pero con manejo específico para triggers
            _unitOfWork.Users.Update(existingUser);

            try
            {
                var result = await _unitOfWork.SaveChangesAsync();

                if (result > 0)
                {
                    _logger.LogInformation("Entidad User actualizada exitosamente: {UserId}", user.UserId);
                    return true;
                }
                else
                {
                    _logger.LogWarning("No se pudo actualizar la entidad User: {UserId}", user.UserId);
                    return false;
                }
            }
            catch (Exception saveEx) when (saveEx.Message.Contains("database triggers"))
            {
                _logger.LogWarning("Error con triggers detectado, intentando método alternativo para usuario: {UserId}", user.UserId);

                // Método alternativo: usar ExecuteSqlRaw para evitar la cláusula OUTPUT
                var sql = "UPDATE Users SET LastTokenIssueAt = {0} WHERE UserId = {1}";
                var parameters = new object[] { user.LastTokenIssueAt ?? (object)DBNull.Value, user.UserId };
                var rowsAffected = await _unitOfWork.ExecuteSqlRawAsync(sql, parameters);

                if (rowsAffected > 0)
                {
                    _logger.LogInformation("Entidad User actualizada exitosamente usando SQL directo: {UserId}", user.UserId);
                    return true;
                }
                else
                {
                    _logger.LogWarning("No se pudo actualizar la entidad User usando SQL directo: {UserId}", user.UserId);
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar entidad User: {UserId}", user.UserId);
            throw;
        }
    }
}
