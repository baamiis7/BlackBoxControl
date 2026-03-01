# BlackBoxControl — Data Model

This document covers the runtime models (`Models/`) and the serialised DTOs used
in `.kbb` project files.

---

## Runtime Models (`Models/`)

### BlackBoxControlPanel

Top-level hardware model. One instance per fire panel.

| Property | Type | Description |
|----------|------|-------------|
| PanelName | string | Display name |
| Location | string | Physical location |
| PanelAddress | string | Panel network address |
| NumberOfLoops | int | Configured loop count |
| NumberOfZones | int | Zone count |
| FirmwareVersion | string | Installed firmware version |
| ConfigGood | string | Config validity flag |
| NodeAddress | string | RS485 node address |
| NetCardAddress | string | Network card IP/MAC |
| WiFiSSID | string | WiFi network name |
| LPSDefVol | string | Default sounder volume |
| LPSDefTone | string | Default sounder tone |
| LoopOffset | string | Loop address offset |
| ISDevices | bool | IS (intrinsically safe) devices fitted |
| TSYear/Month/Day/Hour/Minute | string | RTC time-stamp fields |
| BatteryCapacity | string | Battery capacity (Ah) |
| NumberOfBatteries | int | Battery count |
| NumberOfPowerSupplies | int | PSU count |
| ImagePath | string | Path to panel image |
| Loops | ObservableCollection\<Loop\> | Child loops |
| Busses | ObservableCollection\<Bus\> | Child buses |
| CauseAndEffects | ObservableCollection\<CauseAndEffect\> | C&E rules |

---

### Loop

Represents one detection loop (SLC / addressable loop).

| Property | Type | Description |
|----------|------|-------------|
| LoopNumber | int | Unique loop number (1-based) |
| LoopName | string | Display name |
| LoopProtocol | string | Protocol: Argus / Hochiki / Apollo / System Sensor |
| NumberOfDevices | int | Read-only count (auto-calculated) |
| ImagePath | string | Optional loop diagram image |
| Devices | ObservableCollection\<LoopDevice\> | Child devices |

---

### LoopDevice

One addressable field device on a loop (detector, sounder, MCP, etc.).

| Property | Type | Description |
|----------|------|-------------|
| Type | string | Device type (from device templates JSON) |
| Name | string | Display name |
| Address | int | Loop address |
| LocationText | string | Physical location description |
| ImagePath | string | Device icon path |
| AnalogValue | int | Current analog reading (%) |
| DeviceThreshold | int | Alarm threshold (%) |
| DeviceDaySensitivity | int | Day-mode sensitivity (0–100) |
| DeviceNightSensitivity | int | Night-mode sensitivity (0–100) |
| DeviceInputAction | string | Action on alarm trigger |
| DeviceActionMessage | string | Custom alarm message |

---

### Bus

One RS485 bus (typically 2 buses per panel).

| Property | Type | Description |
|----------|------|-------------|
| BusNumber | int | Bus number (1-based) |
| BusName | string | Display name |
| BusType | string | RS485 / RS232 / CAN / Ethernet |
| NumberOfNodes | int | Node count (auto-calculated) |
| Nodes | ObservableCollection\<BusNode\> | Child nodes |

---

### BusNode

One node on an RS485 bus (actuator, solenoid, relay output, etc.).

| Property | Type | Description |
|----------|------|-------------|
| Name | string | Node type / display name |
| Address | int | Bus address |
| LocationText | string | Physical location |
| ImagePath | string | Node icon path |

---

### CauseAndEffect

One logic rule linking input triggers to output actions.

| Property | Type | Description |
|----------|------|-------------|
| Name | string | Rule name |
| IsEnabled | bool | Active/inactive toggle |
| LogicGate | LogicGate enum | OR / AND / XOR |
| Status | string | "Active" / "Inactive" (derived) |
| Inputs | ObservableCollection\<CauseInput\> | Trigger conditions |
| Outputs | ObservableCollection\<EffectOutput\> | Actions to perform |

### LogicGate (enum)

```
OR   — any one input triggers the rule
AND  — all inputs must trigger simultaneously
XOR  — exactly one input must trigger
```

### CauseInput

One input condition within a C&E rule.

| Property | Type | Description |
|----------|------|-------------|
| Type | string | "Device" / "TimeOfDay" / "DateTime" / "ReceiveApi" |
| DeviceId | string? | Referenced device (Type=Device) |
| TimeOfDay | string? | Scheduled time (Type=TimeOfDay) |
| DateTime | DateTime? | Specific date/time (Type=DateTime) |
| ApiEndpoint | string? | Webhook URL (Type=ReceiveApi) |

### EffectOutput

One output action within a C&E rule.

| Property | Type | Description |
|----------|------|-------------|
| Type | string | "Device" / "SendApi" / "SendEmail" / "SendText" |
| DeviceId | string? | Target device (Type=Device) |
| ApiEndpoint | string? | Webhook URL (Type=SendApi) |
| EmailAddress | string? | Recipient (Type=SendEmail) |
| PhoneNumber | string? | Recipient (Type=SendText) |

---

## Project File Format (`.kbb`)

`.kbb` files are **UTF-8 JSON** serialised from `ProjectData` by `ProjectService`.
The extension stands for **B**lack**B**ox **K**onfiguration.

### Top-level structure

```json
{
  "ProjectName": "Site A — Building 1",
  "Panels": [ { ... } ]
}
```

### Panel object

```json
{
  "PanelName": "Main Panel",
  "Location":  "Ground Floor, Room 1",
  "Loops": [ ... ],
  "Busses": [ ... ],
  "CauseAndEffects": [ ... ]
}
```

### Conversion

`Services/ProjectMapper.cs` is the single source of truth for all conversions:

| Method | Direction |
|--------|-----------|
| `ToProjectData(name, panelVMs)` | ViewModels → DTO (save / upload) |
| `ToPanelViewModels(projectData)` | DTO → ViewModels (load / download) |

Do **not** add conversion logic elsewhere — always go through `ProjectMapper`.
