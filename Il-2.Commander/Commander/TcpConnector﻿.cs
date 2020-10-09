using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace Il_2.Commander.Commander
{
    class TcpConnector﻿ : IDisposable﻿
    {
        T﻿cpClient connection;
        NetworkStream stream;
        string host;
        ushort port;

        public TcpConnecto﻿r(string host, ushort port)
        {
            // TODO: Complete memb﻿er initialization
            this.host = host;
            this.port = port;
            connection = new TcpClient(host, (int)port);
            stream = connection.GetStream();
        }

        public string ExecuteCommand(string command)
        {
            Byte[] sendBytes = Encoding.UTF8.GetBytes(String.Concat(command));
            Byte[] length = BitConverter.GetBytes((ushort)(command.Length + 1));
            Byte[] zero = { 0 };
            Byte[] packet = length.Concat(sendBytes).Concat(zero).ToArray();

            stream.Write(packet, 0, packet.Length);

            packet = new Byte[connection.ReceiveBufferSize];

            Int32 bytes = stream.Read(packet, 0, packet.Length);

            UInt16 responseLength = BitConverter.ToUInt16(packet.Take(2).ToArray(), 0);

            string response = null;
            if (responseLength > 2)
                response = Encoding.UTF8.GetString(packet.Skip(2).Take((int)responseLength - 1).ToArray());
            //else
            //throw new Exception(BitConverter.ToString(packet));
            return response;
        }
        public int RecieveBufferSize { get { return connection.ReceiveBufferSize; } }
        public void Dispose()
        {
            stream.Close();
            connection.Close();
        }
    }
}
