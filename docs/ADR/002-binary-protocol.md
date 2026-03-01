# ADR-002: Custom Binary Framed Protocol over Serial

**Status:** Accepted
**Date:** 2024

---

## Context

The PC tool must transfer structured configuration data to an ESP32 microcontroller
with limited RAM (512-byte packet limit) and no OS-level framing.

## Decision

Use a custom **binary framed protocol** with:
- Fixed `0xAA` start byte and `0x55` end byte
- 2-byte big-endian payload length field
- XOR checksum over payload bytes only
- Explicit packet type byte per transfer unit (panel, loop, device, bus, C&E)
- ACK/NACK handshake after every packet

## Rationale

- **Binary vs text (JSON/XML):** Significantly smaller payloads; ESP32 does not
  need a JSON parser
- **Per-packet ACK:** Gives fine-grained error recovery — if one device packet
  fails, only that device needs resending (not the entire config)
- **XOR checksum:** Sufficient for single-bit error detection on a short-range
  USB-serial link; hardware CRC would be overkill
- **Start/end markers:** Allow the receiver to re-synchronise after noise or
  partial packets without a reset

## Consequences

**Positive:**
- Very small payloads (fits ESP32 RAM)
- Simple to implement on both PC (C#) and firmware (C)
- Deterministic upload time (no retry storms)

**Negative:**
- Both sides must be updated in lockstep when packet format changes
- XOR checksum does not detect all multi-bit errors (acceptable for wired USB)
- Packet format changes require bumping firmware and re-releasing the PC tool
