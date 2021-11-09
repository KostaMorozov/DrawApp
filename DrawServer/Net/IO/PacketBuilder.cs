using System;
using System.IO;
using System.Text;

namespace DrawServer.Net.IO
{
    public class PacketBuilder
    {
        MemoryStream _ms;
        public PacketBuilder()
        {
            _ms = new MemoryStream();
        }
        public void WriteOpCode(byte opcode)
        {
            _ms.WriteByte(opcode);
        }
        public void WriteMessage(string msg)
        {
            var msgLength = msg.Length;
            _ms.Write(BitConverter.GetBytes(msgLength));
            _ms.Write(Encoding.ASCII.GetBytes(msg));
        }
        public void WriteCanvasStrokes(byte[] strokes)
        {
            _ms.Write(BitConverter.GetBytes(strokes.Length));
            _ms.Write(strokes);
        }
        public Byte[] GetPacketBytes()
        {
            return _ms.ToArray();
        }
    }
}
