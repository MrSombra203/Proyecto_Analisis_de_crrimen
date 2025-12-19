# Sistema de Análisis de Crímenes

Sistema web desarrollado en ASP.NET Core MVC para el registro, análisis y comparación de escenas de crímenes, con capacidades avanzadas de detección de patrones y identificación de crímenes en serie.

##  Tabla de Contenidos

- [Descripción General](#descripción-general)
- [Características Principales](#características-principales)
- [Core del Sistema](#core-del-sistema)
- [Arquitectura](#arquitectura)
- [Tecnologías Utilizadas](#tecnologías-utilizadas)
- [Requisitos Previos](#requisitos-previos)
- [Instalación y Configuración](#instalación-y-configuración)
- [Estructura del Proyecto](#estructura-del-proyecto)
- [Modelo de Datos](#modelo-de-datos)
- [Funcionalidades](#funcionalidades)
- [Algoritmo de Comparación](#algoritmo-de-comparación)
- [Sistema de Autenticación](#sistema-de-autenticación)
- [Interfaz de Usuario y Diseño](#interfaz-de-usuario-y-diseño)
- [Configuración de Base de Datos](#configuración-de-base-de-datos)
- [Uso del Sistema](#uso-del-sistema)
- [Mejoras Técnicas Implementadas](#mejoras-técnicas-implementadas)

---

##  Descripción General

Este sistema permite a las autoridades y analistas de crímenes registrar escenas delictivas de manera estructurada, comparar casos similares y detectar automáticamente patrones que puedan indicar crímenes en serie. El sistema utiliza un algoritmo de comparación avanzado que analiza múltiples factores para determinar el grado de similitud entre diferentes escenas de crímenes.

---

##  Características Principales

- **Registro de Escenas de Crimen**: Captura detallada de información sobre crímenes incluyendo ubicación, fecha, tipo, modus operandi, evidencias y características adicionales
- **Sistema de Comparación Inteligente**: Algoritmo optimizado que calcula el porcentaje de similitud entre escenas basado en múltiples criterios con validaciones de integridad
- **Detección de Crímenes en Serie**: Identificación automática de patrones que sugieren crímenes relacionados
- **Autenticación Obligatoria**: Todas las funcionalidades principales requieren inicio de sesión para garantizar seguridad
- **Validación de Comparaciones**: Sistema que previene la comparación de una escena consigo misma (client-side y server-side)
- **Interfaz Unificada**: Esquema de colores consistente en toda la aplicación para una experiencia de usuario coherente
- **Gestión de Catálogos**: Administración de tipos de crimen y modus operandi
- **Sistema de Usuarios y Roles**: Control de acceso basado en roles (Administrador, Usuario)
- **Dashboard Analítico**: Panel de control para administradores con estadísticas y detección de series
- **Búsqueda de Escenas Similares**: Encuentra automáticamente casos relacionados con una escena base
- **Código Documentado**: Comentarios concisos en todo el código para facilitar el mantenimiento y comprensión

---

##  Core del Sistema

### ComparacionService - El Corazón del Sistema

El **`ComparacionService`** es el componente central del proyecto. Este servicio implementa la lógica de análisis criminal mediante un algoritmo de comparación multi-criterio optimizado y mejorado que evalúa:

1. **Tipo de Crimen** (25 puntos): Compara si ambas escenas pertenecen al mismo tipo de delito
2. **Modus Operandi** (25 puntos): Analiza si el método utilizado es compatible
3. **Área Geográfica** (20 puntos): Evalúa si ocurrieron en la misma zona geográfica
4. **Evidencias Físicas** (15 puntos): Calcula la similitud basada en tipos de evidencias encontradas usando el coeficiente de Jaccard optimizado con `HashSet`
5. **Horario del Crimen** (10 puntos): Compara el momento del día en que ocurrieron
6. **Características Adicionales** (5 puntos): Evalúa uso de violencia, planificación, múltiples perpetradores considerando coincidencias tanto en valores `true` como `false`

#### Mejoras Implementadas

- **Constantes Definidas**: Los pesos de criterios y umbrales están definidos como constantes para facilitar ajustes futuros
- **Validaciones Robustas**: Validación de parámetros nulos y verificación de escenas idénticas
- **Optimización de Evidencias**: Uso de `HashSet` para comparación de evidencias con complejidad O(1) en lugar de O(n)
- **Cálculo Mejorado de Características**: Evalúa coincidencias en ambos estados (true/false) para mayor precisión
- **Umbrales Configurables**: Constantes para umbrales de clasificación (75% serie, 60% conexión probable)

El algoritmo genera un **porcentaje de similitud** y clasifica los resultados en tres categorías:

- **CrimenEnSerie** (≥75%): Alta probabilidad de conexión
- **ConexionProbable** (≥60%): Posible relación entre casos
- **SimilitudBaja** (<60%): Baja probabilidad de conexión

### Funcionalidades Core

#### 1. Comparación de Escenas (`CompararEscenas`)
```csharp
public ComparacionResultado CompararEscenas(EscenaCrimen escenaBase, EscenaCrimen escenaComparada)
```
Compara dos escenas específicas y retorna un resultado detallado con porcentaje de similitud y lista de coincidencias. Incluye validación de escenas idénticas y manejo de casos especiales.

#### 2. Comparación de Evidencias (`CompararEvidencias`)
```csharp
private double CompararEvidencias(ICollection<Evidencia> evidencias1, ICollection<Evidencia> evidencias2)
```
Método privado optimizado que utiliza el **coeficiente de Jaccard** y `HashSet` para calcular similitud de evidencias con complejidad O(n) en lugar de O(n²).

#### 3. Cálculo de Similitud de Características (`CalcularSimilitudCaracteristicas`)
```csharp
private double CalcularSimilitudCaracteristicas(EscenaCrimen escenaBase, EscenaCrimen escenaComparada)
```
Evalúa coincidencias en características especiales (violencia, planificación, múltiples perpetradores) considerando ambos valores booleanos (true y false).

#### 4. Búsqueda de Escenas Similares (`BuscarEscenasSimilares`)
```csharp
public List<ComparacionResultado> BuscarEscenasSimilares(EscenaCrimen escenaBase, List<EscenaCrimen> todasLasEscenas, double umbralMinimo = 60)
```
Busca todas las escenas similares a una escena base, filtrando por un umbral mínimo de similitud (por defecto 60%). Retorna resultados ordenados por similitud descendente.

#### 5. Detección de Crímenes en Serie (`DetectarCrimenesEnSerie`)
```csharp
public List<List<EscenaCrimen>> DetectarCrimenesEnSerie(List<EscenaCrimen> todasLasEscenas)
```
Identifica grupos de crímenes que forman series, utilizando un umbral de 75% y mínimo de 3 escenas para considerar crímenes en serie. Evita duplicados usando `HashSet`.

### Servicios del Sistema

El proyecto implementa una arquitectura en capas con servicios especializados que aplican el principio de responsabilidad única (SRP):

#### 1. ComparacionService
- **Responsabilidad**: Algoritmo core de comparación de escenas de crímenes
- **Interfaz**: `IComparacionService`
- **Métodos principales**: `CompararEscenas`, `BuscarEscenasSimilares`, `DetectarCrimenesEnSerie`
- **Dependencias**: Ninguna (servicio puro de lógica)

#### 2. EscenaCrimenService
- **Responsabilidad**: Gestión de escenas de crímenes (CRUD y validaciones)
- **Interfaz**: `IEscenaCrimenService`
- **Métodos principales**: `ObtenerTodasAsync`, `ObtenerPorIdAsync`, `RegistrarEscenaAsync`, `ValidarEscenaAsync`
- **Dependencias**: `IUnitOfWork`

#### 3. CatalogoService
- **Responsabilidad**: Gestión de catálogos (TiposCrimen y ModusOperandi)
- **Interfaz**: `ICatalogoService`
- **Métodos principales**: CRUD para ambos catálogos, validación de existencia, cambio de estado
- **Dependencias**: `IUnitOfWork`

#### 4. UsuarioService
- **Responsabilidad**: Gestión de usuarios y roles
- **Interfaz**: `IUsuarioService`
- **Métodos principales**: CRUD de usuarios, validación de usuarios, gestión de roles
- **Dependencias**: `IUnitOfWork`, `IAuthenticationService`, `ApplicationDbContext` (para Include con Rol)

#### 5. AuthenticationService
- **Responsabilidad**: Autenticación y validación de credenciales
- **Interfaz**: `IAuthenticationService`
- **Métodos principales**: `AuthenticateAsync`, `UsuarioExisteAsync`, `EmailExisteAsync`
- **Dependencias**: `IUnitOfWork`, `ApplicationDbContext` (para Include con Rol)

**Nota**: Todos los servicios están registrados en `Program.cs` con inyección de dependencias (AddScoped) y siguen el principio de inversión de dependencias (DIP) utilizando interfaces.

---

##  Arquitectura

El proyecto sigue el patrón **MVC (Model-View-Controller)** con una arquitectura en capas mejorada que implementa **Repository Pattern** y **Unit of Work Pattern** para una mejor separación de responsabilidades y mantenibilidad:

```
┌─────────────────────────────────────┐
│         Controllers Layer            │
│  (EscenaCrimen, Auth, Catalogos)    │
│  DIP: Depende de interfaces         │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│         Services Layer               │
│  (ComparacionService - CORE)        │
│  (EscenaCrimenService)               │
│  (AuthenticationService)             │
│  (CatalogoService, UsuarioService)   │
│  SRP: Lógica de negocio separada    │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│      Repository Layer                │
│  (Repository Pattern)                │
│  IRepository<T>, Repository<T>       │
│  IEscenaCrimenRepository             │
│  IUnitOfWork, UnitOfWork             │
│  ISP, OCP: Extensible y flexible    │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│         Models Layer                 │
│  (Entities, DbContext)              │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│         Data Access Layer            │
│  (Entity Framework Core + SQL Server)│
└──────────────────────────────────────┘
```

### Patrones Arquitectónicos Implementados

#### Repository Pattern

El proyecto implementa el **Repository Pattern** para abstraer el acceso a datos y aplicar principios SOLID:

- **`IRepository<T>`**: Interfaz genérica que define operaciones CRUD básicas para cualquier entidad
  - Métodos de consulta: `GetByIdAsync`, `GetAllAsync`, `FindAsync`, `FirstOrDefaultAsync`, `AnyAsync`, `CountAsync`
  - Métodos de modificación: `AddAsync`, `AddRangeAsync`, `Update`, `Remove`, `RemoveRange`
  - Aplica **ISP (Interface Segregation Principle)** y **DIP (Dependency Inversion Principle)**

- **`Repository<T>`**: Implementación genérica base que encapsula la lógica común de acceso a datos
  - Usa `DbContext.Set<T>()` para operaciones genéricas
  - Aplica **SRP (Single Responsibility Principle)**: Responsabilidad única de acceso a datos

- **`IEscenaCrimenRepository`**: Interfaz específica que extiende `IRepository<EscenaCrimen>` con operaciones especializadas
  - Métodos optimizados: `GetByIdWithRelationsAsync`, `GetAllWithRelationsAsync`, `GetByTipoCrimenAsync`, `GetByAreaGeograficaAsync`, `GetByModusOperandiAsync`, `GetUltimasEscenasAsync`
  - Aplica **OCP (Open/Closed Principle)**: Extensible sin modificar código base

- **`EscenaCrimenRepository`**: Implementación específica con operaciones optimizadas usando `Include()` para eager loading
  - Evita consultas N+1 cargando relaciones en una sola consulta
  - Ordena resultados por fecha de registro (más recientes primero)

#### Unit of Work Pattern

El proyecto implementa **Unit of Work Pattern** para coordinar múltiples repositorios y manejar transacciones:

- **`IUnitOfWork`**: Interfaz que expone todos los repositorios y métodos de transacción
  - Propiedades: `EscenasCrimen`, `Evidencias`, `TiposCrimen`, `ModusOperandi`, `Usuarios`, `Roles`
  - Métodos: `SaveChangesAsync`, `BeginTransactionAsync`, `CommitTransactionAsync`, `RollbackTransactionAsync`
  - Aplica **DIP (Dependency Inversion Principle)**

- **`UnitOfWork`**: Implementación que coordina todos los repositorios usando lazy initialization
  - Cada repositorio se instancia solo cuando se accede por primera vez (patrón Lazy)
  - Maneja transacciones de base de datos para operaciones atómicas
  - Implementa `IDisposable` para liberar recursos adecuadamente

**Beneficios de estos patrones:**
- **Desacoplamiento**: Los servicios no dependen directamente de Entity Framework
- **Testabilidad**: Fácil crear mocks de repositorios para pruebas unitarias
- **Mantenibilidad**: Cambios en acceso a datos se aíslan en la capa de repositorios
- **Transaccionalidad**: Unit of Work permite operaciones atómicas entre múltiples repositorios
- **Reutilización**: Código genérico reduce duplicación

### Componentes Principales

- **Controllers**: Manejan las peticiones HTTP y coordinan entre vistas y servicios. Aplican **DIP** dependiendo de interfaces de servicios y `IUnitOfWork`.
- **Services**: Contienen la lógica de negocio:
  - `ComparacionService`: Algoritmo core de comparación de escenas
  - `EscenaCrimenService`: Gestión de escenas de crímenes
  - `CatalogoService`: Gestión de catálogos (TiposCrimen, ModusOperandi)
  - `UsuarioService`: Gestión de usuarios y roles
  - `AuthenticationService`: Autenticación y validación de credenciales
  - Todos usan `IUnitOfWork` para acceso a datos y aplican **SRP**.
- **Repositories**: Abstraen el acceso a datos usando Repository Pattern y Unit of Work Pattern:
  - `IRepository<T>` / `Repository<T>`: Repositorio genérico para cualquier entidad
  - `IEscenaCrimenRepository` / `EscenaCrimenRepository`: Repositorio específico con operaciones optimizadas
  - `IUnitOfWork` / `UnitOfWork`: Coordina múltiples repositorios y transacciones
  - Todos aplican principios **SOLID** (SRP, DIP, ISP, OCP).
- **Models**: Entidades del dominio (EscenaCrimen, Evidencia, TipoCrimen, etc.) y contexto de base de datos (ApplicationDbContext)
- **Attributes**: Filtros personalizados para autorización (`RequireAdmin`, `RequireAuth`)
- **Views**: Interfaces de usuario en Razor Pages

---

##  Tecnologías Utilizadas

- **.NET 8.0**: Framework principal
- **ASP.NET Core MVC**: Framework web
- **Entity Framework Core 8.0.11**: ORM para acceso a datos
- **SQL Server**: Base de datos relacional
- **Bootstrap 5**: Framework CSS para UI
- **jQuery**: Librería JavaScript
- **Razor Pages**: Motor de vistas

---

##  Requisitos Previos

- **.NET 8.0 SDK** o superior
- **SQL Server** (LocalDB, Express, o versión completa)
- **Visual Studio 2022** o **Visual Studio Code** (recomendado)
- **SQL Server Management Studio (SSMS)** (opcional, para gestión de BD)

---

##  Instalación y Configuración

### 1. Clonar el Repositorio

```bash
git clone <url-del-repositorio>
cd Proyecto_Analisis_de_crrimen
```

### 2. Configurar la Cadena de Conexión

Edita el archivo `appsettings.json` y configura la cadena de conexión a tu instancia de SQL Server:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=CrimenDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

**Nota**: Ajusta `Server=.` según tu configuración:
- LocalDB: `Server=(localdb)\\mssqllocaldb`
- SQL Server Express: `Server=.\\SQLEXPRESS`
- SQL Server: `Server=nombre-servidor`

### 3. Restaurar Dependencias

```bash
dotnet restore
```

### 4. Crear la Base de Datos

El sistema creará automáticamente la base de datos al iniciar si no existe. Alternativamente, puedes ejecutar migraciones:

```bash
dotnet ef database update
```

### 5. Ejecutar la Aplicación

```bash
dotnet run
```

O desde Visual Studio, presiona `F5`.

La aplicación estará disponible en:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`

### 6. Usuarios Iniciales

El sistema requiere que crees usuarios manualmente en la base de datos o a través de la interfaz de administración. Los roles disponibles son:

- **Administrador** (RolId = 1): Acceso completo al sistema
- **Usuario** (RolId = 2): Acceso limitado a registro y consulta de escenas

---

##  Estructura del Proyecto

```
Proyecto_Analisis_de_crrimen/
│
├── Controllers/              # Controladores MVC
│   ├── EscenaCrimenController.cs    # Gestión de escenas (CORE)
│   ├── AuthController.cs            # Autenticación
│   ├── CatalogosController.cs       # Gestión de catálogos
│   ├── UsuariosController.cs        # Gestión de usuarios
│   └── HomeController.cs            # Página principal
│
├── Models/                   # Modelos de datos
│   ├── EscenaCrimen.cs              # Entidad principal
│   ├── Evidencia.cs                 # Evidencias físicas
│   ├── TipoCrimen.cs                # Catálogo de tipos
│   ├── ModusOperandi.cs             # Catálogo de modus operandi
│   ├── Usuario.cs                   # Usuarios del sistema
│   ├── Rol.cs                       # Roles de usuario
│   ├── ComparacionResultado.cs      # Resultados de comparación
│   └── ApplicationDbContext.cs      # Contexto de EF Core
│
├── Services/                 # Servicios de negocio (capa de lógica)
│   ├── ComparacionService.cs        # CORE: Algoritmo de comparación
│   ├── EscenaCrimenService.cs       # Gestión de escenas de crimen
│   ├── CatalogoService.cs           # Gestión de catálogos (TiposCrimen, ModusOperandi)
│   ├── UsuarioService.cs            # Gestión de usuarios
│   ├── AuthenticationService.cs     # Servicio de autenticación
│   └── Interfaces/                  # Interfaces de servicios (IComparacionService, etc.)
│
├── Repositories/             # Repositorios (capa de acceso a datos)
│   ├── IRepository.cs               # Interfaz genérica del repositorio
│   ├── Repository.cs                # Implementación genérica del repositorio
│   ├── IEscenaCrimenRepository.cs   # Interfaz específica para EscenaCrimen
│   ├── EscenaCrimenRepository.cs    # Repositorio específico con operaciones optimizadas
│   ├── IUnitOfWork.cs               # Interfaz Unit of Work Pattern
│   └── UnitOfWork.cs                # Implementación Unit of Work Pattern
│
├── Attributes/               # Atributos personalizados
│   └── AuthorizeAttribute.cs        # Filtros de autorización ([RequireAuth], [RequireAdmin])
│
├── Views/                    # Vistas Razor
│   ├── EscenaCrimen/                # Vistas de escenas
│   ├── Auth/                        # Vistas de autenticación
│   ├── Catalogos/                   # Vistas de catálogos
│   ├── Usuarios/                    # Vistas de usuarios
│   └── Shared/                      # Layouts compartidos
│
├── wwwroot/                  # Archivos estáticos
│   ├── css/
│   ├── js/
│   └── lib/                         # Librerías (Bootstrap, jQuery)
│
├── Program.cs                # Punto de entrada y configuración
├── appsettings.json          # Configuración de la aplicación
└── Proyecto_Analisis_de_crrimen.csproj  # Archivo de proyecto
```

---

##  Modelo de Datos

### Entidades Principales

#### EscenaCrimen
Entidad central del sistema que representa un crimen registrado.

**Propiedades Clave:**
- `Id`: Identificador único
- `FechaCrimen`: Fecha en que ocurrió el crimen
- `Ubicacion`: Ubicación geográfica (máx. 200 caracteres)
- `TipoCrimenId`: Referencia al tipo de crimen
- `ModusOperandiId`: Referencia al modus operandi
- `AreaGeografica`: Enum (Centro, Norte, Sur, Este, Oeste)
- `HorarioCrimen`: Enum (Madrugada, Mañana, Tarde, Noche)
- `DescripcionDetallada`: Descripción extensa (máx. 2000 caracteres)
- `UsoViolencia`: Boolean
- `ActoPlanificado`: Boolean
- `MultiplesPerpetadores`: Boolean
- `AutoriaDesconocida`: Boolean
- `Evidencias`: Colección de evidencias relacionadas

#### Evidencia
Representa evidencia física encontrada en la escena.

**Tipos de Evidencia:**
- VidrioRoto
- HuellasDactilares
- Sangre
- Cabellos
- Fibras
- ArmaFuego

#### TipoCrimen
Catálogo de tipos de crímenes (ej: Robo, Homicidio, Asalto).

#### ModusOperandi
Catálogo de métodos operativos (ej: Forzamiento de entrada, Amenaza con arma).

#### Usuario
Usuarios del sistema con autenticación basada en sesión.

#### Rol
Roles del sistema (Administrador, Usuario).

### Relaciones

```
EscenaCrimen (1) ──→ (N) Evidencia
EscenaCrimen (N) ──→ (1) TipoCrimen
EscenaCrimen (N) ──→ (1) ModusOperandi
Usuario (N) ──→ (1) Rol
```

---

##  Funcionalidades

### ⚠️ Autenticación Requerida

**Todas las funcionalidades principales requieren inicio de sesión.** El sistema implementa atributos `[RequireAuth]` en todas las acciones sensibles para garantizar que solo usuarios autenticados puedan acceder.

- Los usuarios no autenticados son redirigidos automáticamente al login
- Después del login, se redirige al usuario a la página original que intentaba acceder
- La página principal (`Home/Index`) muestra contenido dinámico según el estado de autenticación

### Para Todos los Usuarios Autenticados

1. **Registrar Escena de Crimen** `[RequireAuth]`
   - Formulario completo con validaciones front-end y back-end
   - Selección de tipo de crimen y modus operandi (solo activos)
   - Selección múltiple de evidencias
   - Campos adicionales (violencia, planificación, etc.)
   - Validación de integridad de datos y catálogos

2. **Ver Lista de Escenas** `[RequireAuth]`
   - Listado ordenado por fecha de registro (más recientes primero)
   - Información resumida de cada escena con relaciones cargadas
   - Visualización correcta de nombres de TipoCrimen y ModusOperandi
   - Enlaces a "Ver Similares" para cada escena

3. **Comparar Escenas** `[RequireAuth]`
   - Selección de dos escenas para comparar mediante dropdowns
   - **Validación client-side y server-side** para prevenir selección de la misma escena en ambos campos
   - JavaScript que deshabilita opciones ya seleccionadas en el otro dropdown
   - Visualización de resultados detallados con formato mejorado
   - Porcentaje de similitud y lista de coincidencias
   - Mostrado correcto de nombres de TipoCrimen y ModusOperandi

4. **Buscar Escenas Similares** `[RequireAuth]`
   - A partir de una escena base, encuentra todas las similares (≥60% similitud)
   - Ordenadas por porcentaje de similitud descendente
   - Visualización clara de resultados con información completa

### Solo para Administradores

1. **Dashboard Analítico** `[RequireAdmin]`
   - Estadísticas generales del sistema
   - Total de escenas registradas
   - Número de crímenes en serie detectados
   - Últimas 5 escenas registradas con información completa
   - Visualización correcta de nombres de TipoCrimen

2. **Gestión de Catálogos** `[RequireAdmin]`
   - CRUD completo de Tipos de Crimen
   - CRUD completo de Modus Operandi
   - Activación/Desactivación de registros
   - Validación de nombres únicos
   - Los registros desactivados no aparecen en formularios de registro

3. **Gestión de Usuarios** `[RequireAdmin]`
   - Crear, editar y desactivar usuarios
   - Asignación de roles
   - Validación de emails y nombres de usuario únicos
   - Validación de formato de email con expresiones regulares
   - Validación de longitud mínima de contraseñas

---

##  Algoritmo de Comparación

El algoritmo de comparación es el **núcleo del sistema**. Evalúa múltiples factores para determinar la similitud entre escenas:

### Fórmula de Cálculo

```
Puntos Totales = Σ(Puntos por Criterio)
Porcentaje de Similitud = (Puntos Totales / Puntos Máximos) × 100
```

### Criterios de Evaluación

| Criterio | Puntos Máximos | Descripción |
|----------|----------------|-------------|
| Tipo de Crimen | 25 | Coincidencia exacta del tipo |
| Área Geográfica | 20 | Misma zona geográfica |
| Modus Operandi | 25 | Mismo método operativo |
| Horario | 10 | Mismo rango horario |
| Evidencias | 15 | Similitud basada en tipos de evidencias |
| Características | 5 | Uso de violencia, planificación, múltiples perpetradores |

**Total de Puntos Máximos: 100**

### Cálculo de Similitud de Evidencias

La similitud de evidencias se calcula usando el **coeficiente de Jaccard** optimizado:

```
Similitud = (Evidencias Comunes) / (Evidencias Totales Únicas)
```

**Optimización Implementada:**
- Uso de `HashSet<TipoEvidencia>` para almacenar tipos de evidencias de cada escena
- Operaciones de intersección y unión optimizadas con complejidad O(n) en lugar de O(n²)
- Manejo de casos especiales: ambas vacías (100% similitud), una vacía (0% similitud)

### Cálculo de Similitud de Características

Las características especiales (violencia, planificación, múltiples perpetradores) se evalúan considerando coincidencias en ambos estados:

- Coincidencia cuando ambas escenas tienen `true` en una característica
- Coincidencia cuando ambas escenas tienen `false` en una característica
- No hay coincidencia cuando los valores difieren

```
Puntos = (Coincidencias / Total de Características) × 5 puntos máximos
```

### Clasificación de Resultados

- **≥ 75%**: `CrimenEnSerie` - Alta probabilidad de conexión
- **≥ 60%**: `ConexionProbable` - Posible relación
- **< 60%**: `SimilitudBaja` - Baja probabilidad

### Ejemplo de Comparación

```
Escena A: Robo, Centro, Forzamiento, Noche, [Huellas, Sangre]
Escena B: Robo, Centro, Forzamiento, Noche, [Huellas, Vidrio]

Cálculo:
- Tipo de Crimen: ✓ (25 puntos)
- Área Geográfica: ✓ (20 puntos)
- Modus Operandi: ✓ (25 puntos)
- Horario: ✓ (10 puntos)
- Evidencias: 1 común / 3 totales = 0.33 × 15 = 5 puntos
- Características: 0 puntos

Total: 85 puntos / 100 = 85%
Clasificación: CrimenEnSerie
```

---

##  Sistema de Autenticación

### Autenticación Basada en Sesión

El sistema utiliza autenticación personalizada basada en sesiones HTTP:

- **Almacenamiento**: Información del usuario en `HttpContext.Session`
- **Datos de Sesión**:
  - `UserId`: ID del usuario
  - `NombreUsuario`: Nombre de usuario
  - `NombreCompleto`: Nombre completo
  - `RolId`: ID del rol
  - `RolNombre`: Nombre del rol

### Autorización

El sistema implementa dos atributos personalizados:

1. **`[RequireAuth]`**: Requiere que el usuario esté autenticado
   - Verifica `UserId` en la sesión
   - Si no está autenticado, redirige al login guardando la URL original (`returnUrl`)
   - Después del login exitoso, redirige al usuario a la página que intentaba acceder
   - Implementado en todas las acciones de `EscenaCrimenController` (excepto Dashboard)

2. **`[RequireAdmin]`**: Requiere rol de Administrador (RolId = 1)
   - Verifica `RolId == 1` en la sesión
   - Si no es administrador, redirige a la página de acceso denegado
   - Usado en: Dashboard, gestión de catálogos, gestión de usuarios

### Flujo de Autenticación

```
Usuario intenta acceder a página protegida
         ↓
    [RequireAuth] verifica sesión
         ↓
    Si no autenticado → Redirige a /Auth/Login?returnUrl=<url-original>
         ↓
    Usuario ingresa credenciales
         ↓
    AuthenticationService.AuthenticateAsync()
         ↓
    Validación de credenciales y estado activo
         ↓
    Creación de sesión con datos del usuario
         ↓
    Redirección a returnUrl o según rol:
    - Administrador → Dashboard (si no hay returnUrl)
    - Usuario → Lista de Escenas (si no hay returnUrl)
```

### Contenido Dinámico

La página principal (`Home/Index`) muestra contenido diferente según el estado de autenticación:

- **No autenticado**: Botón "Iniciar Sesión" y tarjetas informativas sin funcionalidad
- **Autenticado**: Botones funcionales para "Registrar Escena", "Comparar", etc.
- **Administrador**: Además muestra botón "Ver Dashboard"

**Nota de Seguridad**: El sistema actualmente almacena contraseñas en texto plano. Se recomienda encarecidamente implementar hashing (bcrypt, PBKDF2) antes de usar en producción.

---

##  Configuración de Base de Datos

### Esquema de Tablas

El sistema crea automáticamente las siguientes tablas:

- **Roles**: Catálogo de roles del sistema
- **Usuarios**: Usuarios con autenticación
- **TiposCrimen**: Catálogo de tipos de crímenes
- **ModusOperandi**: Catálogo de modus operandi
- **EscenasCrimen**: Escenas de crímenes registradas
- **Evidencias**: Evidencias físicas relacionadas con escenas

### Índices Optimizados

El `ApplicationDbContext` configura índices para optimizar consultas:

- Índice único en `Usuarios.NombreUsuario` y `Usuarios.Email`
- Índice compuesto en `EscenasCrimen` para búsquedas de comparación:
  ```sql
  IX_EscenasCrimen_Comparacion (TipoCrimenId, AreaGeografica, ModusOperandiId, HorarioCrimen)
  ```
- Índices en campos frecuentemente consultados (FechaCrimen, FechaRegistro, etc.)

### Configuración de Relaciones

- **Restrict Delete**: En relaciones con catálogos (TipoCrimen, ModusOperandi, Rol)
- **Cascade Delete**: En Evidencias cuando se elimina una EscenaCrimen

---

##  Interfaz de Usuario y Diseño

### Esquema de Colores Unificado

El sistema implementa un **esquema de colores consistente** en toda la aplicación mediante variables CSS:

- **Color Primario**: `#1a3a52` (Azul oscuro) - Usado en headers, navbar, botones principales
- **Color Secundario**: `#2c5aa0` (Azul) - Usado en hovers, bordes, enlaces
- **Color Accent**: `#2c5aa0` - Énfasis y elementos destacados

**Variables CSS Definidas** (`wwwroot/css/site.css`):
```css
--color-primary: #1a3a52
--color-secondary: #2c5aa0
--color-accent: #2c5aa0
--color-text: #333
--color-success: #28a745
--color-danger: #dc3545
--color-warning: #ffc107
--color-info: #17a2b8
```

### Aplicación del Diseño

- **Navbar**: Fondo color primario con borde secundario
- **Botones**: Color primario con hover en secundario
- **Formularios**: Focus en color secundario
- **Tablas**: Headers en color primario
- **Alertas**: Colores semánticos consistentes
- **Tarjetas**: Headers en color primario con borde secundario

### Experiencia de Usuario

- **Navegación Intuitiva**: Menú contextual que muestra opciones según rol
- **Feedback Visual**: Mensajes de éxito/error mediante `TempData`
- **Validación en Tiempo Real**: JavaScript para prevenir errores antes del envío
- **Responsive Design**: Adaptación a diferentes tamaños de pantalla con Bootstrap 5
- **Transiciones Suaves**: Efectos hover y transiciones CSS para mejor interactividad

---

##  Uso del Sistema

### 1. Iniciar Sesión (Obligatorio)

**Antes de acceder a cualquier funcionalidad, debes iniciar sesión.**

1. Accede a `/Auth/Login` o haz clic en "Iniciar Sesión" desde la página principal
2. Ingresa tu nombre de usuario y contraseña
3. Si intentabas acceder a una página específica, serás redirigido automáticamente después del login
4. Si inicias sesión directamente, serás redirigido según tu rol:
   - **Administrador** → Dashboard
   - **Usuario** → Lista de Escenas

### 2. Registrar una Escena de Crimen

1. Navega a **Escenas de Crimen** → **Registrar**
2. Completa el formulario:
   - Fecha del crimen
   - Ubicación geográfica
   - Selecciona Tipo de Crimen
   - Selecciona Área Geográfica
   - Selecciona Modus Operandi
   - Selecciona Horario
   - Marca las evidencias encontradas
   - Completa características adicionales
   - (Opcional) Agrega descripción detallada
3. Haz clic en **Registrar**

### 3. Comparar Escenas

1. Ve a **Escenas de Crimen** → **Comparar** (requiere autenticación)
2. Selecciona dos escenas diferentes de los dropdowns
   - **Validación automática**: El sistema previene seleccionar la misma escena en ambos campos
   - Las opciones ya seleccionadas se deshabilitan automáticamente en el otro dropdown
   - Si intentas seleccionar la misma escena, aparecerá una alerta y se limpiará la selección
3. Haz clic en **Comparar**
4. Revisa el resultado detallado:
   - Porcentaje de similitud (0-100%)
   - Información completa de ambas escenas (Tipo de Crimen, Modus Operandi mostrados correctamente)
   - Lista de coincidencias detectadas
   - Clasificación automática:
     - **Crimen en Serie**: ≥75% similitud
     - **Conexión Probable**: 60-74% similitud
     - **Similitud Baja**: <60% similitud

**Nota**: El sistema valida tanto en el cliente (JavaScript) como en el servidor que las dos escenas sean diferentes.

### 4. Buscar Escenas Similares

1. Desde la lista de escenas, haz clic en **Ver Similares**
2. El sistema mostrará todas las escenas con similitud ≥ 60%
3. Resultados ordenados por porcentaje descendente

### 5. Dashboard (Solo Administradores)

1. Accede al **Dashboard** desde el menú
2. Visualiza:
   - Total de escenas registradas
   - Número de crímenes en serie detectados
   - Últimas escenas registradas

### 6. Gestionar Catálogos (Solo Administradores)

1. Ve a **Catálogos** → **Tipos de Crimen** o **Modus Operandi**
2. Crea, edita o desactiva registros según sea necesario
3. Los registros desactivados no aparecerán en los formularios de registro

---

##  Mejoras Técnicas Implementadas

### Optimizaciones del Algoritmo Core

1. **Constantes para Configuración**
   - Pesos de criterios definidos como constantes (`PESO_TIPO_CRIMEN`, `PESO_MODUS_OPERANDI`, etc.)
   - Umbrales de clasificación configurables (`UMBRAL_CRIMEN_EN_SERIE`, `UMBRAL_CONEXION_PROBABLE`)
   - Facilita ajustes futuros sin modificar la lógica del algoritmo

2. **Optimización de Comparación de Evidencias**
   - Implementación con `HashSet<TipoEvidencia>` para operaciones O(1)
   - Uso de `Intersect` y `UnionWith` para cálculos eficientes
   - Reducción de complejidad de O(n²) a O(n)

3. **Cálculo Mejorado de Características**
   - Evalúa coincidencias en ambos estados booleanos (true/false)
   - Proporciona mayor precisión en la detección de patrones

4. **Validaciones Robustas**
   - Validación de parámetros nulos en todos los métodos públicos
   - Verificación de escenas idénticas con retorno apropiado
   - Validación de rangos para umbrales

### Mejoras de Seguridad

1. **Autenticación Obligatoria**
   - Todas las funcionalidades principales protegidas con `[RequireAuth]`
   - Redirección automática con `returnUrl` para mejor UX
   - Validación en cada acción sensible

2. **Validación Multi-Capa**
   - **Client-side**: JavaScript previene selección de misma escena en comparaciones
   - **Server-side**: Validación en controlador asegura integridad de datos
   - **Base de Datos**: Constraints y validaciones a nivel de esquema

### Mejoras de Presentación

1. **Display Correcto de Datos**
   - TipoCrimen y ModusOperandi muestran su propiedad `Nombre` en lugar del objeto completo
   - Uso de null-conditional operator (`?.`) y null-coalescing (`??`) para robustez
   - Implementado en: ResultadoComparacion, Resultados, Dashboard

2. **Interfaz Unificada**
   - Esquema de colores centralizado mediante variables CSS
   - Consistencia visual en toda la aplicación
   - Mejor experiencia de usuario

### Mejoras de Código

1. **Documentación**
   - Comentarios concisos y claros en todo el código
   - Explicación de lógica compleja y decisiones de diseño
   - Documentación orientada a estudiantes universitarios

2. **Estructura y Organización**
   - Separación de responsabilidades mejorada
   - Métodos auxiliares privados para evitar duplicación
   - Código más mantenible y legible

3. **Arquitectura con Repository Pattern y Unit of Work**
   - **Repository Pattern**: Abstracción completa del acceso a datos mediante `IRepository<T>` y repositorios específicos
   - **Unit of Work Pattern**: Coordinación de múltiples repositorios mediante `IUnitOfWork` para operaciones transaccionales
   - **Lazy Initialization**: Los repositorios en `UnitOfWork` se instancian solo cuando se acceden por primera vez
   - **Eager Loading Optimizado**: `EscenaCrimenRepository` utiliza `Include()` para cargar relaciones en una sola consulta
   - **Desacoplamiento**: Los servicios no dependen directamente de Entity Framework, solo de interfaces
   - **Testabilidad**: Arquitectura permite fácil creación de mocks para pruebas unitarias
   - **Mantenibilidad**: Cambios en acceso a datos se aíslan en la capa de repositorios

4. **Configuración de Inyección de Dependencias**
   - Todos los servicios registrados en `Program.cs` con `AddScoped` (una instancia por request HTTP)
   - `IUnitOfWork` registrado como servicio scoped para coordinar repositorios dentro del mismo request
   - Interfaces e implementaciones registradas siguiendo DIP (Dependency Inversion Principle)
   - Entity Framework configurado con retry on failure para mayor resiliencia
   - Sesiones HTTP configuradas con seguridad (HttpOnly, SecurePolicy)

---

##  Características Técnicas Destacadas

### Validaciones

- **Front-end**: 
  - Validaciones con Data Annotations y jQuery Validation
  - JavaScript personalizado para validación de comparaciones (prevenir misma escena)
  - Deshabilitación dinámica de opciones en dropdowns
- **Back-end**: 
  - Validaciones robustas en controladores
  - Verificación de existencia y estado activo de catálogos
  - Validación de parámetros y rangos
  - Validación de integridad de datos (escenas diferentes en comparaciones)
- **Base de Datos**: 
  - Constraints y validaciones a nivel de esquema
  - Índices únicos para evitar duplicados (usuario, email)
  - Relaciones con restricciones apropiadas (Restrict/Cascade)

### Manejo de Errores

- Try-catch en operaciones de base de datos
- Mensajes de error descriptivos para el usuario
- Logging de errores en desarrollo

### Optimizaciones

- **Carga Eager**: Uso de `Include()` para cargar relaciones en una sola consulta, evitando consultas N+1
- **Índices Optimizados**: 
  - Índices únicos en `Usuarios.NombreUsuario` y `Usuarios.Email`
  - Índice compuesto en `EscenasCrimen` para búsquedas de comparación
  - Índices en campos frecuentemente consultados (FechaCrimen, FechaRegistro)
- **Algoritmos Optimizados**: 
  - Uso de `HashSet` para comparación de evidencias (O(n) vs O(n²))
  - Evasión de duplicados usando `HashSet` en detección de series
- **Consultas LINQ Optimizadas**: 
  - Proyecciones eficientes
  - Ordenamiento y filtrado en base de datos
  - Paginación implícita donde aplica

### Seguridad

- **Protección CSRF**: `ValidateAntiForgeryToken` en todos los formularios POST
- **Autorización Estricta**: 
  - `[RequireAuth]` en todas las acciones de escenas
  - `[RequireAdmin]` en funciones administrativas
  - Verificación de sesión en cada request
- **Sanitización de Inputs**: Validación y limpieza de datos de entrada
- **Validación Multi-Capa**: Front-end, back-end y base de datos
- **Redirección Segura**: Validación de `returnUrl` con `Url.IsLocalUrl()` para prevenir ataques de redirección abierta

---

##  Notas Importantes

1. **Contraseñas**: El sistema actualmente almacena contraseñas en texto plano. **Se recomienda encarecidamente implementar hashing** antes de usar en producción.

2. **Base de Datos**: El sistema crea automáticamente la base de datos si no existe. Asegúrate de tener permisos adecuados en SQL Server.

3. **Roles**: El sistema asume que el RolId = 1 corresponde a Administrador. Verifica que este rol exista en la base de datos.

4. **Sesiones**: Las sesiones expiran después de 30 minutos de inactividad (configurable en `Program.cs`).

---

##  Contribuciones

Este es un proyecto académico/demostrativo. Para mejoras o correcciones:

1. Analiza el código existente
2. Identifica áreas de mejora
3. Implementa cambios siguiendo la arquitectura actual
4. Prueba exhaustivamente antes de proponer cambios

---

##  Licencia

Este proyecto es de uso educativo/demostrativo.

---

##  Autor

Sistema desarrollado para análisis y gestión de escenas de crímenes.

---

##  Aprendizajes Clave del Proyecto

Este proyecto demuestra:

- **Arquitectura MVC** en ASP.NET Core con separación en capas
- **Repository Pattern y Unit of Work Pattern** para abstracción de acceso a datos
- **Principios SOLID** aplicados en toda la arquitectura (SRP, DIP, ISP, OCP)
- **Inyección de Dependencias** con ASP.NET Core DI Container
- **Entity Framework Core** para acceso a datos con eager loading optimizado
- **Algoritmos de comparación** y análisis de patrones (coeficiente de Jaccard)
- **Sistemas de autenticación** personalizados basados en sesión
- **Gestión de catálogos** y datos maestros con activación/desactivación
- **Optimización de consultas** con índices compuestos y consultas eficientes
- **Validaciones** multi-capa (front-end, back-end, BD)

---

---

**Versión**: 2.0  
**Última actualización**: Diciembre 2024

### Changelog v2.0

- ✅ Implementación completa de Repository Pattern y Unit of Work Pattern
- ✅ Arquitectura en capas con separación clara de responsabilidades (Controllers → Services → Repositories → Models)
- ✅ Servicios especializados: ComparacionService, EscenaCrimenService, CatalogoService, UsuarioService, AuthenticationService
- ✅ Repositorios genéricos (`IRepository<T>`) y específicos (`IEscenaCrimenRepository`) con operaciones optimizadas
- ✅ Inyección de dependencias completa siguiendo principios SOLID (DIP, SRP, ISP, OCP)
- ✅ Eager loading optimizado con `Include()` para evitar consultas N+1
- ✅ Implementación de autenticación obligatoria para todas las funcionalidades
- ✅ Unificación del esquema de colores en toda la aplicación
- ✅ Validación para prevenir comparación de escenas idénticas (client-side y server-side)
- ✅ Optimización del algoritmo core con constantes y `HashSet` para evidencias
- ✅ Corrección de visualización de TipoCrimen y ModusOperandi en todas las vistas
- ✅ Mejora del cálculo de similitud de características
- ✅ Documentación mejorada con comentarios concisos en todo el código
- ✅ Mejoras de seguridad y validaciones multi-capa
- ✅ Experiencia de usuario mejorada con contenido dinámico según autenticación
