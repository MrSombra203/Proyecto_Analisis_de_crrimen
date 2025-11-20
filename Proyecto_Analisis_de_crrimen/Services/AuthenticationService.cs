using Microsoft.EntityFrameworkCore;
using Proyecto_Analisis_de_crimen.Models;

namespace Proyecto_Analisis_de_crimen.Services
{
    /// <summary>
    /// Servicio encargado de la autenticación de usuarios en el sistema.
    /// 
    /// Este servicio maneja:
    /// - Validación de credenciales (usuario y contraseña)
    /// - Verificación de existencia de usuarios y emails
    /// - Actualización de información de acceso
    /// 
    /// NOTA DE SEGURIDAD: Este sistema almacena contraseñas en texto plano.
    /// En un entorno de producción, se debe implementar hashing (bcrypt, PBKDF2, etc.)
    /// </summary>
    public class AuthenticationService
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Constructor que recibe el contexto de base de datos mediante inyección de dependencias
        /// </summary>
        public AuthenticationService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Autentica un usuario verificando sus credenciales contra la base de datos.
        /// 
        /// Proceso de autenticación:
        /// 1. Valida que los parámetros no estén vacíos
        /// 2. Busca el usuario en la BD por nombre de usuario y estado activo
        /// 3. Compara la contraseña proporcionada con la almacenada
        /// 4. Si es válido, actualiza la fecha de último acceso
        /// 5. Retorna el objeto Usuario completo (con su Rol) o null si falla
        /// 
        /// IMPORTANTE: La contraseña se compara en texto plano. En producción usar hashing.
        /// </summary>
        /// <param name="nombreUsuario">Nombre de usuario a autenticar</param>
        /// <param name="password">Contraseña del usuario</param>
        /// <returns>Objeto Usuario si la autenticación es exitosa, null en caso contrario</returns>
        public async Task<Usuario?> AuthenticateAsync(string nombreUsuario, string password)
        {
            // Validación inicial: rechazar credenciales vacías
            if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrWhiteSpace(password))
                return null;

            // Buscar usuario en la base de datos
            // Include(u => u.Rol) carga también la información del rol relacionado (eager loading)
            // Solo busca usuarios activos (u.Activo == true)
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario && u.Activo);

            // Si no se encuentra el usuario, retornar null
            if (usuario == null)
                return null;

            // Comparar contraseña directamente (texto plano según la BD)
            // En producción, aquí se debería usar: BCrypt.Verify(password, usuario.Password)
            if (usuario.Password != password)
                return null;

            // Actualizar último acceso del usuario
            usuario.UltimoAcceso = DateTime.Now;
            await _context.SaveChangesAsync();

            return usuario;
        }

        /// <summary>
        /// Obtiene un usuario específico por su ID desde la base de datos.
        /// 
        /// Este método es útil para recuperar información completa de un usuario
        /// cuando ya se conoce su identificador único.
        /// </summary>
        /// <param name="id">ID del usuario a buscar</param>
        /// <returns>Objeto Usuario con su Rol incluido, o null si no existe</returns>
        public async Task<Usuario?> GetUsuarioByIdAsync(int id)
        {
            return await _context.Usuarios
                .Include(u => u.Rol)  // Cargar también el rol relacionado
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        /// <summary>
        /// Verifica si un nombre de usuario ya está registrado en el sistema.
        /// 
        /// Este método es útil para validar la unicidad del nombre de usuario
        /// antes de crear o editar un registro.
        /// 
        /// El parámetro excludeId permite excluir un usuario específico de la búsqueda,
        /// útil cuando se está editando un usuario existente y se quiere verificar
        /// que el nuevo nombre no esté en uso por OTRO usuario.
        /// </summary>
        /// <param name="nombreUsuario">Nombre de usuario a verificar</param>
        /// <param name="excludeId">ID de usuario a excluir de la búsqueda (opcional, para edición)</param>
        /// <returns>true si el nombre ya existe, false si está disponible</returns>
        public async Task<bool> UsuarioExisteAsync(string nombreUsuario, int? excludeId = null)
        {
            // Construir la consulta base: buscar usuarios con ese nombre
            var query = _context.Usuarios.Where(u => u.NombreUsuario == nombreUsuario);
            
            // Si se proporciona un ID a excluir, filtrarlo (útil al editar)
            if (excludeId.HasValue)
            {
                query = query.Where(u => u.Id != excludeId.Value);
            }

            // Retornar true si existe al menos un usuario con ese nombre
            return await query.AnyAsync();
        }

        /// <summary>
        /// Verifica si un email ya está registrado en el sistema.
        /// 
        /// Similar a UsuarioExisteAsync, pero para emails. Garantiza que cada
        /// usuario tenga un email único en el sistema.
        /// </summary>
        /// <param name="email">Email a verificar</param>
        /// <param name="excludeId">ID de usuario a excluir de la búsqueda (opcional, para edición)</param>
        /// <returns>true si el email ya existe, false si está disponible</returns>
        public async Task<bool> EmailExisteAsync(string email, int? excludeId = null)
        {
            // Construir la consulta base: buscar usuarios con ese email
            var query = _context.Usuarios.Where(u => u.Email == email);
            
            // Si se proporciona un ID a excluir, filtrarlo (útil al editar)
            if (excludeId.HasValue)
            {
                query = query.Where(u => u.Id != excludeId.Value);
            }

            // Retornar true si existe al menos un usuario con ese email
            return await query.AnyAsync();
        }
    }
}
