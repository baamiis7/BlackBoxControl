# BlackBoxControl — Architecture Overview

**Product:** Fire alarm panel configurator (BAAMIIS LTD)
**Platform:** Windows WPF, .NET 9
**Pattern:** MVVM with Dependency Injection

---

## System Context

```
┌─────────────────────────────────────────────────┐
│              BlackBoxControl.exe                │
│           (WPF Configuration Tool)              │
└──────────────────────┬──────────────────────────┘
                       │  USB/Serial  RS-232
                       ▼
             ┌─────────────────┐
             │   ESP32 MCU     │
             │ (Fire Panel FW) │
             └─────────────────┘
                       │
             ┌─────────────────┐
             │  Fire Panel HW  │
             │  Loops / Buses  │
             └─────────────────┘
```

The PC tool connects to the ESP32 over a serial port (115 200 baud) using a custom
binary framed protocol. Configuration is stored locally as `.kbb` files (JSON).

---

## Application Layers

```
┌──────────────────────────────────────────────────────────┐
│  Views  (XAML UserControls + MainWindow)                 │
│  MainWindow · BlackBoxControlForm · LoopForm             │
│  LoopDeviceForm · BusForm · BusNodeForm                  │
│  CauseAndEffectForm · CauseAndEffectsListForm            │
├──────────────────────────────────────────────────────────┤
│  ViewModels                                              │
│  MainViewModel ──── MenuViewModel                        │
│                └─── DevicePaletteViewModel               │
│  BlackBoxControlPanelViewModel                           │
│  LoopViewModel · LoopDeviceViewModel                     │
│  BusViewModel  · BusNodeViewModel                        │
│  CauseAndEffectViewModel · CauseAndEffectsListViewModel  │
│  UploadConfigurationViewModel                            │
│  DownloadConfigurationViewModel                          │
├──────────────────────────────────────────────────────────┤
│  Services                                                │
│  IProjectService  →  ProjectService                      │
│  ISerialCommunicationService → SerialCommunicationService│
│                              → MockSerialCommunicationService
│  ProjectMapper  (VM ↔ ProjectData conversion)           │
│  ConfigurationUploadService                              │
│  ConfigurationDownloadService                            │
├──────────────────────────────────────────────────────────┤
│  Models  (BlackBoxControlPanel, Loop, LoopDevice,        │
│           Bus, BusNode, CauseAndEffect, LogicGate)       │
├──────────────────────────────────────────────────────────┤
│  Protocol  (BinaryProtocol.cs — packet framing)          │
└──────────────────────────────────────────────────────────┘
```

---

## Startup & Dependency Injection

`App.xaml.cs.OnStartup()` manually wires the object graph (no DI container):

```
App.OnStartup()
  └── IProjectService       = new ProjectService()
  └── ISerialService        = new SerialCommunicationService()  (or Mock)
  └── MainViewModel(IProjectService, ISerialService)
        └── MenuViewModel(IProjectService)
        └── DevicePaletteViewModel()
  └── MainWindow(MainViewModel)
```

---

## Data Flow — Save Project

```
User clicks Save
  → MenuViewModel.SaveCommand
  → IProjectService.SaveAsync(path, ProjectData)
      ← ProjectMapper.ToProjectData(panelViewModels)
  → File.WriteAllTextAsync → .kbb (JSON)
```

## Data Flow — Upload to Device

```
User clicks Upload
  → UploadConfigurationViewModel.UploadCommand
  → ProjectMapper.ToProjectData(panelViewModels)
  → ConfigurationUploadService.UploadAsync(projectData, serialService)
      → SerialCommunicationService.SendPacketAsync(HANDSHAKE)
      → WaitForAckAsync  (TCS-based, 2 s timeout)
      → SendPacketAsync(PANEL_CONFIG)  → ACK
      → SendPacketAsync(LOOP_CONFIG)   → ACK  (× N loops)
      → SendPacketAsync(DEVICE_CONFIG) → ACK  (× N devices)
      → SendPacketAsync(BUS_CONFIG)    → ACK  (× N buses)
      → SendPacketAsync(BUS_NODE_CONFIG) → ACK (× N nodes)
      → SendPacketAsync(CE_HEADER / CE_INPUT / CE_OUTPUT) → ACK
      → SendPacketAsync(END_TRANSMISSION)
```

---

## Tree Structure (Panel Explorer)

```
BlackBoxControlPanel
├── Loops (container)
│   └── Loop
│       └── LoopDevice (×N)
├── Buses (container)
│   └── Bus
│       └── BusNode (×N)
└── Cause & Effects (container)
    └── CauseAndEffect
        ├── CauseInput (×N)
        └── EffectOutput (×N)
```

`MainViewModel.SelectedNode` drives `DisplayDetails()` which sets `SelectedForm`
(a `ContentControl` in `MainWindow.xaml`) to the matching UserControl.

---

## Theme System

```
App.Resources
  └── GreenTheme.xaml  (or BlueTheme / DarkTheme)
        ├── color brush definitions  (~20 SolidColorBrush keys)
        └── BaseStyles.xaml (merged)
              ├── LogicGateIcons.xaml (merged)
              ├── converters
              └── all control templates (DynamicResource colors)
```

Switching theme at runtime: `ThemeManager.ChangeTheme(app, "Blue")` clears and
reloads the merged dictionaries. See `ADR/003-basestyles-theme-split.md`.

---

## Key Files

| File | Purpose |
|------|---------|
| `App.xaml.cs` | Entry point; wires DI object graph |
| `MainWindow.xaml` | Shell — title bar, menu, tree, content area, device palette |
| `ViewModels/MainViewModel.cs` | Tree selection, navigation, add-device commands |
| `ViewModels/MenuViewModel.cs` | File menu — new/open/save/save-as, upload/download dialogs |
| `ViewModels/DevicePaletteViewModel.cs` | Device and bus-node palette strip |
| `Services/ProjectMapper.cs` | Single source of truth for VM ↔ ProjectData conversion |
| `Services/ProjectService.cs` | Async JSON file I/O (.kbb files) |
| `Services/SerialCommunicationService.cs` | Real serial port (TCS-based ACK wait) |
| `Services/MockSerialCommunicationService.cs` | Simulator (Channel-based async) |
| `Protocol/BinaryProtocol.cs` | Packet framing, ProtocolWriter/Reader helpers |
| `Themes/BaseStyles.xaml` | All shared control templates |
