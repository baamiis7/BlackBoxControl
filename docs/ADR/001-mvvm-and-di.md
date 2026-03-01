# ADR-001: MVVM Pattern with Manual Dependency Injection

**Status:** Accepted
**Date:** 2024

---

## Context

The application is a WPF desktop tool. It needs testable ViewModels, swappable
services (real serial port vs. simulator), and clear separation between UI and
business logic.

## Decision

Adopt the **MVVM** pattern throughout:
- All UI state lives in ViewModels that extend `ViewModelBase`
- Views (XAML) bind to ViewModel properties and commands only
- No code-behind logic except event handlers that immediately delegate to the VM

Use **manual dependency injection** via `App.xaml.cs.OnStartup()` rather than a
DI container (e.g. Microsoft.Extensions.DependencyInjection).

## Rationale

- The object graph is small and shallow — a DI container adds complexity without benefit
- Manual wiring is explicit and easier to trace for a small team
- The `ISerialCommunicationService` / `IProjectService` interfaces are sufficient to
  enable unit testing of ViewModels without any container

## Consequences

**Positive:**
- ViewModels are independently testable (pass mock services in constructor)
- Serial hardware can be swapped for simulator without changing any ViewModel
- No NuGet dependency on a DI framework

**Negative:**
- Adding a new transitive dependency requires manually threading it through constructors
- No lifetime management (all objects are effectively singletons for the app lifetime)
