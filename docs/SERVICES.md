# BlackBoxControl — Services

---

## IProjectService / ProjectService

**File:** `Services/IProjectService.cs`, `Services/ProjectService.cs`

Responsible for saving and loading `.kbb` project files. Pure file I/O — no
conversion logic lives here.

```csharp
interface IProjectService
{
    void   Save(string filePath, ProjectData data);
    Task   SaveAsync(string filePath, ProjectData data);
    ProjectData? Load(string filePath);
    Task<ProjectData?> LoadAsync(string filePath);
}
```

- Uses `Newtonsoft.Json` for serialisation
- Async overloads use `File.WriteAllTextAsync` / `File.ReadAllTextAsync`
- Called only from `MenuViewModel` — never directly from ViewModels below it

---

## ProjectMapper

**File:** `Services/ProjectMapper.cs`

Single source of truth for converting between ViewModels and the `ProjectData` DTO.
All save, load, upload, and download paths go through this class.

| Method | Purpose |
|--------|---------|
| `ToProjectData(name, panelVMs)` | ViewModels → DTO for save/upload |
| `ToPanelViewModels(projectData)` | DTO → ViewModels for load/download |
| `ToCauseInputData(input)` | CauseInput VM → DTO |
| `ToCauseInput(data)` | DTO → CauseInput VM |
| `ToEffectOutputData(output)` | EffectOutput VM → DTO |
| `ToEffectOutput(data)` | DTO → EffectOutput VM |

**Rule:** Do not add conversion logic in ViewModels or ProjectService.
Always extend `ProjectMapper` instead.

---

## ISerialCommunicationService

**File:** `Services/ISerialCommunicationService.cs`

Abstracts the serial port so that ViewModels can work with real hardware or the
simulator interchangeably.

```csharp
interface ISerialCommunicationService
{
    void   EnableSimulator();
    Task   SendPacketAsync(BinaryPacket packet, CancellationToken ct);
    Task<BinaryPacket?> ReceivePacketAsync(CancellationToken ct);
    // ... port management ...
}
```

---

## SerialCommunicationService (real hardware)

**File:** `Services/SerialCommunicationService.cs`

- Opens `System.IO.Ports.SerialPort` at 115 200 baud
- `SendPacketAsync` — serialises `BinaryPacket.ToBytes()` and writes to port
- `WaitForAckAsync` — **TaskCompletionSource-based** (no polling):
  - Creates a `TaskCompletionSource<bool>`
  - `SerialPort.DataReceived` fires, buffers bytes, parses ACK packet,
    calls `_ackTcs.TrySetResult(isAck)`
  - `WaitForAckAsync` awaits `Task.WhenAny(tcs.Task, Task.Delay(timeout))`
- `_receiveBuffer` accumulates bytes between `DataReceived` events

---

## MockSerialCommunicationService (simulator)

**File:** `Services/MockSerialCommunicationService.cs`

Used during development or when no hardware is connected.

- Two **`Channel<BinaryPacket>`** replace the old `Queue<BinaryPacket>` + polling:
  - `_ackChannel` — ACK/NACK packets; read by `WaitForAckAsync`
  - `_dataChannel` — config packets; read by `ReceivePacketAsync`
- `OnSimulatorPacket` routes by packet type into the correct channel
- `WaitForAckAsync` calls `_ackChannel.Reader.ReadAsync(timeoutCts.Token)` — no sleep loop
- `ReceivePacketAsync` calls `_dataChannel.Reader.ReadAsync(timeoutCts.Token)`
- `ClearStoredData()` drains both channels with `TryRead` loops

Activated by calling `EnableSimulator()` (overrides base class no-op).

---

## ConfigurationUploadService

**File:** `Services/ConfigurationUploadService.cs`

Orchestrates the full upload sequence:

1. Handshake
2. Send PANEL_CONFIG
3. For each Loop: LOOP_CONFIG + N × DEVICE_CONFIG
4. For each Bus: BUS_CONFIG + N × BUS_NODE_CONFIG
5. For each C&E: CE_HEADER + N × CE_INPUT + N × CE_OUTPUT
6. END_TRANSMISSION

Uses `ISerialCommunicationService` — works identically with real or mock.

---

## ConfigurationDownloadService

**File:** `Services/ConfigurationDownloadService.cs`

1. Sends DOWNLOAD_REQUEST
2. Reads packets via `ReceivePacketAsync` until END_TRANSMISSION
3. Builds `ProjectData` from received packets
4. Calls `ProjectMapper.ToPanelViewModels` to populate the tree
