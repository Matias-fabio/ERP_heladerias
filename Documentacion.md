### 🏛️ Visión Arquitectónica y Estrategia Inicial

Antes de escribir la primera línea de código, recordemos por qué elegimos Clean Architecture:

1.  Regla de Dependencia en Cebolla (Onion/Clean Architecture): Las dependencias solo van hacia adentro.  
     • El Dominio no sabe que existe PostgreSQL, EF Core, MediatR o ASP.NET Core. Contiene la pura lógica de negocio y las reglas del helado  
     (recetas, lotes, rendimientos, mermas).  
     • La Aplicación orquesta los casos de uso (CQRS con MediatR) e interactúa con el Dominio.  
     • La Infraestructura implementa los accesos a datos (EF Core, PostgreSQL, JWT, servicios de correo/pdf).  
     • La API expone las interfaces HTTP, maneja Middlewares, DTOs de transporte y mapea los controladores.
    [ GelatoERP.Api (Web API) ]
    │
    ▼
    [ GelatoERP.Infrastructure ]
    │
    ▼
    [ GelatoERP.Application ]
    │
    ▼
    [ GelatoERP.Domain ] ◄── (Núcleo Puro sin dependencias externas)
    ──────

## 🚀 PASO 1 del Sprint 1: Creación de la Solución y Proyectos .NET

Vamos a crear la Solución C# (.NET Core) limpia mediante la CLI de .NET.

### 1.1 Comandos para crear la solución y proyectos

Abre tu terminal en la carpeta raíz ERP_heladerias (o ejecuta estos comandos):

    # 1. Crear el archivo de Solución (.sln)
    dotnet new sln -n GelatoERP

    # 2. Crear la capa de Dominio (Class Library)
    dotnet new classlib -o src/GelatoERP.Domain -f net8.0

    # 3. Crear la capa de Aplicación (Class Library)
    dotnet new classlib -o src/GelatoERP.Application -f net8.0

    # 4. Crear la capa de Infraestructura (Class Library)
    dotnet new classlib -o src/GelatoERP.Infrastructure -f net8.0

    # 5. Crear la capa API (Web API ASP.NET Core)
    dotnet new webapi -o src/GelatoERP.Api -f net8.0

    # 6. Agregar todos los proyectos a la Solución
    dotnet sln add src/GelatoERP.Domain/GelatoERP.Domain.csproj
    dotnet sln add src/GelatoERP.Application/GelatoERP.Application.csproj
    dotnet sln add src/GelatoERP.Infrastructure/GelatoERP.Infrastructure.csproj
    dotnet sln add src/GelatoERP.Api/GelatoERP.Api.csproj

    # 7. Configurar las Referencias de Proyectos (Regla de Dependencia)
    # Application -> Domain
    dotnet add src/GelatoERP.Application/GelatoERP.Application.csproj reference src/GelatoERP.Domain/GelatoERP.Domain.csproj

    # Infrastructure -> Application (e implícitamente Domain)
    dotnet add src/GelatoERP.Infrastructure/GelatoERP.Infrastructure.csproj reference src/GelatoERP.Application/GelatoERP.Application.csproj

    # Api -> Infrastructure y Application
    dotnet add src/GelatoERP.Api/GelatoERP.Api.csproj reference src/GelatoERP.Infrastructure/GelatoERP.Infrastructure.csproj
    dotnet add src/GelatoERP.Api/GelatoERP.Api.csproj reference src/GelatoERP.Application/GelatoERP.Application.csproj
    ──────

### 🎓 Explicación Didáctica: ¿Por qué esta estructura de referencias?

• GelatoERP.Domain: Tiene 0 referencias a otros proyectos. No instala ningún NuGet de base de datos ni de framework web. ¿Por qué? Para  
 garantizar que si mañana cambiamos de EF Core a Dapper, o de PostgreSQL a SQL Server, las reglas de negocio del helado no cambien ni 1 sola línea
de código.  
 • GelatoERP.Application: Conoce solo a Domain. Define interfaces de repositorios (ej: ITenantRepository, IUnitOfWork), abstrae servicios y define
Comandos/Consultas (CQRS).  
 • GelatoERP.Infrastructure: Implementa las interfaces definidas en Application usando Entity Framework Core, Npgsql (PostgreSQL), controladores  
 JWT, etc.  
 • GelatoERP.Api: Es el punto de entrada ejecutable. Configura la Inyección de Dependencias (DI) uniendo Infrastructure y Application, expone  
 Endpoints y ejecuta Middlewares.  
 ──────

### ⚠️ Errores Comunes en Producción y Debugging (Para tener en cuenta)

1. Dependencia Circular (Circular Dependency Error):  
   • Error: Ocurre si por descuido intentas referenciar Domain desde Infrastructure y al mismo tiempo Infrastructure desde Domain. .NET fallará
   al compilar (CS0114).  
   • Prevención: El Dominio es intocable y sagrado; nunca referencia a nada.
2. Fuga de Abstracción (Leaking Abstractions):  
   • Error: Usar tipos de Entity Framework (como DbSet<T> o DbContext) dentro de Domain o dentro de los Handlers de Application.  
   • Solución: En Application solo usamos IQueryable<T> o métodos de interfaces de repositorio como Task<Tenant?> GetByIdAsync(...).

──────

### 🧩 Pregunta de Control #1 (Desafío Corto)

Para validar que estamos alineados antes de avanzar al modelo de datos de Tenants y Usuarios:

│ ¿Por qué definimos la interfaz IUnitOfWork o ITenantRepository en la capa Application (o Domain), pero escribimos su código concreto (clase  
 │ TenantRepository) en la capa Infrastructure? ¿Qué principio SOLID respalda esto?

La respuesta exacta es el Principio de Inversión de Dependencias (DIP), la D de los principios SOLID.

¿Qué significa en la práctica?

1. Los módulos de alto nivel (Dominio / Aplicación) no deben depender de los de bajo nivel (Infraestructura / EF Core). Ambos deben depender de  
   Abstracciones (Interfaces).
2. ¿Por qué lo hacemos?  
   • Testeabilidad (Unit Tests): Si Application dependiera directamente de la clase TenantRepository de EF Core, no podrías probar tus casos de
   uso sin tener PostgreSQL corriendo. Al depender de una interfaz (ITenantRepository), en tus pruebas unitarias puedes pasarle un "Mock" o  
   "Fake" en memoria super rápido.  
   • Independencia tecnológica: Si el día de mañana decides guardar ciertos logs o lotes en Redis o Mongo, la capa de Aplicación ni se entera;  
   solo cambias la clase en Infrastructure que implementa la interfaz.

