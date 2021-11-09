using DrawServer.Net.IO;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace DrawServer
{
    public class Client
    {
        public string UserName { get; set; }
        public Guid UID { get; set; }
        public TcpClient ClientSocket { get; set; }

        private PacketReader _packetReader;
        public Client(TcpClient client)
        {
            ClientSocket = client;

            _packetReader = new PacketReader(ClientSocket.GetStream());

            UID = Guid.NewGuid();
            UserName = _packetReader.ReadMessage();

            Console.WriteLine($"{UserName} has connected");

            Task.Run(() => Process());
        }
        void Process()
        {
            while (true)
            {
                try
                {
                    var opcode = _packetReader.ReadByte();
                    switch (opcode)
                    {
                        case 5:
                            var msg = _packetReader.ReadMessage();
                            Program.BrodcastMessage(msg);
                            Console.WriteLine($"Message received {msg}");
                            Program.BrodcastMessage($"[{UserName}] : {msg}");
                            break;
                        case 7:
                            var ink = _packetReader.ReadCanvasStrokes();
                            Console.WriteLine("Canvas Strokes received");
                            Program.BrodcastCanvasStrokes(ink);
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine($"[{UID.ToString()}] Disconnected");
                    Program.BrodcastDisconnect(UID.ToString());
                    ClientSocket.Close();
                    break;
                }
            }
        }
    }
}
