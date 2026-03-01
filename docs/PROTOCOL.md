# BlackBoxControl — Binary Serial Protocol

**Transport:** RS-232 / USB-Serial
**Baud rate:** 115 200
**MCU:** ESP32
**Source:** `Protocol/BinaryProtocol.cs`

---

## Packet Frame Format

Every packet uses the same framing regardless of type:

```
┌──────────┬──────────┬──────────┬──────────┬──────────────┬──────────┬──────────┐
│  START   │   TYPE   │  LEN_H   │  LEN_L   │   PAYLOAD    │ CHECKSUM │   END    │
│  0xAA    │  1 byte  │  1 byte  │  1 byte  │  0–512 bytes │  1 byte  │  0x55    │
└──────────┴──────────┴──────────┴──────────┴──────────────┴──────────┴──────────┘
```

| Field | Size | Description |
|-------|------|-------------|
| START | 1 byte | Always `0xAA` |
| TYPE | 1 byte | Packet type constant (see table below) |
| LEN_H | 1 byte | Payload length, high byte (big-endian) |
| LEN_L | 1 byte | Payload length, low byte |
| PAYLOAD | 0–512 bytes | Type-specific data |
| CHECKSUM | 1 byte | XOR of all payload bytes |
| END | 1 byte | Always `0x55` |

**Minimum packet size:** 6 bytes (START + TYPE + LEN_H + LEN_L + CHECKSUM + END, zero payload).
**Maximum payload:** 512 bytes (ESP32 RAM constraint).

---

## Checksum Algorithm

```csharp
byte checksum = 0;
foreach (byte b in payloadBytes)
    checksum ^= b;
```

XOR of every byte in the payload. The START, TYPE, LEN, and END bytes are **not** included.

---

## Packet Types

### Control packets

| Constant | Value | Direction | Description |
|----------|-------|-----------|-------------|
| `PACKET_HANDSHAKE` | `0xF0` | PC → ESP32 | Open session; ESP32 replies ACK |
| `PACKET_ACK` | `0xF1` | ESP32 → PC | Acknowledge last packet |
| `PACKET_NACK` | `0xF2` | ESP32 → PC | Reject last packet (bad checksum / overflow) |
| `PACKET_DOWNLOAD_REQUEST` | `0xF3` | PC → ESP32 | Request ESP32 to send its config |
| `PACKET_END_TRANSMISSION` | `0xFF` | PC → ESP32 | Close session |

### Configuration packets (upload PC → ESP32)

| Constant | Value | Description |
|----------|-------|-------------|
| `PACKET_PANEL_CONFIG` | `0x01` | Global panel settings |
| `PACKET_LOOP_CONFIG` | `0x02` | One loop definition |
| `PACKET_DEVICE_CONFIG` | `0x03` | One loop device |
| `PACKET_BUS_CONFIG` | `0x04` | One RS485 bus |
| `PACKET_CAUSE_EFFECT` | `0x05` | One C&E rule (legacy combined) |
| `PACKET_BUS_NODE_CONFIG` | `0x06` | One RS485 bus node |
| `PACKET_CE_HEADER` | `0x07` | C&E rule header |
| `PACKET_CE_INPUT` | `0x08` | One cause input within a C&E rule |
| `PACKET_CE_OUTPUT` | `0x09` | One effect output within a C&E rule |

---

## Payload Serialisation

Payloads are written using `ProtocolWriter` and read using `ProtocolReader`.

### Primitive types

| Method | Wire format |
|--------|-------------|
| `WriteByte(v)` | 1 byte |
| `WriteUInt16(v)` | 2 bytes, big-endian |
| `WriteUInt32(v)` | 4 bytes, big-endian |
| `WriteBoolean(v)` | 1 byte: `0x01` = true, `0x00` = false |
| `WriteString(s, maxLen=32)` | 1-byte length prefix + UTF-8 bytes (truncated to maxLen) |
| `WriteIPAddress(s)` | 4 bytes (one per octet) |

### String encoding detail

```
┌────────┬──────────────────────────┐
│  LEN   │  UTF-8 bytes (LEN bytes) │
│ 1 byte │  up to maxLength bytes   │
└────────┴──────────────────────────┘
```

Empty string → single `0x00` length byte, no further bytes.

---

## Upload Sequence

```
PC                                    ESP32
│                                       │
│──── HANDSHAKE (0xF0) ────────────────►│
│◄─── ACK (0xF1) ──────────────────────│  timeout: 5 000 ms
│                                       │
│──── PANEL_CONFIG (0x01) ────────────►│
│◄─── ACK (0xF1) ──────────────────────│  timeout: 2 000 ms
│                                       │
│──── LOOP_CONFIG (0x02) × N ─────────►│  one packet per loop
│◄─── ACK each ────────────────────────│
│                                       │
│──── DEVICE_CONFIG (0x03) × N ───────►│  one packet per device
│◄─── ACK each ────────────────────────│
│                                       │
│──── BUS_CONFIG (0x04) × N ──────────►│  one packet per bus
│◄─── ACK each ────────────────────────│
│                                       │
│──── BUS_NODE_CONFIG (0x06) × N ─────►│  one packet per node
│◄─── ACK each ────────────────────────│
│                                       │
│──── CE_HEADER (0x07) ───────────────►│  one per C&E rule
│──── CE_INPUT  (0x08) × N ───────────►│  one per cause input
│──── CE_OUTPUT (0x09) × N ───────────►│  one per effect output
│◄─── ACK each ────────────────────────│
│                                       │
│──── END_TRANSMISSION (0xFF) ────────►│
│                                       │
```

If ESP32 replies **NACK** at any step, the upload is aborted and an error is shown.
If no reply arrives within the timeout, the upload is also aborted.

---

## Download Sequence

```
PC                                    ESP32
│                                       │
│──── HANDSHAKE (0xF0) ────────────────►│
│◄─── ACK (0xF1) ──────────────────────│
│                                       │
│──── DOWNLOAD_REQUEST (0xF3) ────────►│
│◄─── PANEL_CONFIG (0x01) ─────────────│
│◄─── LOOP_CONFIG  (0x02) × N ─────────│
│◄─── DEVICE_CONFIG (0x03) × N ────────│
│◄─── BUS_CONFIG   (0x04) × N ─────────│
│◄─── BUS_NODE_CONFIG (0x06) × N ──────│
│◄─── CE_HEADER / CE_INPUT / CE_OUTPUT │
│◄─── END_TRANSMISSION (0xFF) ─────────│
│                                       │
```

---

## Timeouts

| Constant | Value | Used for |
|----------|-------|---------|
| `ACK_TIMEOUT_MS` | 2 000 ms | Every packet ACK wait during upload/download |
| `HANDSHAKE_TIMEOUT_MS` | 5 000 ms | Initial handshake only |

---

## Simulator (MockSerialCommunicationService)

For development without hardware, `MockSerialCommunicationService` replaces the real
serial port. It uses two `Channel<BinaryPacket>` queues:

- `_ackChannel` — receives ACK/NACK packets; read by `WaitForAckAsync`
- `_dataChannel` — receives config packets; read by `ReceivePacketAsync`

The simulator auto-ACKs every sent packet and replays a canned configuration on download.
Enable via `ISerialCommunicationService.EnableSimulator()`.