──────

## 🎯 PASO 2 del Sprint 1: Construcción del Dominio Core

Vamos a empezar en el proyecto GelatoERP.Domain.

### 🔹 2.1 Crear la carpeta Common y las clases base

En el proyecto GelatoERP.Domain, crea una carpeta llamada Common. Dentro de ella vas a crear dos archivos:

#### 📄 Archivo 1: Common/BaseEntity.cs

Esta será la clase padre de casi todas nuestras entidades de base de datos.

    namespace GelatoERP.Domain.Common;

    /// <summary>
    /// Clase base abstracta para todas las entidades del dominio.
    /// Incluye identificador único (GUID) y auditoría básica.
    /// </summary>
    public abstract class BaseEntity
    {
        // Usamos Guid para evitar ataques de enumeración (ej. /api/tenants/1, /api/tenants/2)
        public Guid Id { get; protected set; } = Guid.NewGuid();

        // Siempre guardamos la fecha en UTC para evitar problemas con zonas horarias
        public DateTime CreatedAtUtc { get; protected set; } = DateTime.UtcNow;
        public string? CreatedBy { get; protected set; }

        public DateTime? LastModifiedAtUtc { get; protected set; }
        public string? LastModifiedBy { get; protected set; }

        // Soft Delete: En un ERP NUNCA se borra un registro físico de la BD si tiene historial comercial/de producción.
        public bool IsDeleted { get; protected set; } = false;

        public void MarkAsDeleted(string? deletedBy = null)
        {
            IsDeleted = true;
            LastModifiedAtUtc = DateTime.UtcNow;
            LastModifiedBy = deletedBy;
        }

        public void UpdateAuditInfo(string? modifiedBy = null)
        {
            LastModifiedAtUtc = DateTime.UtcNow;
            LastModifiedBy = modifiedBy;
        }
    }
    ──────

#### 📄 Archivo 2: Common/ITenantEntity.cs

Esta interfaz la implementarán todas las entidades que pertenezcan obligatoriamente a una Empresa / Heladería (ej: Sabores, Recetas, Lotes,  
 Usuarios).

    namespace GelatoERP.Domain.Common;

    /// <summary>
    /// Contrato para asegurar que una entidad pertenece a un Tenant (Empresa/Heladería) específico.
    /// EF Core utilizará esta interfaz para aplicar filtros globales automáticos (Global Query Filters).
    /// </summary>
    public interface ITenantEntity
    {
        public Guid TenantId { get; set; }
    }
    ──────

### 🎓 Explicación Técnica y Buenas Prácticas C#

1. protected set en lugar de public set:  
   • ¿Por qué? Mantiene el encapsulamiento (DDD). No queremos que cualquier servicio externo le cambie el Id o modifique la fecha de creación a
   su antojo. Solo la propia entidad o sus clases hijas pueden alterar estas propiedades.
2. Guid vs int autoincremental:  
   • ¿Por qué? En un ERP SaaS multi-empresa, los IDs numéricos (1, 2, 3...) son peligrosos porque un usuario de la Heladería A podría adivinar  
   la URL de la Heladería B (/api/pedidos/104). Además, los GUIDs permiten generar IDs en el cliente/servidor antes de guardar en base de datos.
3. DateTime.UtcNow:  
   • ¿Por qué? Nunca guardes DateTime.Now en la base de datos. Si el servidor de producción está en Virginia (Azure/AWS) y la fábrica de helados
   en Buenos Aires o Madrid, las horas se desfasarán. Guardar en UTC y convertir al huso horario del usuario en la UI es la regla de oro.

──────

## 🎯 PASO 2.2: Entidades Tenant y Plant en GelatoERP.Domain

En la industria heladera, una empresa (Tenant) puede tener una o varias instalaciones:

• Planta de Producción / Fábrica: Donde están las marmitas, pasteurizadoras, abatidores y cámaras de congelación.  
 • Punto de Venta / Sucursal / Depósito: Donde solo se almacena o distribuye producto terminado (carapinas/baldes).  
 ──────

### 📄 Archivo 1: Crear Enums/TenantStatus.cs

Primero crea una carpeta llamada Enums dentro de GelatoERP.Domain y agrega este archivo:

    namespace GelatoERP.Domain.Enums;

    public enum TenantStatus
    {
        Active = 1,      // Empresa activa con suscripción al día
        Trialing = 2,    // En período de prueba gratis (ej: 14 días)
        Suspended = 3,   // Suspendida por falta de pago
        Cancelled = 4    // Suscripción cancelada
    }
    ──────

### 📄 Archivo 2: Crear Entities/Tenant.cs

Crea una carpeta llamada Entities dentro de GelatoERP.Domain y agrega la entidad Tenant.cs:

    using GelatoERP.Domain.Common;
    using GelatoERP.Domain.Enums;

    namespace GelatoERP.Domain.Entities;

    /// <summary>
    /// Representa a la Empresa/Heladería (Suscritor del SaaS).
    /// Hereda de BaseEntity (tiene Id, CreatedAtUtc, IsDeleted, etc).
    /// </summary>
    public class Tenant : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;
        public string TaxId { get; private set; } = string.Empty; // CUIT / NIF / RUC
        public string DomainOrSlug { get; private set; } = string.Empty; // Ej: "luccianos" para luccianos.gelatoerp.com
        public TenantStatus Status { get; private set; } = TenantStatus.Trialing;

        // Colecciones de Navegación de EF Core (Encapsuladas como ReadOnly)
        private readonly List<Plant> _plants = new();
        public IReadOnlyCollection<Plant> Plants => _plants.AsReadOnly();

        private readonly List<User> _users = new();
        public IReadOnlyCollection<User> Users => _users.AsReadOnly();

        // Constructor privado para Entity Framework Core
        private Tenant() { }

        // Factory Method (Patrón de Creación en DDD)
        public Tenant(string name, string taxId, string domainOrSlug)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("El nombre de la empresa es obligatorio.", nameof(name));

            if (string.IsNullOrWhiteSpace(taxId))
                throw new ArgumentException("El identificador fiscal (TaxId) es obligatorio.", nameof(taxId));

            Name = name.Trim();
            TaxId = taxId.Trim();
            DomainOrSlug = domainOrSlug.Trim().ToLowerInvariant();
            Status = TenantStatus.Trialing;
        }

        // Métodos de Dominio para modificar el estado (DDD en lugar de setters públicos)
        public void UpdateInfo(string name, string taxId)
        {
            Name = name.Trim();
            TaxId = taxId.Trim();
            UpdateAuditInfo();
        }

        public void ChangeStatus(TenantStatus newStatus)
        {
            Status = newStatus;
            UpdateAuditInfo();
        }
    }
    ──────

