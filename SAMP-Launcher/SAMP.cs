using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows;

namespace SAMP
{
    public class SAMP : IDisposable
    {
        Socket qSocket;
        IPAddress address;
        int _port = 0;
        string _password = null;
        string[] results = new string[50];
        int _count = 0;

        public SAMP(string addr, int port, string password, bool dns)
        {
            qSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            qSocket.SendTimeout = 5000;
            qSocket.ReceiveTimeout = 5000;
            if (dns)
            {
                try
                {
                    address = Dns.GetHostAddresses(addr)[0];
                }
                catch
                {
                }
            }
            else
            {
                try
                {
                    address = IPAddress.Parse(addr);
                }
                catch
                {
                }
            }

            _port = port;
            _password = password;
        }

        public bool Send(string command)
        {
            try
            {
                IPEndPoint endpoint = new IPEndPoint(address, _port);
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        writer.Write("SAMP".ToCharArray());
                        string[] SplitIP = address.ToString().Split('.');
                        writer.Write(Convert.ToByte(SplitIP[0]));
                        writer.Write(Convert.ToByte(SplitIP[1]));
                        writer.Write(Convert.ToByte(SplitIP[2]));
                        writer.Write(Convert.ToByte(SplitIP[3]));
                        writer.Write((ushort)_port);
                        writer.Write('x');
                        writer.Write((ushort)_password.Length);
                        writer.Write(_password.ToCharArray());
                        writer.Write((ushort)command.Length);
                        writer.Write(command.ToCharArray());
                    }
                    if (qSocket.SendTo(stream.ToArray(), endpoint) > 0) return true;
                }
            }
            catch
            {
                return false;
            }
            return false;
        }

        public int Receive()
        {
            try
            {
                for (int i = 0; i < results.GetLength(0); i++) results.SetValue(null, i);
                _count = 0;
                EndPoint endpoint = new IPEndPoint(address, _port);
                byte[] rBuffer = new byte[500];
                int count = qSocket.ReceiveFrom(rBuffer, ref endpoint);
                using (MemoryStream stream = new MemoryStream(rBuffer))
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        if (stream.Length <= 11) return _count;
                        reader.ReadBytes(11);
                        short len;
                        try
                        {
                            while ((len = reader.ReadInt16()) != 0) results[_count++] = new string(reader.ReadChars((int)len));
                        }
                        catch
                        {
                            return _count;
                        }
                    }
                }
            }
            catch
            {
                return _count;
            }
            return _count;
        }

        public string[] Store(int count = -1)
        {
            string[] rString = new string[count != -1 ? count : _count];
            for (int i = 0; (i < count || count == -1) && i < _count; i++) rString[i] = results[i];
            _count = 0;
            return rString;
        }

        public void Dispose()
        {
            try
            {
                qSocket.Dispose();
            }
            catch
            {
            }
        }
    }

    public class Query : IDisposable
    {
        Socket qSocket;
        IPAddress address;
        int _port = 0;
        string[] results;
        int _count = 0;
        DateTime[] timestamp = new DateTime[2];

        public string passworded;
        public string players;
        public string max_players;
        public string hostname;
        public string gamemode;
        public string mapname;

        public enum PaketOpcode
        {
            Info = 'i',
            Rules = 'r',
            ClientList = 'c',
            DetailedClientList = 'd',
            Ping = 'p'
        }

        public Query(string addr, int port, bool dns)
        {
            qSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            qSocket.SendTimeout = 5000;
            qSocket.ReceiveTimeout = 5000;
            if (dns)
            {
                try
                {
                    address = Dns.GetHostAddresses(addr)[0];
                }
                catch (Exception e)
                {
                    MessageBox.Show("An error has occured in SAMP API!\n\n" + e.ToString());
                    Environment.Exit(2);
                }
            }
            else
            {
                try
                {
                    address = IPAddress.Parse(addr);
                }
                catch
                {
                }
            }
            _port = port;
        }

        public bool Send(char opcode, string sign = "1337")
        {
            try
            {
                EndPoint endpoint = new IPEndPoint(address, _port);
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        writer.Write("SAMP".ToCharArray());
                        string[] SplitIP = address.ToString().Split('.');
                        writer.Write(Convert.ToByte(SplitIP[0]));
                        writer.Write(Convert.ToByte(SplitIP[1]));
                        writer.Write(Convert.ToByte(SplitIP[2]));
                        writer.Write(Convert.ToByte(SplitIP[3]));
                        writer.Write((ushort)_port);
                        writer.Write(opcode);
                        if (opcode == 'p') writer.Write(sign.ToCharArray());
                        timestamp[0] = DateTime.Now;
                    }
                    if (qSocket.SendTo(stream.ToArray(), endpoint) > 0) return true;
                }
            }
            catch
            {
                return false;
            }
            return false;
        }

        public int Receive()
        {
            try
            {
                _count = 0;
                EndPoint endpoint = new IPEndPoint(address, _port);
                byte[] rBuffer = new byte[500];
                qSocket.ReceiveFrom(rBuffer, ref endpoint);
                timestamp[1] = DateTime.Now;
                using (MemoryStream stream = new MemoryStream(rBuffer))
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        if (stream.Length <= 10) return _count;
                        reader.ReadBytes(10);
                        switch (reader.ReadChar())
                        {
                            case 'i':
                                {
                                    results = new string[6];
                                    passworded = reader.ReadByte().ToString();
                                    //results[_count++] = reader.ReadByte().ToString(); // either 0 or 1, depending whether if the password has been set.
                                    players = reader.ReadInt16().ToString();
                                    //results[_count++] = reader.ReadInt16().ToString(); // current amount of players online on the server
                                    max_players = reader.ReadInt16().ToString();
                                    //results[_count++] = reader.ReadInt16().ToString(); // maximum amount of players that can join the server
                                    hostname = new string(reader.ReadChars(reader.ReadInt32()));
                                    //results[_count++] = new string(reader.ReadChars(reader.ReadInt32())); // hostname
                                    gamemode = new string(reader.ReadChars(reader.ReadInt32()));
                                    //results[_count++] = new string(reader.ReadChars(reader.ReadInt32())); // gamemode
                                    mapname = new string(reader.ReadChars(reader.ReadInt32()));
                                    //results[_count++] = new string(reader.ReadChars(reader.ReadInt32())); // mapname
                                    return 6;
                                }

                            case 'r':
                                {
                                    int rulecount = reader.ReadInt16();
                                    results = new string[rulecount * 2];
                                    for (int i = 0; i < rulecount; i++)
                                    {
                                        results[_count++] = new string(reader.ReadChars(reader.ReadByte())); // rule name (key)
                                        results[_count++] = new string(reader.ReadChars(reader.ReadByte())); // rule value (value)
                                    }
                                    return _count;
                                }

                            case 'c':
                                {
                                    int playercount = reader.ReadInt16();
                                    results = new string[playercount * 2];
                                    for (int i = 0; i < playercount; i++)
                                    {
                                        results[_count++] = new string(reader.ReadChars(reader.ReadByte())); // nickname
                                        results[_count++] = reader.ReadInt32().ToString(); // score
                                    }
                                    return _count;
                                }

                            case 'd':
                                {
                                    int playercount = reader.ReadInt16();
                                    results = new string[playercount * 4];
                                    for (int i = 0; i < playercount; i++)
                                    {
                                        results[_count++] = reader.ReadByte().ToString(); //playerid
                                        results[_count++] = new string(reader.ReadChars(reader.ReadByte())); //nick
                                        results[_count++] = reader.ReadInt32().ToString(); //score
                                        results[_count++] = reader.ReadInt32().ToString(); //ping
                                    }
                                    return _count;
                                }

                            case 'p':
                                {
                                    results = new string[1];
                                    results[_count++] = timestamp[1].Subtract(timestamp[0]).Milliseconds.ToString(); // time difference
                                    results[_count++] = new string(reader.ReadChars(4)); // paket signature
                                    return _count;
                                }

                            default: return _count;
                        }
                    }
                }
            }
            catch
            {
                return _count;
            }
        }

        public string[] Store(int count)
        {
            string[] rString = new string[count];
            for (int i = 0; i < count && i < _count; i++) rString[i] = results[i];
            _count = 0;
            return rString;
        }

        public void Dispose()
        {
            try
            {
                qSocket.Dispose();
            }
            catch
            {
            }
        }
    }
}