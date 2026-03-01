# BlackBoxControl — Documentation Index

Fire alarm panel configurator (BAAMIIS LTD) — developer documentation.

---

## Documents

| Document | Description |
|----------|-------------|
| [ARCHITECTURE.md](ARCHITECTURE.md) | System overview, layers, startup, data flow diagrams |
| [PROTOCOL.md](PROTOCOL.md) | Binary serial protocol — packet format, types, upload/download sequences |
| [DATA-MODEL.md](DATA-MODEL.md) | Runtime models (Panel, Loop, Device, Bus, C&E) and `.kbb` file format |
| [SERVICES.md](SERVICES.md) | Service interfaces — ProjectService, ProjectMapper, SerialCommunicationService |
| [THEMES.md](THEMES.md) | Theme system — brush keys, BaseStyles pattern, adding a new theme |

## Architecture Decision Records

| ADR | Decision |
|-----|---------|
| [ADR-001](ADR/001-mvvm-and-di.md) | MVVM pattern with manual dependency injection |
| [ADR-002](ADR/002-binary-protocol.md) | Custom binary framed protocol over serial |
| [ADR-003](ADR/003-basestyles-theme-split.md) | BaseStyles.xaml + per-theme color files |

---

## Quick-start for new developers

1. Read **ARCHITECTURE.md** — understand the layer structure and data flow (10 min)
2. Read **PROTOCOL.md** — understand the ESP32 communication contract (15 min)
3. Open the solution in Visual Studio 2022 or Rider
4. Run the app — it defaults to the **Green theme** and **simulator mode** (no hardware needed)
5. Try uploading a config — the simulator auto-ACKs every packet

## Project file location

`C:\BlackBoxConfigurator\BlackBoxControl1\BlackBoxControl.csproj`

## Build

```bash
cd C:\BlackBoxConfigurator\BlackBoxControl1
dotnet build
dotnet run
```

Expected: 0 errors, ≤ 2 warnings (pre-existing CS1998 stub in SerialCommunicationService).