### 📄 Archivo 3: Crear Entities/Plant.cs

En la misma carpeta Entities, crea Plant.cs (representa las Plantas o Sucursales de la Heladería):

    using GelatoERP.Domain.Common;

    namespace GelatoERP.Domain.Entities;

    /// <summary>
    /// Representa una Planta de Producción, Depósito o Sucursal de una Heladería.
    /// Implementa ITenantEntity para obligar al aislamiento por TenantId.
    /// </summary>
    public class Plant : BaseEntity, ITenantEntity
    {
        public Guid TenantId { get; set; }
        public string Name { get; private set; } = string.Empty;
        public string Code { get; private set; } = string.Empty; // Ej: "PL-01", "SUC-CENTRO"
        public string Address { get; private set; } = string.Empty;
        public bool IsProductionPlant { get; private set; } = true; // true = Fábrica de helado, false = Depósito/Sucursal
        public bool IsActive { get; private set; } = true;

        // Propiedad de Navegación hacia el Tenant padre
        public Tenant Tenant { get; private set; } = null!;

        private Plant() { }

        public Plant(Guid tenantId, string name, string code, string address, bool isProductionPlant)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("El nombre de la planta es obligatorio.", nameof(name));

            TenantId = tenantId;
            Name = name.Trim();
            Code = code.Trim().ToUpperInvariant();
            Address = address.Trim();
            IsProductionPlant = isProductionPlant;
            IsActive = true;
        }

        public void Update(string name, string code, string address, bool isProductionPlant)
        {
            Name = name.Trim();
            Code = code.Trim().ToUpperInvariant();
            Address = address.Trim();
            IsProductionPlant = isProductionPlant;
            UpdateAuditInfo();
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdateAuditInfo();
        }
    }
    ──────

### 🎓 Explicación Didáctica de C# y DDD

1. IReadOnlyCollection<Plant> + private readonly List<Plant>:  
   • ¿Por qué no usar public List<Plant> Plants { get; set; }?  
   • En DDD evitamos el "Modelo de Dominio Anémico" (donde las clases son solo contenedores de datos sin reglas). Si la lista fuera pública con
   set, cualquiera afuera podría hacer tenant.Plants.Clear() o modificar plantas sin pasar por la lógica del Tenant. Con IReadOnlyCollection,  
   obligas a que las mutaciones se hagan mediante métodos de la entidad.
2. Tenant = null!; (Null Forgiving Operator):  
   • El operador null! en C# 8+ le dice al compilador: "Sé que esta propiedad parece nula al inicializar la clase, pero Entity Framework Core se
   encargará de cargarla en tiempo de ejecución". Evita advertencias irrelevantes de C#.
3. Factory Constructor (new Tenant(...) con validaciones):  
   • Garantiza que nunca se cree un Tenant en memoria que sea inválido (por ejemplo sin nombre o sin TaxId).

## 🎯 PASO 2.3: Autenticación, Usuarios y Roles RBAC en GelatoERP.Domain

En un ERP empresarial para heladerías, los permisos deben ser estrictos. Por ejemplo:

• Un Jefe de Producción necesita ver/crear recetas, insumos y registrar pasteurización, pero no debería ver la facturación total o ganancias de  
 la empresa.  
 • Un Repartidor / Chofer necesita ver los pedidos del día y registrar la devolución de carapinas (envases), pero no puede modificar las fórmulas
del helado.  
 • Un SuperAdmin del SaaS (tú) puede administrar las empresas clientes.

Para lograr esto implementaremos RBAC (Role-Based Access Control) con aislamiento por Tenant.  
 ──────

### 📄 Archivo 1: Crear Enums/UserRoleType.cs

En la carpeta Domain/Enums, agrega este enumerador:

    namespace GelatoERP.Domain.Enums;

    public enum UserRoleType
    {
        SuperAdmin = 1,        // Administrador global de la plataforma SaaS (Tú)
        TenantAdmin = 2,       // Dueño / Gerente General de la Heladería
        ProductionManager = 3, // Jefe de Planta / Maestro Heladero / Pasteurizador
        LogisticsManager = 4,  // Encargado de Despacho, Reparto y Carapinas
        PointOfSaleUser = 5,   // Operario de Sucursal / Mostrador
        QualityInspector = 6   // Técnico de Laboratorio / Control de Calidad
    }
    ──────

### 📄 Archivo 2: Crear Entities/Role.cs

En la carpeta Domain/Entities, agrega Role.cs:

    using GelatoERP.Domain.Common;
    using GelatoERP.Domain.Enums;

    namespace GelatoERP.Domain.Entities;

    /// <summary>
    /// Representa un Rol dentro del sistema (RBAC).
    /// </summary>
    public class Role : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public UserRoleType RoleType { get; private set; }

        private readonly List<UserRole> _userRoles = new();
        public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

        private Role() { }

        public Role(string name, string description, UserRoleType roleType)
        {
            Name = name.Trim();
            Description = description.Trim();
            RoleType = roleType;
        }
    }
    ──────

### 📄 Archivo 3: Crear Entities/User.cs

