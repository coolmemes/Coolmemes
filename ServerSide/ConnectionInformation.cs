using System;
using System.Net.Sockets;
using SharedPacketLib;
using System.IO;
using System.Text;
using Azure.Encryption.Hurlant.Crypto.Prng;

namespace ConnectionManager
{
    public class ConnectionInformation : IDisposable
    {
        #region declares
        /// <summary>
        /// The socket this connection is based upon
        /// </summary>
        private readonly Socket dataSocket;

        /// <summary>
        /// The ip of this connection
        /// </summary>
        private readonly string ip;

        /// <summary>
        /// The id of this connection
        /// </summary>
        private readonly int connectionID;

        /// <summary>
        /// Boolean indicating of this instance is connected to the user or not
        /// </summary>
        private bool isConnected;

        /// <summary>
        /// The ar c4 server side
        /// </summary>
        public ARC4 ARC4ServerSide;

        /// <summary>
        /// The ar c4 client side
        /// </summary>
        public ARC4 ARC4ClientSide;

        /// <summary>
        /// Is used when a connection state changes
        /// </summary>
        /// <param name="state">The new state of the connection</param>
        public delegate void ConnectionChange(ConnectionInformation information, ConnectionState state);
        /// <summary>
        /// Is triggered when the user connects/disconnects
        /// </summary>
        public event ConnectionChange connectionChanged;

        /// <summary>
        /// Buffer of the connection
        /// </summary>
        private readonly byte[] buffer;

        /// <summary>
        /// This item contains the data parser for the connection
        /// </summary>
        public IDataParser parser { get; set; }

        private readonly AsyncCallback sendCallback;

        public static bool disableSend = false;
        public static bool disableReceive = false;

        #endregion

        #region constructor

        /// <summary>
        /// Creates a new Connection witht he given information
        /// </summary>
        /// <param name="dataStream">The Socket of the connection</param>
        /// <param name="connectionID">The id of the connection</param>
        public ConnectionInformation(Socket dataStream, int connectionID, IDataParser parser, string ip)
        {
            this.parser = parser;
            buffer = new byte[GameSocketManagerStatics.BUFFER_SIZE];
            dataSocket = dataStream;
            dataSocket.SendBufferSize = GameSocketManagerStatics.BUFFER_SIZE;
            this.ip = ip;
            this.connectionID = connectionID;
            sendCallback = sentData;

            if (connectionChanged != null)
                connectionChanged.Invoke(this, ConnectionState.open);

            //MessageLoggerManager.AddMessage(null, connectionID, LogState.ConnectionOpen);
        }

        /// <summary>
        /// Starts this item packet processor
        /// MUST be called before sending data
        /// </summary>
        public void startPacketProcessing()
        {
            if (!isConnected)
            {
                isConnected = true;
                //Out.writeLine("Starting packet processsing of client [" + this.connectionID + "]", Out.logFlags.lowLogLevel);
                try
                {
                    dataSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, incomingDataPacket, dataSocket);
                }
                catch
                {
                    disconnect();
                }
            }
        }

        #endregion

        #region getters

        /// <summary>
        /// Returns the ip of the current connection
        /// </summary>
        /// <returns>The ip of this connection</returns>
        /// 
        public string getIp()
        {
            return ip;
        }

        /// <summary>
        /// Returns the connection id
        /// </summary>
        /// <returns>The id of the connection</returns>
        public int getConnectionID()
        {
            return connectionID;
        }

        #endregion

        #region methods

        #region connection management
        /// <summary>
        /// Disconnects the current connection
        /// </summary>
        internal void disconnect()
        {
            try
            {
                if (isConnected)
                {
                    isConnected = false;
                    //MessageLoggerManager.AddMessage(null, connectionID, LogState.ConnectionClose);

                    //Out.writeLine("Connection [" + this.connectionID + "] has been disconnected", Out.logFlags.BelowStandardlogLevel);
                    try
                    {
                        if (dataSocket != null && dataSocket.Connected)
                        {
                            dataSocket.Shutdown(SocketShutdown.Both);
                            dataSocket.Close();
                        }
                    }
                    catch { }

                    if (dataSocket != null) dataSocket.Dispose();
                    parser.Dispose();

                    try
                    {
                        if (connectionChanged != null)
                            connectionChanged.Invoke(this, ConnectionState.closed);
                    }
                    catch
                    {
                        //Out.writeError(Ex.ToString(), Out.logFlags.ImportantLogLevel);

                        //Out.writeSeriousError(Ex.ToString());
                    }
                    connectionChanged = null;
                }
                else
                {
                    //Out.writeLine("Connection [" + this.connectionID + "] has already been disconnected - ignoring disconnect call", Out.logFlags.BelowStandardlogLevel);
                }
            }
            catch { }
        }


