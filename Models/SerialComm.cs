using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackBoxControl.ViewModels;
namespace BlackBoxControl.Models
{
    public class SerialCommunicator
    {
        private const byte SER_SYNC = 0x16;   // Sync byte
        private const byte SER_ESCAPE = 0x1B; // Escape byte
        private const byte DESTINATION_PC = 1;

        private byte PacketCheckSum;
        private byte[] PacketBuf; // Packet buffer for payload data
        private SerialCommViewModel serialComm; // Serial communication model view

        public SerialCommunicator(int packetBufSize, SerialCommViewModel serialCommModel)
        {
            PacketBuf = new byte[packetBufSize];
            this.serialComm = serialCommModel;
        }

        // Send a packet of data using the SerialCommModelView
        public void SerSendPacket(byte packetType, int length, byte nodeTo, byte serMsgId)
        {
            PacketCheckSum = 0; // Reset checksum before sending a new packet

            // Send the sync byte first
            SendByte(SER_SYNC);

            // Determine local or network serial packet
            if (nodeTo == DESTINATION_PC || nodeTo == ConfGetNodeAddress())
            {
                SendByte(serMsgId);           // Local serial packet
                SendByte(ConfGetNodeAddress());
            }
            else
            {
                SendByte(serMsgId);           // Network serial packet
                SendByte(nodeTo);
            }

            // Send packet type and length
            SendByte(packetType);
            SendByte((byte)length);

            // Send payload from the buffer
            for (int i = 0; i < length; i++)
            {
                SendByte(PacketBuf[i]);
            }

            // Send the final checksum byte
            SendByte(PacketCheckSum);
        }

        // Handle special cases for the byte to be sent and update checksum
        private void SendByte(byte c)
        {
            if (c == SER_SYNC)
            {
                SerPutCh(SER_ESCAPE, DESTINATION_PC);
                SerPutCh(1, DESTINATION_PC);
            }
            else if (c == SER_ESCAPE)
            {
                SerPutCh(SER_ESCAPE, DESTINATION_PC);
                SerPutCh(0, DESTINATION_PC);
            }
            else
            {
                SerPutCh(c, DESTINATION_PC);
            }

            // Add byte to the packet checksum
            PacketCheckSum += c;
        }

        // Simulate sending a byte to the serial port through SerialCommModelView
        private void SerPutCh(byte c, byte dest)
        {
            byte[] byteToSend = new byte[] { c }; // Prepare a byte array to send
            serialComm._serialModel.SendData(byteToSend); // Send the byte via SerialCommModelView
        }

        // Simulate getting the node address (to represent local panel)
        private byte ConfGetNodeAddress()
        {
            return DESTINATION_PC; // Placeholder to simulate node address
        }
    }

}
