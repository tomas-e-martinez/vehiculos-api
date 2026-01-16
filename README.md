# Vehículos API

API REST para gestión de vehículos y seguimiento de mantenimiento, desarrollada con ASP.NET Core 10.

## Demo en Vivo

- **Frontend**: [vehiculos.tomasmartinez.com.ar](https://vehiculos.tomasmartinez.com.ar)
- **Backend**: Desplegado en VPS personal

## Sobre el Proyecto

Este es un proyecto personal/portfolio que permite a los usuarios registrar sus vehículos y llevar un control del mantenimiento preventivo. La idea surgió de la necesidad de tener un registro organizado de cuándo toca hacer el próximo service del auto o la moto.

### Funcionalidades Principales

- **Autenticación JWT**: Registro e inicio de sesión con tokens seguros
- **Gestión de Vehículos**: Crear, editar y dar de baja vehículos (autos y motos)
- **Seguimiento de Kilometraje**: Historial de actualizaciones de kilómetros
- **Mantenimiento Preventivo**: Tareas automáticas según el tipo de vehículo
- **Roles de Usuario**: Sistema de permisos (Admin/Usuario)

## Stack Tecnológico

- **Framework**: ASP.NET Core 10.0
- **Lenguaje**: C#
- **Base de Datos**: MySQL con Entity Framework Core
- **Autenticación**: JWT Bearer
- **Contenedores**: Docker
- **CI/CD**: GitHub Actions
- **Hosting**: VPS propio

## Arquitectura

```
vehiculos-api/
├── Controller/          # Controladores de la API
├── Model/               # Entidades del dominio
├── Service/             # Lógica de negocio
├── DTOs/                # Objetos de transferencia de datos
├── Data/                # Contexto de base de datos
├── Migrations/          # Migraciones de EF Core
└── Program.cs           # Configuración y startup
```

## Endpoints de la API

### Usuarios (`/users`)

| Método | Endpoint | Descripción | Auth |
|--------|----------|-------------|------|
| POST | `/users/signup` | Registrar nuevo usuario | No |
| POST | `/users/login` | Iniciar sesión | No |
| GET | `/users` | Listar usuarios | Admin |

### Vehículos (`/vehicles`)

| Método | Endpoint | Descripción | Auth |
|--------|----------|-------------|------|
| GET | `/vehicles` | Obtener vehículos del usuario | Sí |
| GET | `/vehicles/{id}` | Detalle de un vehículo | Sí |
| POST | `/vehicles` | Crear vehículo | Sí |
| PATCH | `/vehicles/{id}` | Actualizar vehículo | Sí |
| DELETE | `/vehicles/{id}` | Dar de baja vehículo | Sí |
| GET | `/vehicles/{id}/maintenance` | Tareas de mantenimiento | Sí |
| GET | `/vehicles/{id}/kilometers` | Historial de kilometraje | Sí |
| PATCH | `/vehicles/{id}/kilometers` | Actualizar kilometraje | Sí |

### Mantenimiento (`/maintenance`)

| Método | Endpoint | Descripción | Auth |
|--------|----------|-------------|------|
| PATCH | `/maintenance/{id}/complete` | Completar tarea | Sí |

### Health Check

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/health` | Estado de la API |

## Configuración Local

### Prerequisitos

- .NET 10.0 SDK
- MySQL Server
- Docker (opcional)

### Variables de Entorno

Crear un archivo `appsettings.Development.json` o configurar las siguientes variables:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=vehicles;User ID=tu_usuario;Password=tu_password;"
  },
  "Jwt": {
    "Key": "tu_clave_secreta_de_al_menos_32_caracteres",
    "Issuer": "vehiculos-api",
    "Audience": "vehiculos-api"
  }
}
```

### Ejecutar el Proyecto

```bash
# Restaurar dependencias
dotnet restore

# Aplicar migraciones
dotnet ef database update

# Ejecutar
dotnet run
```

La API va a estar disponible en `http://localhost:5000` o `https://localhost:5001`.

### Con Docker

```bash
# Construir imagen
docker build -t vehiculos-api .

# Ejecutar contenedor
docker run -p 8080:8080 -e DB_CONNECTION="tu_connection_string" vehiculos-api
```

## Deploy

El proyecto usa GitHub Actions para CI/CD. Cada push a `master` dispara:

1. Build de la imagen Docker
2. Push a GitHub Container Registry
3. Deploy automático al VPS via SSH

## Proyecto Relacionado

Este backend funciona junto con el frontend desarrollado en React:
- **URL**: [vehiculos.tomasmartinez.com.ar](https://vehiculos.tomasmartinez.com.ar)

## Licencia

Este proyecto es de código abierto y está disponible para fines educativos y de portfolio.

---

Desarrollado por [Tomás Martínez](https://github.com/tomas-e-martinez)