En la carpeta Domain/Entities, agrega User.cs:

    using GelatoERP.Domain.Common;

    namespace GelatoERP.Domain.Entities;

    /// <summary>
    /// Representa a un Usuario del ERP.
    /// Implementa ITenantEntity para forzar que pertenezca a una Heladería.
    /// </summary>
    public class User : BaseEntity, ITenantEntity
    {
        public Guid TenantId { get; set; }
        public string FirstName { get; private set; } = string.Empty;
        public string LastName { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string PasswordHash { get; private set; } = string.Empty;
        public bool IsActive { get; private set; } = true;

        // Planta/Sucursal por defecto asignada al usuario (Opcional)
        public Guid? AssignedPlantId { get; private set; }
        public Plant? AssignedPlant { get; private set; }

        // Propiedad de Navegación hacia la Heladería
        public Tenant Tenant { get; private set; } = null!;

        // Colección de Roles del Usuario
        private readonly List<UserRole> _userRoles = new();
        public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

        private User() { }

        public User(Guid tenantId, string firstName, string lastName, string email, string passwordHash, Guid? assignedPlantId = null)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("El email es obligatorio.", nameof(email));

            TenantId = tenantId;
            FirstName = firstName.Trim();
            LastName = lastName.Trim();
            Email = email.Trim().ToLowerInvariant(); // Normalizamos siempre a minúsculas
            PasswordHash = passwordHash;
            AssignedPlantId = assignedPlantId;
            IsActive = true;
        }

        public string FullName => $"{FirstName} {LastName}";

        public void UpdateProfile(string firstName, string lastName, Guid? assignedPlantId)
        {
            FirstName = firstName.Trim();
            LastName = lastName.Trim();
            AssignedPlantId = assignedPlantId;
            UpdateAuditInfo();
        }

        public void SetPasswordHash(string newPasswordHash)
        {
            if (string.IsNullOrWhiteSpace(newPasswordHash))
                throw new ArgumentException("El hash de la contraseña no puede estar vacío.", nameof(newPasswordHash));

            PasswordHash = newPasswordHash;
            UpdateAuditInfo();
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdateAuditInfo();
        }

        public void Activate()
        {
            IsActive = true;
            UpdateAuditInfo();
        }
    }
    ──────

### 📄 Archivo 4: Crear Entities/UserRole.cs

En la carpeta Domain/Entities, crea la entidad intermedia de la relación Muchos-a-Muchos:

    namespace GelatoERP.Domain.Entities;

    /// <summary>
    /// Tabla pivote Muchos-a-Muchos entre User y Role.
    /// </summary>
    public class UserRole
    {
        public Guid UserId { get; private set; }
        public User User { get; private set; } = null!;

        public Guid RoleId { get; private set; }
        public Role Role { get; private set; } = null!;

        private UserRole() { }

        public UserRole(Guid userId, Guid roleId)
        {
            UserId = userId;
            RoleId = roleId;
        }
    }
    ──────

### 🎓 Explicación Didáctica de Seguridad y C#

1. PasswordHash vs Contraseña en texto plano:  
   • En la entidad NUNCA se almacena la contraseña limpia (ej: "Helado123"). Solo se guarda el Hash cifrado (que generaremos en Infrastructure  
   usando algoritmos seguros como BCrypt o Argon2).
2. Email.ToLowerInvariant():  
   • En C#, la comparación de strings depende del idioma y servidor si no se normaliza. Al registrar o buscar usuarios, siempre pasamos el email
   a minúsculas invariantes para evitar que "Juan@heladeria.com" y "juan@heladeria.com" se traten como cuentas distintas.
3. Guid? AssignedPlantId (Tipos Nullable):  
   • El signo ? indica que la propiedad es opcional. Un Gerente General (TenantAdmin) puede no tener una planta fija porque administra todas las
   sucursales, mientras que un pastelero de fábrica sí tendrá su AssignedPlantId apuntando a la planta de producción.

La instalación de paquetes NuGet (Npgsql.EntityFrameworkCore.PostgreSQL, EF Core Design, BCrypt, MediatR y FluentValidation) está completándose en segundo  
 plano.

Mientras termina, pasamos a diseñar el PASO 3 del Sprint 1: Infraestructura y Persistencia Multi-Tenant con EF Core y PostgreSQL.  
 ──────

## 🎯 PASO 3: Aislamiento por Tenant (ICurrentTenantService) y ApplicationDbContext

En un ERP Multi-Empresa, el mayor riesgo es que un programador olvide escribir WHERE tenant_id = '...' en alguna consulta LINQ y se filtren datos de otra  
 heladería.

Para solucionar esto de raíz, implementaremos 2 mecanismos en C# / Entity Framework Core:

1. ICurrentTenantService: Una interfaz en Application que extrae el TenantId del usuario que hace la solicitud HTTP (a partir de su Token JWT).
2. Global Query Filters en EF Core: Un filtro automático en el DbContext que intercepta todas las consultas de la base de datos y le agrega WHERE "TenantId"
   == currentTenantId sin que tú tengas que escribirlo manualmente en cada Query.  
   ──────

### 📄 Archivo 1: Interfaz en GelatoERP.Application/Common/Interfaces/ICurrentTenantService.cs

Crea la carpeta Common/Interfaces dentro de GelatoERP.Application y agrega:

    namespace GelatoERP.Application.Common.Interfaces;

    /// <summary>
    /// Proporciona la información del Tenant (Empresa) y Usuario actual en el contexto de la solicitud HTTP.
    /// </summary>
    public interface ICurrentTenantService
    {
        public Guid? TenantId { get; }
        public string? UserId { get; }
        public bool IsSuperAdmin { get; }
    }
    ──────

### 📄 Archivo 2: Implementación en GelatoERP.Infrastructure/Services/CurrentTenantService.cs

Crea la carpeta Services dentro de GelatoERP.Infrastructure y agrega:

    using System.Security.Claims;
    using GelatoERP.Application.Common.Interfaces;
    using Microsoft.AspNetCore.Http;

    namespace GelatoERP.Infrastructure.Services;

    public class CurrentTenantService : ICurrentTenantService
    {
        public Guid? TenantId { get; }
        public string? UserId { get; }
        public bool IsSuperAdmin { get; }

        public CurrentTenantService(IHttpContextAccessor httpContextAccessor)
        {
            var user = httpContextAccessor.HttpContext?.User;

            if (user is null) return;

            // Extraemos el UserId del Claim Types.NameIdentifier (o "sub")
            UserId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            // Extraemos si es SuperAdmin de la plataforma
            var role = user.FindFirstValue(ClaimTypes.Role);
            IsSuperAdmin = role == "SuperAdmin";

            // Extraemos el TenantId del Claim personalizado "tenant_id"
            var tenantClaim = user.FindFirstValue("tenant_id");
            if (Guid.TryParse(tenantClaim, out var tenantId))
            {
                TenantId = tenantId;
            }
        }
    }
    ──────

### 📄 Archivo 3: Interfaz del DbContext en GelatoERP.Application/Common/Interfaces/IApplicationDbContext.cs

