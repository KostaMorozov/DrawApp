using DrawServer.Net.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace DrawServer
{
    class Program
    {
        static List<Client> _users;
        static TcpListener _listener;
        static void Main(string[] args)
        {
            _users = new List<Client>();
            _listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 7891);
            _listener.Start();
            while (true)
            {
                var client = new Client(_listener.AcceptTcpClient());
                Console.WriteLine("Client has connected");
                _users.Add(client);
                BrodcastConnection();
            } 
        }
        static void BrodcastConnection()
        {
            foreach (var user in _users)
            {
                foreach (var usr in _users)
                {
                    var brodcastPacket = new PacketBuilder();
                    brodcastPacket.WriteOpCode(1);
                    brodcastPacket.WriteMessage(usr.UserName);
                    brodcastPacket.WriteMessage(usr.UID.ToString());
                    user.ClientSocket.Client.Send(brodcastPacket.GetPacketBytes());
                }
            }
        }
        public static void BrodcastMessage(string message)
        {
            foreach (var user in _users)
            {
                var msgPacket = new PacketBuilder();
                msgPacket.WriteOpCode(5);
                msgPacket.WriteMessage(message);
                user.ClientSocket.Client.Send(msgPacket.GetPacketBytes());
            }
        }
        public static void BrodcastCanvasStrokes(byte[] strokes)
        {
            foreach (var user in _users)
            {
                var strokesPacket = new PacketBuilder();
                strokesPacket.WriteOpCode(7);
                strokesPacket.WriteCanvasStrokes(strokes);
                user.ClientSocket.Client.Send(strokesPacket.GetPacketBytes());
            }
        }
        public static void BrodcastDisconnect(string uid)
        {
            var disconnectedUser = _users.Where(u => u.UID.ToString() == uid).FirstOrDefault();
            _users.Remove(disconnectedUser);
            foreach (var user in _users)
            {
                var brodcastPacket = new PacketBuilder();
                brodcastPacket.WriteOpCode(10);
                brodcastPacket.WriteMessage(uid);
                user.ClientSocket.Client.Send(brodcastPacket.GetPacketBytes());
            }
            BrodcastMessage($"[{disconnectedUser.UserName}] Disconnected");
        }
    }

}
