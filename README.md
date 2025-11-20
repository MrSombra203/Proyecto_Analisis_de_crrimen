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
- [Configuración de Base de Datos](#configuración-de-base-de-datos)
- [Uso del Sistema](#uso-del-sistema)

---

##  Descripción General

Este sistema permite a las autoridades y analistas de crímenes registrar escenas delictivas de manera estructurada, comparar casos similares y detectar automáticamente patrones que puedan indicar crímenes en serie. El sistema utiliza un algoritmo de comparación avanzado que analiza múltiples factores para determinar el grado de similitud entre diferentes escenas de crímenes.

---

##  Características Principales

- **Registro de Escenas de Crimen**: Captura detallada de información sobre crímenes incluyendo ubicación, fecha, tipo, modus operandi, evidencias y características adicionales
- **Sistema de Comparación Inteligente**: Algoritmo que calcula el porcentaje de similitud entre escenas basado en múltiples criterios
- **Detección de Crímenes en Serie**: Identificación automática de patrones que sugieren crímenes relacionados
- **Gestión de Catálogos**: Administración de tipos de crimen y modus operandi
- **Sistema de Usuarios y Roles**: Control de acceso basado en roles (Administrador, Usuario)
- **Dashboard Analítico**: Panel de control para administradores con estadísticas y detección de series
- **Búsqueda de Escenas Similares**: Encuentra automáticamente casos relacionados con una escena base

---

##  Core del Sistema

### ComparacionService - El Corazón del Sistema

El **`ComparacionService`** es el componente central del proyecto. Este servicio implementa la lógica de análisis criminal mediante un algoritmo de comparación multi-criterio que evalúa:

1. **Tipo de Crimen** (25 puntos):** Compara si ambas escenas pertenecen al mismo tipo de delito
2. **Área Geográfica** (20 puntos):** Evalúa si ocurrieron en la misma zona geográfica
3. **Modus Operandi** (25 puntos):** Analiza si el método utilizado es compatible
4. **Horario del Crimen** (10 puntos):** Compara el momento del día en que ocurrieron
5. **Evidencias Físicas** (15 puntos):** Calcula la similitud basada en tipos de evidencias encontradas
6. **Características Adicionales** (5 puntos):** Evalúa uso de violencia, planificación, múltiples perpetradores

El algoritmo genera un **porcentaje de similitud** y clasifica los resultados en tres categorías:

- **CrimenEnSerie** (≥75%): Alta probabilidad de conexión
- **ConexionProbable** (≥60%): Posible relación entre casos
- **SimilitudBaja** (<60%): Baja probabilidad de conexión

### Funcionalidades Core

#### 1. Comparación de Escenas (`CompararEscenas`)
```csharp
public ComparacionResultado CompararEscenas(EscenaCrimen escenaBase, EscenaCrimen escenaComparada)
```
Compara dos escenas específicas y retorna un resultado detallado con porcentaje de similitud y lista de coincidencias.

#### 2. Búsqueda de Escenas Similares (`BuscarEscenasSimilares`)
```csharp
public List<ComparacionResultado> BuscarEscenasSimilares(EscenaCrimen escenaBase, List<EscenaCrimen> todasLasEscenas, double umbralMinimo = 60)
```
Busca todas las escenas similares a una escena base, filtrando por un umbral mínimo de similitud (por defecto 60%).

#### 3. Detección de Crímenes en Serie (`DetectarCrimenesEnSerie`)
```csharp
public List<List<EscenaCrimen>> DetectarCrimenesEnSerie(List<EscenaCrimen> todasLasEscenas)
```
Identifica grupos de crímenes que forman series, utilizando un umbral de 75% para considerar crímenes en serie.

---

##  Arquitectura

El proyecto sigue el patrón **MVC (Model-View-Controller)** con una arquitectura en capas:

```
┌─────────────────────────────────────┐
│         Controllers Layer            │
│  (EscenaCrimen, Auth, Catalogos)    │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│         Services Layer               │
│  (ComparacionService - CORE)        │
│  (AuthenticationService)             │
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

### Componentes Principales

- **Controllers**: Manejan las peticiones HTTP y coordinan entre vistas y servicios
- **Services**: Contienen la lógica de negocio, especialmente `ComparacionService`
- **Models**: Entidades del dominio y contexto de base de datos
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
├── Services/                 # Servicios de negocio
│   ├── ComparacionService.cs        # CORE: Algoritmo de comparación
│   └── AuthenticationService.cs     # Servicio de autenticación
│
├── Attributes/               # Atributos personalizados
│   └── AuthorizeAttribute.cs        # Filtros de autorización
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

### Para Todos los Usuarios Autenticados

1. **Registrar Escena de Crimen**
   - Formulario completo con validaciones
   - Selección de tipo de crimen y modus operandi
   - Selección múltiple de evidencias
   - Campos adicionales (violencia, planificación, etc.)

2. **Ver Lista de Escenas**
   - Listado ordenado por fecha de registro
   - Información resumida de cada escena

3. **Comparar Escenas**
   - Selección de dos escenas para comparar
   - Visualización de resultados detallados
   - Porcentaje de similitud y lista de coincidencias

4. **Buscar Escenas Similares**
   - A partir de una escena base, encuentra todas las similares
   - Ordenadas por porcentaje de similitud descendente

### Solo para Administradores

1. **Dashboard Analítico**
   - Estadísticas generales del sistema
   - Total de escenas registradas
   - Número de crímenes en serie detectados
   - Últimas escenas registradas

2. **Gestión de Catálogos**
   - CRUD completo de Tipos de Crimen
   - CRUD completo de Modus Operandi
   - Activación/Desactivación de registros

3. **Gestión de Usuarios**
   - Crear, editar y desactivar usuarios
   - Asignación de roles
   - Validación de emails y nombres de usuario únicos

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

La similitud de evidencias se calcula usando el **coeficiente de Jaccard**:

```
Similitud = (Evidencias Comunes) / (Evidencias Totales Únicas)
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
2. **`[RequireAdmin]`**: Requiere rol de Administrador (RolId = 1)

### Flujo de Autenticación

```
Usuario → Login → AuthenticationService.AuthenticateAsync()
         ↓
    Validación de credenciales
         ↓
    Creación de sesión
         ↓
    Redirección según rol:
    - Administrador → Dashboard
    - Usuario → Lista de Escenas
```

**Nota de Seguridad**: El sistema actualmente almacena contraseñas en texto plano. Se recomienda implementar hashing (bcrypt, PBKDF2) en producción.

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

##  Uso del Sistema

### 1. Iniciar Sesión

1. Accede a `/Auth/Login`
2. Ingresa tus credenciales
3. El sistema te redirigirá según tu rol

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

1. Ve a **Escenas de Crimen** → **Comparar**
2. Selecciona dos escenas de los dropdowns
3. Haz clic en **Comparar**
4. Revisa el resultado:
   - Porcentaje de similitud
   - Lista de coincidencias
   - Clasificación (Crimen en Serie, Conexión Probable, Similitud Baja)

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

##  Características Técnicas Destacadas

### Validaciones

- **Front-end**: Validaciones con Data Annotations y jQuery Validation
- **Back-end**: Validaciones robustas en controladores
- **Base de Datos**: Constraints y validaciones a nivel de esquema

### Manejo de Errores

- Try-catch en operaciones de base de datos
- Mensajes de error descriptivos para el usuario
- Logging de errores en desarrollo

### Optimizaciones

- Uso de `Include()` para carga eager de relaciones
- Índices en campos frecuentemente consultados
- Consultas optimizadas con LINQ

### Seguridad

- Protección CSRF con `ValidateAntiForgeryToken`
- Validación de autorización en cada acción sensible
- Sanitización de inputs

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

- **Arquitectura MVC** en ASP.NET Core
- **Entity Framework Core** para acceso a datos
- **Algoritmos de comparación** y análisis de patrones
- **Sistemas de autenticación** personalizados
- **Gestión de catálogos** y datos maestros
- **Optimización de consultas** con índices
- **Validaciones** multi-capa (front-end, back-end, BD)

---

**Versión**: 1.0  
**Última actualización**: 2024