En la carpeta GelatoERP.Application/Common/Interfaces, agrega esta interfaz (abstracción del DbContext para los Handlers CQRS):

    using GelatoERP.Domain.Entities;
    using Microsoft.EntityFrameworkCore;

    namespace GelatoERP.Application.Common.Interfaces;

    public interface IApplicationDbContext
    {
        DbSet<Tenant> Tenants { get; }
        DbSet<Plant> Plants { get; }
        DbSet<User> Users { get; }
        DbSet<Role> Roles { get; }
        DbSet<UserRole> UserRoles { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
    ──────

### 🎓 Explicación Didáctica

1. IHttpContextAccessor: Es la herramienta de ASP.NET Core que nos permite leer los encabezados, la sesión o los Claims del Token JWT del cliente en cualquier capa del sistema sin acoplar la base de datos a la Web API.
2. IApplicationDbContext: Siguiendo el principio de Inversión de Dependencias (DIP), los comandos y consultas MediatR en Application usarán la interfaz IApplicationDbContext.

──────

### 📄 Archivo 4: Clase Concreta DbContext en GelatoERP.Infrastructure/Persistence/ApplicationDbContext.cs

Crea la carpeta Persistence dentro de GelatoERP.Infrastructure y agrega ApplicationDbContext.cs:

    using GelatoERP.Application.Common.Interfaces;
    using GelatoERP.Domain.Common;
    using GelatoERP.Domain.Entities;
    using Microsoft.EntityFrameworkCore;

    namespace GelatoERP.Infrastructure.Persistence;

    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        private readonly ICurrentTenantService _currentTenantService;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            ICurrentTenantService currentTenantService)
            : base(options)
        {
            _currentTenantService = currentTenantService;
        }

        public DbSet<Tenant> Tenants => Set<Tenant>();
        public DbSet<Plant> Plants => Set<Plant>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar relación Muchos-a-Muchos UserRole (Clave primaria compuesta)
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            // Global Query Filters (Aislamiento por Tenant y Soft Delete)
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                // 1. Soft Delete Filter (IsDeleted == false)
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                    var body = System.Linq.Expressions.Expression.Equal(
                        System.Linq.Expressions.Expression.Property(parameter, nameof(BaseEntity.IsDeleted)),
                        System.Linq.Expressions.Expression.Constant(false));

                    var lambda = System.Linq.Expressions.Expression.Lambda(body, parameter);
                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }

                // 2. Multi-Tenant Filter (TenantId == currentTenantId)
                if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                    var tenantIdProperty = System.Linq.Expressions.Expression.Property(parameter, nameof(ITenantEntity.TenantId));
                    var currentTenantIdValue = System.Linq.Expressions.Expression.Property(
                        System.Linq.Expressions.Expression.Constant(_currentTenantService),
                        nameof(ICurrentTenantService.TenantId));

                    // tenantId == _currentTenantService.TenantId
                    var body = System.Linq.Expressions.Expression.Equal(
                        tenantIdProperty,
                        System.Linq.Expressions.Expression.Convert(currentTenantIdValue, typeof(Guid)));

                    var lambda = System.Linq.Expressions.Expression.Lambda(body, parameter);
                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        // La fecha de creación la setea la propia entidad (UtcNow), registramos auditoría
                        entry.Property(e => e.CreatedBy).CurrentValue = _currentTenantService.UserId;
                        break;

                    case EntityState.Modified:
                        entry.Entity.UpdateAuditInfo(_currentTenantService.UserId);
                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }

────── 2. IApplicationDbContext: Siguiendo el principio de Inversión de Dependencias (DIP) que repasamos antes, los comandos y consultas MediatR en Application  
 usarán la interfaz IApplicationDbContext para hacer \_context.Tenants.Add(...) o \_context.Users.ToListAsync(...). No dependerán directamente de la clase  
 concreta GelatoDbContext de la capa Infrastructure.

### 🎯 PASO 3.4: Construir la Persistencia con ApplicationDbContext

Ahora vamos a implementar el corazón del acceso a datos y aislamiento Multi-Tenant en Entity Framework Core.

#### 🏛️ Conceptos clave que tenés que entender de este paso:

1. Global Query Filters (Filtros Globales Automáticos):  
   • En lugar de obligar al programador a escribir .Where(x => x.TenantId == tenantId && !x.IsDeleted) en cada consulta
   LINQ del sistema, usamos Expresiones Lambda de C# en el OnModelCreating.  
   • EF Core interceptará todas las consultas enviadas a PostgreSQL y agregará automáticamente estas condiciones. Si  
   entra un usuario de la Heladería Lucciano's, solo verá datos con su TenantId.
2. Auditoría Automática en SaveChangesAsync:  
   • Sobreescribimos el método SaveChangesAsync.  
   • Antes de enviar los datos a PostgreSQL, inspeccionamos ChangeTracker.Entries<BaseEntity>().  
   • Si la entidad es nueva (Added), le asignamos CreatedBy con el UserId del usuario actual.  
   • Si la entidad se está editando (Modified), llamamos a UpdateAuditInfo(\_currentTenantService.UserId) para actualizar
   LastModifiedAtUtc y LastModifiedBy.

──────

### 📄 Instrucciones: Crear el archivo ApplicationDbContext.cs

1.  En el proyecto GelatoERP.Infrastructure, crea una carpeta llamada Persistence.
2.  Dentro de Persistence, crea el archivo ApplicationDbContext.cs.
3.  Pega el siguiente código:

    using GelatoERP.Application.Common.Interfaces;
    using GelatoERP.Domain.Common;
    using GelatoERP.Domain.Entities;
    using Microsoft.EntityFrameworkCore;

    namespace GelatoERP.Infrastructure.Persistence;

    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
    private readonly ICurrentTenantService \_currentTenantService;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            ICurrentTenantService currentTenantService)
            : base(options)
        {
            _currentTenantService = currentTenantService;
        }

        public DbSet<Tenant> Tenants => Set<Tenant>();
        public DbSet<Plant> Plants => Set<Plant>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Configurar clave primaria compuesta para la tabla intermedia UserRole
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            // 2. Aplicar Global Query Filters a las entidades
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                // Soft Delete Filter: Oculta registros marcados como IsDeleted = true
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                    var body = System.Linq.Expressions.Expression.Equal(
                        System.Linq.Expressions.Expression.Property(parameter, nameof(BaseEntity.IsDeleted)),
                        System.Linq.Expressions.Expression.Constant(false));

                    var lambda = System.Linq.Expressions.Expression.Lambda(body, parameter);
                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }

                // Multi-Tenant Filter: Filtra automáticamente por TenantId
                if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");

                    var tenantIdProperty = System.Linq.Expressions.Expression.Property(parameter, nameof(ITenantEntity.TenantId));

                    var currentTenantIdValue = System.Linq.Expressions.Expression.Property(
                    System.Linq.Expressions.Expression.Constant(\_currentTenantService),nameof(ICurrentTenantService.TenantId));

                    var body = System.Linq.Expressions.Expression.Equal(
                        tenantIdProperty,
                        System.Linq.Expressions.Expression.Convert(currentTenantIdValue, typeof(Guid)));

                    var lambda = System.Linq.Expressions.Expression.Lambda(body, parameter);
                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Property(e => e.CreatedBy).CurrentValue = _currentTenantService.UserId;
                        break;

                    case EntityState.Modified:
                        entry.Entity.UpdateAuditInfo(_currentTenantService.UserId);
                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

    }
    ──────

