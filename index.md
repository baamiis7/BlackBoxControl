---
_layout: landing
---

# BlackBoxControl

**Fire alarm panel configurator** — BAAMIIS LTD developer documentation.

BlackBoxControl is a WPF desktop tool for configuring fire alarm control panels, uploading
configurations to ESP32 microcontrollers over serial, and defining Cause & Effect logic.

## Quick start

| Step | Action |
|------|--------|
| 1 | Read **[Architecture](docs/ARCHITECTURE.md)** — layer structure and data flow (10 min) |
| 2 | Read **[Protocol](docs/PROTOCOL.md)** — ESP32 serial communication contract (15 min) |
| 3 | Open the solution in Visual Studio 2022 or Rider |
| 4 | Run the app — defaults to **Green theme** and **simulator mode** (no hardware needed) |
| 5 | Try uploading a config — the simulator auto-ACKs every packet |

## Documentation sections

- **[Guides](docs/README.md)** — architecture, protocol, data model, services, themes
- **[API Reference](api/index.md)** — auto-generated from XML doc comments in source code
- **[ADRs](docs/ADR/001-mvvm-and-di.md)** — Architecture Decision Records explaining *why*

## Build

```bash
cd C:\BlackBoxConfigurator\BlackBoxControl1
dotnet build
dotnet run
```

Expected: 0 errors, ≤ 2 warnings (pre-existing CS1998 stub in SerialCommunicationService).
