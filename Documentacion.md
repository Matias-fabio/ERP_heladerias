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