### 🎯 Próximo Paso: Paso 3.5 — Configurar la Inyección de Dependencias (DI)

Para que el proyecto GelatoERP.Api pueda usar la base de datos y los servicios sin acoplarse directamente a las  
 implementaciones concretas, debemos registrar las dependencias en cada capa.  
 ──────

#### 📄 Instrucción 1: Crear DependencyInjection.cs en GelatoERP.Application

1.  En la raíz del proyecto src/GelatoERP.Application, crea un archivo llamado DependencyInjection.cs.
2.  Pega el siguiente código:

    using Microsoft.Extensions.DependencyInjection;
    using System.Reflection;

    namespace GelatoERP.Application;

    public static class DependencyInjection
    {
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
    // Registrar MediatR para manejar Commands y Queries (CQRS)
    services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    });

            return services;
        }

    }

Explicación: Este extensión permite que con solo llamar a builder.Services.AddApplicationServices() en el Program.cs de  
 la API, se registren automáticamente todos los handlers de MediatR y validadores que vayamos agregando en la capa  
 Application.  
 ──────

#### 📄 Instrucción 2: Crear DependencyInjection.cs en GelatoERP.Infrastructure

1.  En la raíz del proyecto src/GelatoERP.Infrastructure, crea un archivo llamado DependencyInjection.cs.
2.  Pega el siguiente código:

    using GelatoERP.Application.Common.Interfaces;
    using GelatoERP.Infrastructure.Persistence;
    using GelatoERP.Infrastructure.Services;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    namespace GelatoERP.Infrastructure;

    public static class DependencyInjection
    {
    public static IServiceCollection AddInfrastructureServices(
    this IServiceCollection services,
    IConfiguration configuration)
    {
    // 1. Registrar servicio para resolver el Tenant y Usuario de la petición actual
    services.AddHttpContextAccessor();
    services.AddScoped<ICurrentTenantService, CurrentTenantService>();

            // 2. Registrar DbContext con PostgreSQL
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString, b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.

FullName)));

            // 3. Registrar IApplicationDbContext para la Inversión de Dependencias
            services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

            return services;
        }
    }

Explicación: Aquí registramos el servicio que identifica qué heladería/tenant está realizando el request  
 (CurrentTenantService) y configuramos la conexión a PostgreSQL a través de Entity Framework Core.  
 ──────

### 🎯 PASO 3.6: Configurar appsettings.json y Program.cs en la API

Ahora vamos a conectar las capas en GelatoERP.Api registrando los servicios y definiendo la cadena de conexión a  
 PostgreSQL. Además, configuraremos Swagger para que nos permita enviar el header X-Tenant-Id en cada prueba de endpoints.
──────

#### 📄 Instrucción 1: Actualizar appsettings.json

1. Abrí el archivo src/GelatoERP.Api/appsettings.json.
2. Reemplazá su contenido con la configuración de la cadena de conexión a PostgreSQL:

   {
   "ConnectionStrings": {
   "DefaultConnection": "Host=localhost;Port=5432;Database=gelato_erp_db;Username=postgres;Password=postgres"
   },
   "Logging": {
   "LogLevel": {
   "Default": "Information",
   "Microsoft.AspNetCore": "Warning"
   }
   },
   "AllowedHosts": "\*"
   }

│ 💡 Nota: Si la contraseña o usuario de tu PostgreSQL local es diferente (por ejemplo, clave distinta de postgres),  
 │ ajustala según tu entorno.  
 ──────

#### 📄 Instrucción 2: Actualizar Program.cs

1.  Abrí el archivo src/GelatoERP.Api/Program.cs.
2.  Reemplazá todo su contenido por el siguiente código:

    using GelatoERP.Application;
    using GelatoERP.Infrastructure;
    using Microsoft.OpenApi.Models;

    var builder = WebApplication.CreateBuilder(args);

    // 1. Agregar servicios de las capas de Aplicación e Infraestructura
    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructureServices(builder.Configuration);

    // 2. Agregar controladores
    builder.Services.AddControllers();

    // 3. Configurar Swagger/OpenAPI con soporte para el Header X-Tenant-Id
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
    options.SwaggerDoc("v1", new OpenApiInfo
    {
    Title = "GelatoERP API",
    Version = "v1",
    Description = "API del ERP Multi-Tenant para Heladerías y Fábricas de Helado"
    });

        // Agregar Header X-Tenant-Id a la interfaz visual de Swagger
        options.AddSecurityDefinition("TenantId", new OpenApiSecurityScheme
        {
            Name = "X-Tenant-Id",
            Type = SecuritySchemeType.ApiKey,
            In = ParameterLocation.Header,
            Description = "ID del Tenant (Guid) para la heladería/sucursal actual"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "TenantId"
                    }
                },
                Array.Empty<string>()
            }
        });

    });

    var app = builder.Build();

    // 4. Configurar el pipeline HTTP
    if (app.Environment.IsDevelopment())
    {
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "GelatoERP API v1"));
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
    ──────

──────

### 🎯 PASO 3.7: Crear y Aplicar las Migraciones en Supabase

Entity Framework Core utiliza las Migraciones para transformar las entidades C# que definimos (Tenant, Plant, User, Role,
UserRole) en tablas reales SQL dentro de PostgreSQL en Supabase.  
 ──────

#### 📄 Instrucción 1: Generar la migración InitialCreate

Abrí la consola/terminal desde la raíz de tu proyecto (donde está el archivo GelatoErp.sln) y ejecutá el siguiente  
 comando:

    dotnet ef migrations add InitialCreate --project src/GelatoERP.Infrastructure --startup-project src/GelatoERP.Api

