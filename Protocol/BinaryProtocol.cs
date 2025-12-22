using System;
using System.Collections.Generic;
using System.Linq;

namespace BlackBoxControl.Protocol
{
    /// <summary>
    /// Binary protocol constants for ESP32 communication
    /// </summary>
    public static class ProtocolConstants
    {
        // Protocol markers
        public const byte START_BYTE = 0xAA;
        public const byte END_BYTE = 0x55;

        // Packet types
        public const byte PACKET_HANDSHAKE = 0xF0;
        public const byte PACKET_ACK = 0xF1;
        public const byte PACKET_NACK = 0xF2;
        public const byte PACKET_DOWNLOAD_REQUEST = 0xF3;
        public const byte PACKET_PANEL_CONFIG = 0x01;
        public const byte PACKET_LOOP_CONFIG = 0x02;
        public const byte PACKET_DEVICE_CONFIG = 0x03;
        public const byte PACKET_BUS_CONFIG = 0x04;
        public const byte PACKET_CAUSE_EFFECT = 0x05;
        public const byte PACKET_END_TRANSMISSION = 0xFF;

        // Timeouts
        public const int ACK_TIMEOUT_MS = 2000;
        public const int HANDSHAKE_TIMEOUT_MS = 5000;

        // Baud rate
        public const int BAUD_RATE = 115200;

        // Max packet size (ESP32 has limited RAM)
        public const int MAX_PACKET_SIZE = 512;
    }

    /// <summary>
    /// Binary packet structure
    /// </summary>
    public class BinaryPacket
    {
        public byte PacketType { get; set; }
        public byte[] Data { get; set; }

        public BinaryPacket(byte packetType, byte[] data)
        {
            PacketType = packetType;
            Data = data ?? new byte[0];
        }

        /// <summary>
        /// Serialize packet to byte array with START, LENGTH, DATA, CHECKSUM, END
        /// </summary>
        public byte[] ToBytes()
        {
            var length = Data.Length;
            if (length > ProtocolConstants.MAX_PACKET_SIZE)
                throw new InvalidOperationException($"Packet size {length} exceeds maximum {ProtocolConstants.MAX_PACKET_SIZE}");

            var packet = new List<byte>
            {
                ProtocolConstants.START_BYTE,
                PacketType,
                (byte)(length >> 8),    // Length high byte
                (byte)(length & 0xFF)   // Length low byte
            };

            packet.AddRange(Data);

            // Calculate checksum (XOR of all data bytes)
            byte checksum = CalculateChecksum(Data);
            packet.Add(checksum);

            packet.Add(ProtocolConstants.END_BYTE);

            return packet.ToArray();
        }

        /// <summary>
        /// Parse byte array into BinaryPacket
        /// </summary>
        public static BinaryPacket FromBytes(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 6)
                throw new ArgumentException("Invalid packet: too short");

            if (bytes[0] != ProtocolConstants.START_BYTE)
                throw new ArgumentException("Invalid packet: missing start byte");

            if (bytes[bytes.Length - 1] != ProtocolConstants.END_BYTE)
                throw new ArgumentException("Invalid packet: missing end byte");

            byte packetType = bytes[1];
            int length = (bytes[2] << 8) | bytes[3];

            if (bytes.Length != length + 6) // START + TYPE + LEN_H + LEN_L + DATA + CHECKSUM + END
                throw new ArgumentException($"Invalid packet: length mismatch. Expected {length + 6}, got {bytes.Length}");

            byte[] data = new byte[length];
            Array.Copy(bytes, 4, data, 0, length);

            byte receivedChecksum = bytes[bytes.Length - 2];
            byte calculatedChecksum = CalculateChecksum(data);

            if (receivedChecksum != calculatedChecksum)
                throw new ArgumentException($"Invalid packet: checksum mismatch. Expected {calculatedChecksum:X2}, got {receivedChecksum:X2}");

            return new BinaryPacket(packetType, data);
        }

        /// <summary>
        /// Calculate XOR checksum of data
        /// </summary>
        private static byte CalculateChecksum(byte[] data)
        {
            byte checksum = 0;
            foreach (byte b in data)
                checksum ^= b;
            return checksum;
        }
    }

    /// <summary>
    /// Binary writer helper for protocol serialization
    /// </summary>
    public class ProtocolWriter
    {
        private readonly List<byte> _buffer = new List<byte>();

        public void WriteByte(byte value) => _buffer.Add(value);

        public void WriteUInt16(ushort value)
        {
            _buffer.Add((byte)(value >> 8));
            _buffer.Add((byte)(value & 0xFF));
        }

        public void WriteUInt32(uint value)
        {
            _buffer.Add((byte)(value >> 24));
            _buffer.Add((byte)(value >> 16));
            _buffer.Add((byte)(value >> 8));
            _buffer.Add((byte)(value & 0xFF));
        }

        public void WriteString(string value, int maxLength = 32)
        {
            if (string.IsNullOrEmpty(value))
            {
                WriteByte(0);
                return;
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(value);
            var length = Math.Min(bytes.Length, maxLength);

            WriteByte((byte)length);
            _buffer.AddRange(bytes.Take(length));
        }

        public void WriteBoolean(bool value) => WriteByte((byte)(value ? 1 : 0));

        public void WriteIPAddress(string ipAddress)
        {
            var parts = ipAddress.Split('.');
            if (parts.Length != 4)
            {
                // Invalid IP, write zeros
                WriteByte(0);
                WriteByte(0);
                WriteByte(0);
                WriteByte(0);
                return;
            }

            foreach (var part in parts)
            {
                if (byte.TryParse(part, out byte value))
                    WriteByte(value);
                else
                    WriteByte(0);
            }
        }

        public byte[] ToArray() => _buffer.ToArray();

        public void Clear() => _buffer.Clear();
    }

    /// <summary>
    /// Binary reader helper for protocol deserialization
    /// </summary>
    public class ProtocolReader
    {
        private readonly byte[] _buffer;
        private int _position;

        public ProtocolReader(byte[] buffer)
        {
            _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
            _position = 0;
        }

        public int Position => _position;
        public int Remaining => _buffer.Length - _position;

        public byte ReadByte()
        {
            if (_position >= _buffer.Length)
                throw new InvalidOperationException("End of buffer reached");
            return _buffer[_position++];
        }

        public ushort ReadUInt16()
        {
            byte high = ReadByte();
            byte low = ReadByte();
            return (ushort)((high << 8) | low);
        }

        public uint ReadUInt32()
        {
            byte b1 = ReadByte();
            byte b2 = ReadByte();
            byte b3 = ReadByte();
            byte b4 = ReadByte();
            return (uint)((b1 << 24) | (b2 << 16) | (b3 << 8) | b4);
        }

        public string ReadString()
        {
            byte length = ReadByte();
            if (length == 0)
                return string.Empty;

            if (_position + length > _buffer.Length)
                throw new InvalidOperationException("String length exceeds buffer");

            var str = System.Text.Encoding.UTF8.GetString(_buffer, _position, length);
            _position += length;
            return str;
        }

        public bool ReadBoolean() => ReadByte() != 0;

        public string ReadIPAddress()
        {
            byte b1 = ReadByte();
            byte b2 = ReadByte();
            byte b3 = ReadByte();
            byte b4 = ReadByte();
            return $"{b1}.{b2}.{b3}.{b4}";
        }
    }
}
