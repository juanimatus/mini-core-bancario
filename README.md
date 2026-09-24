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

La API expone `GET /health` y Swagger en desarrollo.

## Roadmap

- [x] Esqueleto en capas, entidad `Cuenta` con reglas y tests
- [ ] Persistencia en SQL Server y consulta de saldo
- [ ] Transferencia entre cuentas con transacción ACID
- [ ] Consulta de movimientos por rango de fechas con paginación
- [ ] Módulo en VB.NET
- [ ] Cliente SOAP contra un servicio de prueba