│ 💡 Nota: Si la consola te dice que no reconoce dotnet ef, primero ejecutá este comando para instalar la herramienta  
 │ global de EF Core:  
 │ dotnet tool install --global dotnet-ef

¿Qué hace este comando?  
 Crea automáticamente la carpeta Migrations dentro del proyecto GelatoERP.Infrastructure con el código C# que define la  
 estructura SQL inicial de tus tablas.  
 ──────

#### 📄 Instrucción 2: Aplicar la migración a la base de datos en Supabase

Una vez creada la migración, ejecutá el comando para impactar los cambios en la nube:

    dotnet ef database update --project src/GelatoERP.Infrastructure --startup-project src/GelatoERP.Api

¿Qué hace este comando?  
 Se conecta a Supabase usando la cadena de conexión configurada en tu appsettings.json y crea la estructura completa de la
base de datos (Tenants, Plants, Users, Roles, UserRoles).  
 ──────

### 🗺️ ¿Dónde estamos y qué sigue ahora?

Hasta acá hemos completado con éxito la Fase 1, 2 y 3 (Dominio, Infraestructura, EF Core, Multi-Tenant Query Filters,  
 Auditoría y Conexión en la Nube con Supabase).  
 ──────

### 🚀 FASE 4: Módulo de Gestión de Tenants (CQRS con MediatR)

Ahora vamos a empezar a programar la lógica de negocio de la aplicación en la capa GelatoERP.Application implementando el
patrón CQRS (Command Query Responsibility Segregation) con MediatR.

El primer caso de uso será: Crear un nuevo Tenant (Heladería / Empresa).  
 ──────

### 🎯 PASO 4.1: Crear el DTO y el Comando CreateTenantCommand

Para recibir los datos desde la API y crear un nuevo Tenant, crearemos la estructura CQRS para la entidad Tenant.

#### 🏛️ Conceptos clave:

1. Command (Comando): Es un objeto que representa una acción de escritura o modificación en el sistema (ej. Crear Tenant).
2. Handler (Manejador): Es la clase encargada de procesar la orden: recibe el comando, valida la información, interactúa
   con IApplicationDbContext y guarda en PostgreSQL.
3. Response DTO: El objeto liviano que devolvemos como respuesta a quien llamó al comando.  
   ──────

#### 📄 Instrucciones:

1.  En el proyecto GelatoERP.Application, crea la siguiente estructura de carpetas:  
    Tenants/Commands/CreateTenant
2.  Dentro de la carpeta CreateTenant, crea el archivo CreateTenantCommand.cs.
3.  Pega el siguiente código:

    using GelatoERP.Application.Common.Interfaces;
    using GelatoERP.Domain.Entities;
    using MediatR;

    namespace GelatoERP.Application.Tenants.Commands.CreateTenant;

    // 1. DTO de Respuesta
    public record TenantDto(
    Guid Id,
    string Name,
    string LegalName,
    string TaxId,
    string Plan,
    bool IsActive,
    DateTime CreatedAtUtc);

    // 2. El Comando (Request de MediatR)
    public record CreateTenantCommand(
    string Name,
    string LegalName,
    string TaxId,
    string? Plan) : IRequest<TenantDto>;

    // 3. El Manejador (Handler)
    public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, TenantDto>
    {
    private readonly IApplicationDbContext \_context;

        public CreateTenantCommandHandler(IApplicationDbContext context)
        {
        _context = context;

    }

        public async Task<TenantDto> Handle(CreateTenantCommand request, CancellationToken cancellationToken)

    {
    // Instanciar la entidad del Dominio
    var tenant = new Tenant(
    request.Name,
    request.LegalName,
    request.TaxId,
    request.Plan ?? "Standard");

            // Agregar al DbContext
            _context.Tenants.Add(tenant);

            // Guardar cambios en la base de datos (Supabase)
            await _context.SaveChangesAsync(cancellationToken);

            // Retornar el DTO de respuesta
            return new TenantDto(
                tenant.Id,
                tenant.Name,
                tenant.LegalName,
                tenant.TaxId,
                tenant.Plan,
                tenant.IsActive,
                tenant.CreatedAtUtc);
        }

    }

──────

### 📌 ESTADO Y RESUMEN PARA REINICIO DE SESIÓN

**Fases Completadas:**

- ✅ **Fase 1 (Dominio):** Entidades `Tenant`, `Plant`, `User`, `Role`, `UserRole` y clases base `BaseEntity`, `ITenantEntity`.
- ✅ **Fase 2 (Aplicación):** Interfaces `IApplicationDbContext`, `ICurrentTenantService`, configuración de MediatR y FluentValidation.
- ✅ **Fase 3 (Infraestructura y Nube):** `ApplicationDbContext` con Global Query Filters (Soft Delete y Multi-Tenant), auditoría automática, `CurrentTenantService`, y conexión a **Supabase PostgreSQL** mediante `appsettings.Development.json` (protegido en `.gitignore`).
- ✅ **Migración EF Core:** Se generó la migración `InitialCreate` y se ejecutó exitosamente la actualización de la base de datos en Supabase (`dotnet ef database update`).

**Paso en el que nos quedamos (Paso 4.1):**