        /// <summary>
        /// Disposes the current item
        /// </summary>
        public void Dispose()
        {
            if (isConnected)
            {
                disconnect();
            }
        }
        #endregion

        #region data receiving

        /// <summary>
        /// Receives a packet of data and processes it
        /// </summary>
        /// <param name="iAr">The interface of an async result</param>
        private void incomingDataPacket(IAsyncResult iAr)
        {
            try
            {
                //The amount of bytes received in the packet
                int bytesReceived = dataSocket.EndReceive(iAr);

                if (bytesReceived == 0)
                {
                    disconnect();
                    return;
                }

                if (!disableReceive)
                {
                    byte[] packet = new byte[bytesReceived];
                    Buffer.BlockCopy(buffer, 0, packet, 0, bytesReceived);

                    handlePacketData(packet);
                }

                dataSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, incomingDataPacket, dataSocket);

                if (bytesReceived == 0)
                    disconnect();
            }
            catch
            {
                disconnect();
            }
        }

        /// <summary>
        /// Handles packet data
        /// </summary>
        /// <param name="packet">The data received by the </param>
        private void handlePacketData(byte[] packet)
        {
            if (parser == null)
                return;

            if (ARC4ServerSide != null)
                ARC4ServerSide.Parse(ref packet);

            parser.handlePacketData(packet);
        }

        #endregion

        #region data sending
        /// <summary>
        /// Sends data to this instance
        /// </summary>
        /// <param name="Data">The data to be send</param>
        //public void sendData(ServerOutgoingPacket data)
        //{
        //    try
        //    {
        //        Out.writeLine("Sending packet [" + data.getOpCode().ToString() + "] to client [" + this.connectionID + "]", Out.logFlags.BelowStandardlogLevel);
        //        byte[] dataBytes = data.GetPacketData();
        //        this.dataSocket.BeginSend(dataBytes, 0, dataBytes.Length, 0, new AsyncCallback(sentData), this.dataSocket);
                
        //    }
        //    catch
        //    {
        //        disconnect();
        //    }
        //}

        public void SendData(byte[] packet)
        {
            if (packet.Length <= 0)
                return;
            sendData(packet);
        }

        public void SendMuchData(byte[] packet)
        {
            if (packet.Length <= 0)
                return;
            //Console.WriteLine("Sended a lot of packets: " + Encoding.Default.GetString(packet).Replace(Convert.ToChar(0).ToString(), "[0]"));
            sendData(packet);
        }

        public void sendbData(byte[] packet)
        {
            if (packet.Length <= 0)
                return;
            sendData(packet);
        }

        //private byte[] lastSent;
        private void sendData(byte[] packet)
        {
            try
            {
                //lastSent = packet;

                //if (logShit)
                //{
                    //string packetData = System.Text.Encoding.Default.GetString(packet);

                    //StackTrace stackTrace = new StackTrace();
                    ////Console.WriteLine(stackTrace.ToString());
                    //Console.ForegroundColor = ConsoleColor.Green;
                    //Console.WriteLine(string.Format("Data from server => [{0}]", packetData));
                    //Console.ForegroundColor = ConsoleColor.White;
                //}
                
                //Out.writeLine("Sending byte packet of length [" + packet.Length + "] to client [" + this.connectionID + "]", Out.logFlags.BelowStandardlogLevel);
                //this.dataSocket.BeginSend(packet, 0, packet.Length, 0, sendCallback, null);
                if (packet.Length <= 0)
                    return;
                SendUnsafeData(packet);
            }
            catch
            {
                disconnect();
            }
        }

        public void SendUnsafeData(byte[] packet)
        {
            if (!isConnected || disableSend)
                return;
            //string packetData = System.Text.Encoding.Default.GetString(packet);
            //Console.WriteLine(string.Format("Data from server => [{0}]", packetData));
            if (packet.Length <= 0)
                return;

            if (ARC4ClientSide != null) 
                ARC4ClientSide.Parse(ref packet);

            dataSocket.BeginSend(packet, 0, packet.Length, 0, sendCallback, null);
        }

        /// <summary>
        /// Same as sendData
        /// </summary>
        /// <param name="iAr">The a-synchronious interface</param>
        private void sentData(IAsyncResult iAr)
        {
            try
            {
                dataSocket.EndSend(iAr);
            }
            catch
            {
                //Console.WriteLine("0x004: Socket send"); disconnect();
                disconnect();
            }
        }

        private void LogMessage(string message)
        {
            try
            {
                var errWriter = new FileStream("packetlog.txt", FileMode.Append, FileAccess.Write);
                var Msg = ASCIIEncoding.ASCII.GetBytes(Environment.NewLine + message);
                errWriter.Write(Msg, 0, Msg.Length);
                errWriter.Dispose();
            }
            catch
            {
                Console.WriteLine("UNABLE TO WRITE TO LOGFILE");
            }
        }
        #endregion

        #endregion
    }
}
