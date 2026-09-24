# Mini Core Bancario

Proyecto de práctica en **.NET 8 / C#** que simula el núcleo de un sistema bancario simple: cuentas, movimientos y transferencias. Es un proyecto personal de aprendizaje, sin relación con ninguna entidad financiera.

## Objetivo

Practicar, de forma cercana a un entorno laboral real:

- Arquitectura en **capas** (presentación, negocio, datos).
- **C#** y POO aplicada a reglas de negocio.
- **SQL Server** con transacciones (ACID) y consultas.
- Integración con servicios **SOAP** y convivencia con código **VB.NET** (legacy).
- Flujo de trabajo profesional: ramas, commits chicos, tests y Pull Requests.

## Estructura

```
src/
  Banco.Api/       API REST (capa de presentación)
  Banco.Negocio/   Entidades y reglas de negocio
  Banco.Datos/     Acceso a datos (SQL Server)
tests/
  Banco.Tests/     Tests unitarios (xUnit)
```

Las dependencias van en un solo sentido: `Api → Negocio` y `Datos → Negocio`. La capa de negocio no conoce a las demás.

## Decisiones de diseño

- **`decimal` para dinero**, nunca `double`: evita errores de redondeo (hay un test que lo demuestra).
- **El saldo nunca puede quedar negativo**: `Cuenta.Retirar` lanza `SaldoInsuficienteException` y no modifica el estado.

## Cómo correrlo

Requisitos: SDK de .NET 8.

```bash
dotnet build
dotnet test
dotnet run --project src/Banco.Api
```

### Base de datos

Usa SQL Server (probado con SQL Server Express, instancia `localhost\SQLEXPRESS`, autenticación de Windows). Ejecutá el script [sql/01_crear_base_y_tablas.sql](sql/01_crear_base_y_tablas.sql) desde SSMS: crea la base `MiniCoreBancario`, las tablas y tres cuentas de ejemplo. Es idempotente. Si tu instancia es otra, cambiá la cadena `ConnectionStrings:Banco` en `src/Banco.Api/appsettings.json`.

### Endpoints

- `GET /health`
- `GET /cuentas/{id}/saldo`: devuelve titular y saldo, o 404 si la cuenta no existe.
- `POST /transferencias`: cuerpo `{ "origenId": 1, "destinoId": 2, "monto": 100.50, "descripcion": "opcional" }`. Devuelve `200` con los saldos resultantes, `400` si los datos son inválidos (misma cuenta, monto ≤ 0 o con más de 2 decimales), `404` si alguna cuenta no existe y `422` si el origen no tiene saldo suficiente.

### Transferencias y transacciones (ACID)

Una transferencia debita una cuenta, acredita la otra y registra dos movimientos (`TRANSFER_OUT` y `TRANSFER_IN`). `TransferenciaRepositorio` lo hace dentro de **una sola transacción SQL**: si algo falla, se deshace todo (`ROLLBACK`) y no queda un débito sin su crédito.

- **`UPDLOCK` al leer los saldos:** bloquea las filas hasta el final de la transacción, para que dos transferencias simultáneas no lean el mismo saldo y lo dejen inconsistente.
- **Bloqueo en orden de `Id`:** evita deadlocks entre transferencias cruzadas (A→B y B→A al mismo tiempo).
- **Reglas de dinero reutilizadas:** el saldo suficiente lo valida `Cuenta.Retirar`, y el `CHECK (Saldo >= 0)` de la base queda como red de seguridad.
- **Validación previa:** `TransferenciaServicio` rechaza datos inválidos antes de abrir una transacción.

Swagger queda disponible en desarrollo.

### Modelo de datos

- `Cuentas`: `Saldo DECIMAL(18,2)` con `CHECK (Saldo >= 0)`, así la regla del saldo también la garantiza la base.
- `Movimientos`: clave foránea a `Cuentas` e índice por `(CuentaId, Fecha)` para consultar movimientos por período.

### Acceso a datos

`ICuentaRepositorio` está definido en la capa de negocio y lo implementa `Banco.Datos` con ADO.NET y consultas parametrizadas (previene SQL injection). El negocio no depende de SQL Server.

## Roadmap

- [x] Esqueleto en capas, entidad `Cuenta` con reglas y tests
- [x] Persistencia en SQL Server y consulta de saldo
- [x] Transferencia entre cuentas con transacción ACID
- [ ] Consulta de movimientos por rango de fechas con paginación
- [ ] Módulo en VB.NET
- [ ] Cliente SOAP contra un servicio de prueba
