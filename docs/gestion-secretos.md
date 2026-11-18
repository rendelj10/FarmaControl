
# FarmaControl - Gestión de secretos

## 1. Objetivo

Documentar la configuración de las credenciales de conexión a SQL Server mediante Secret Manager de ASP.NET Core.

## 2. Configuración del proyecto

El proyecto utiliza ASP.NET Core MVC con .NET 10.

En el archivo `Proyecto_FarmaControl.csproj` se encuentra configurado `UserSecretsId`, necesario para identificar el almacén de secretos del proyecto.

## 3. Configuración de la conexión

Se utilizó el siguiente comando:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "[cadena de conexión local]"
```

La cadena de conexión se almacena fuera del repositorio, en el almacén de secretos del usuario.

No se incorpora el contenido de los secretos al repositorio Git.

## 4. Lectura de la configuración

En `Program.cs` se obtiene la conexión mediante:

```csharp
builder.Configuration.GetConnectionString("DefaultConnection")
```

La configuración se utiliza para registrar `AppDbContext` con SQL Server mediante Entity Framework Core.

## 5. Ambiente de ejecución

El archivo `Properties/launchSettings.json` establece el ambiente `Development` para los perfiles HTTP y HTTPS.

ASP.NET Core carga los secretos del proyecto durante la ejecución en este ambiente.

## 6. Verificación realizada

Se realizaron las siguientes comprobaciones:

- Configuración del `UserSecretsId`.
- Registro de `ConnectionStrings:DefaultConnection`.
- Consulta de secretos mediante `dotnet user-secrets list`.
- Compilación satisfactoria mediante `dotnet build`.
- Inicio de la aplicación mediante `dotnet run --launch-profile https`.
- Consulta GET al endpoint `/api/categorias`.
- Confirmación de lectura de registros desde `FarmaControlDB`.

## 7. Resultado

La aplicación obtuvo correctamente la configuración de conexión y realizó consultas a SQL Server mediante Entity Framework Core.

Se comprobó que la conexión funciona sin incorporar las credenciales al repositorio.