- **Acción pendiente del usuario:** Crear el archivo `CreateTenantCommand.cs` en la ruta `src/GelatoERP.Application/Tenants/Commands/CreateTenant/CreateTenantCommand.cs` con el código del comando y handler de MediatR.
- **Próximo paso a realizar:** Crear el validador `CreateTenantCommandValidator.cs` con FluentValidation y el controlador `TenantsController.cs` en la API.

  ──────

  ### 🚀 PASO 4.2: Crear el Validador CreateTenantCommandValidator.cs con FluentValidation

  Para garantizar la integridad de los datos antes de ejecutar la lógica de negocio en el handler, implementamos  
  validaciones defensivas usando FluentValidation.

  #### 💡 ¿Qué reglas vamos a aplicar?
  1. Name: Requerido, no vacío, máximo 100 caracteres.
  2. TaxId: Requerido, no vacío (ej: CUIT/RUT/RFC), máximo 20 caracteres.
  3. DomainOrSlug: Requerido, no vacío, máximo 50 caracteres y solo debe permitir letras minúsculas, números y guiones  
     medios (ej: heladeria-don-luis).  
     ──────

  ### 📋 Instrucciones paso a paso:

  #### 1️⃣ Crear el archivo del Validador

  Dentro de la carpeta src/GelatoERP.Application/Tenants/Commands/CreateTenant/, crea el archivo:  
  📄 CreateTenantCommandValidator.cs

  #### 2️⃣ Pegar el siguiente código en CreateTenantCommandValidator.cs

  using FluentValidation;

  namespace GelatoERP.Application.Tenants.Commands.CreateTenant;

  /// <summary>  
   /// Reglas de validación para CreateTenantCommand utilizando FluentValidation.  
   /// </summary>  
   public class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>  
   {  
   public CreateTenantCommandValidator()  
   {  
   RuleFor(v => v.Name)  
   .NotEmpty().WithMessage("El nombre de la heladería/empresa es obligatorio.")  
   .MaximumLength(100).WithMessage("El nombre no debe superar los 100 caracteres.");

            RuleFor(v => v.TaxId)
                .NotEmpty().WithMessage("El identificador fiscal (CUIT/RUT) es obligatorio.")
                .MaximumLength(20).WithMessage("El identificador fiscal no debe superar los 20 caracteres.");

            RuleFor(v => v.DomainOrSlug)
                .NotEmpty().WithMessage("El subdominio o slug es obligatorio.")
                .MaximumLength(50).WithMessage("El subdominio o slug no debe superar los 50 caracteres.")
                .Matches("^[a-z0-9-]+$").WithMessage("El subdominio solo puede contener letras minúsculas, números y

  guiones medios (ej: heladeria-don-luis).");  
   }  
   }  
   ──────

  #### 3 Registrar FluentValidation en DependencyInjection.cs

  Para que los validadores se registren automáticamente en el contenedor de dependencias de la aplicación, debemos  
  actualizar DependencyInjection.cs.

  Abre src/GelatoERP.Application/DependencyInjection.cs y actualízalo para que quede así:

  using System.Reflection;  
   using FluentValidation;  
   using Microsoft.Extensions.DependencyInjection;

  namespace GelatoERP.Application;

  public static class DependencyInjection  
   {  
   public static IServiceCollection AddApplicationServices(this IServiceCollection services)  
   {  
   // 1. Registrar MediatR para manejar Commands y Queries (CQRS)  
   services.AddMediatR(cfg => {  
   cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());  
   });

            // 2. Registrar automáticamente todos los validadores de FluentValidation del ensamblado
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            return services;
        }

  }  
   ──────

### 🚀 PASO 4.3: Crear los Controladores en la API (ApiControllerBase y TenantsController)

Ahora vamos a exponer nuestro primer endpoint HTTP (POST /api/tenants) en la capa GelatoERP.Api.

Para mantener el código limpio y reusable, crearemos primero un controlador base (ApiControllerBase) que nos resolverá  
 automáticamente la instancia de ISender (MediatR) para todos los futuros controladores.  
 ──────

### 📋 Instrucciones paso a paso:

#### 1️⃣ Crear la carpeta Controllers

En la raíz del proyecto src/GelatoERP.Api, crea la carpeta:  
 📁 Controllers

Ruta completa: src/GelatoERP.Api/Controllers/  
 ──────

#### 2️⃣ Crear el controlador base ApiControllerBase.cs

Dentro de src/GelatoERP.Api/Controllers/, crea el archivo:  
 📄 ApiControllerBase.cs

Pega el siguiente código:

    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    namespace GelatoERP.Api.Controllers;

    [ApiController]
    [Route("api/[controller]")]
    public abstract class ApiControllerBase : ControllerBase
    {
        private ISender? _mediator;

        /// <summary>
        /// Propiedad protegida que resuelve el bus de MediatR mediante Inyección de Dependencias.
        /// </summary>
        protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();
    }
    ──────

#### 3️⃣ Crear el controlador TenantsController.cs

Dentro de src/GelatoERP.Api/Controllers/, crea el archivo:  
 📄 TenantsController.cs

Pega el siguiente código:

    using GelatoERP.Application.Tenants.Commands.CreateTenant;
    using Microsoft.AspNetCore.Mvc;

    namespace GelatoERP.Api.Controllers;

    public class TenantsController : ApiControllerBase
    {
        /// <summary>
        /// Registrar un nuevo Tenant (Heladería / Empresa) en la plataforma ERP.
        /// </summary>
        /// <param name="command">Datos del Tenant a crear</param>
        /// <returns>Tenant creado con su ID generado</returns>
        [HttpPost]
        [ProducesResponseType(typeof(TenantDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TenantDto>> Create(CreateTenantCommand command)
        {
            var result = await Mediator.Send(command);
            return CreatedAtAction(nameof(Create), new { id = result.Id }, result);
        }
    }
    ──────

### 📌 ESTADO Y RESUMEN DE AVANCE DEL PROYECTO

**Fases Completadas:**

- ✅ **Fase 1 (Dominio):** Entidades `Tenant`, `Plant`, `User`, `Role`, `UserRole` y clases base `BaseEntity`, `ITenantEntity`.
- ✅ **Fase 2 (Aplicación):** Interfaces `IApplicationDbContext`, `ICurrentTenantService`, configuración de MediatR y FluentValidation.
- ✅ **Fase 3 (Infraestructura y Nube):** `ApplicationDbContext` con Global Query Filters (Soft Delete y Multi-Tenant), auditoría automática, `CurrentTenantService`, y conexión a **Supabase PostgreSQL** mediante `appsettings.Development.json`.
- ✅ **Migración EF Core:** Migración `InitialCreate` ejecutada exitosamente en la base de datos de Supabase (`dotnet ef database update`).
- ✅ **Fase 4 (Módulo Tenants - Creación):**
  - Comando y Handler MediatR: `CreateTenantCommand.cs`
  - Validador FluentValidation: `CreateTenantCommandValidator.cs`
  - Controladores API: `ApiControllerBase.cs` y `TenantsController.cs` (`POST /api/tenants` probado y respondiendo `201 Created`).

**Próximas Fases a Desarrollar:**
- 🟢 **Fase 5 (Consultas / Queries y Middlewares):**
  - Implementar Pipeline Behavior para la ejecución automática de validaciones FluentValidation en MediatR.
  - Implementar `GetTenantsQuery` (`GET /api/tenants`) y `GetTenantByIdQuery` (`GET /api/tenants/{id}`).
  - Middleware global de excepciones.

