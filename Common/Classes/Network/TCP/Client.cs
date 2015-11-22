using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Common.Classes.Network.TCP
{
    public class Client : IDisposable
    {
        #region Field

        private readonly long UNIQUE_CID;
        private bool CLIENT_STATUS = false;
        private bool LOG;

        private IPEndPoint Base_IPEndPoint;
        private Socket Base_Socket;
        private NetworkStream Base_NetworkStream;

        private IPAddress HOST;
        private uint PORT;

        #region Limit Section

        /// <summary>
        /// Unit : KB
        /// 1,024 = 1KB
        /// 1,024,000 = 1MB (1024 * 1000)
        /// </summary>

        private uint MAXIMUM_DOWNLOAD_SIZE;
        private uint MAXIMUM_UPLOAD_SIZE;

        private uint RECEIVE_BUFFER_SIZE;
        private uint SEND_BUFFER_SIZE;

        #endregion

        private uint HEARTBEAT_INTERVAL = 1500; //Milliseconds

        #endregion

        public Client(IPAddress Host, uint Port, uint MaximumDownloadSize = (1024 * 1000), uint MaximumUploadSize = (1024 * 1000), uint ReceiveBufferSize = (1024 * 1000), uint SendBufferSize = (1024 * 1000), bool Log = false)
        {
            this.UNIQUE_CID = Network.AssignUIC();
            this.HOST = Host;
            this.PORT = Port;
            this.Base_IPEndPoint = new IPEndPoint(Host, (int)Port);
            this.MAXIMUM_DOWNLOAD_SIZE = MaximumDownloadSize;
            this.MAXIMUM_UPLOAD_SIZE = MaximumUploadSize;
            this.RECEIVE_BUFFER_SIZE = ReceiveBufferSize;
            this.SEND_BUFFER_SIZE = SendBufferSize;
            this.LOG = Log;
        }

        private void Init()
        {
            Base_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IPv4);
            Base_Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, false);
            Base_Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 250);
            Base_Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 250);

            if(LOG)
            {

            }
        }

        public void Connect()
        {
            try
            {
                Dispose();
                Init();
                Task.Factory.StartNew(() => Receive());
            }
            catch (Exception _EXCEPTION)
            {
                Clear();

                if (LOG)
                {
                }
            }
        }

        private void CheckHeartbeat()
        {
            while(CLIENT_STATUS)
            {
                Send(Network.PACKET_HEARTBEAT);
                Task.Factory.CancellationToken.WaitHandle.WaitOne((int)HEARTBEAT_INTERVAL);
            }
        }

        #region Get or Set Methods

        public bool GetClientStatus()
        {
            return CLIENT_STATUS;
        }

        #endregion

        public void Clear()
        {
            CLIENT_STATUS = false;

            if (Base_NetworkStream != null)
            {
                Base_NetworkStream.Close();
            }
            if (Base_Socket != null)
            {
                Base_Socket.Close();
            }
        }

        public void Dispose()
        {
            Clear();
            GC.SuppressFinalize(this);
        }

        #region Network Process

        private void BeginConnect(IAsyncResult IAR)
        {
            try
            {
                Base_Socket = (Socket)IAR.AsyncState;
                Base_Socket.EndConnect(IAR);
                Base_NetworkStream = new NetworkStream(Base_Socket);
                CLIENT_STATUS = true;
            }
            catch (Exception _EXCEPTION)
            {
                Clear();

                if(LOG)
                {

                }
            }
        }

        private void Receive()
        {
            try
            {
                while (CLIENT_STATUS && Base_Socket.Connected)
                {
                    if (Base_NetworkStream.CanRead && Base_NetworkStream.DataAvailable)
                    {
                        using (MemoryStream MS = new MemoryStream())
                        {
                            byte[] RECEIVE_BUFFER = new byte[RECEIVE_BUFFER_SIZE];
                            int NumberOfBytesRead = 0;
                            int WrittenSize = 0;

                            do
                            {
                                NumberOfBytesRead = Base_NetworkStream.Read(RECEIVE_BUFFER, 0, RECEIVE_BUFFER.Length);
                                MS.Write(RECEIVE_BUFFER, 0, NumberOfBytesRead);
                                WrittenSize += NumberOfBytesRead;

                                if (WrittenSize > MAXIMUM_DOWNLOAD_SIZE)
                                {
                                    break;
                                }

                            } while (Base_NetworkStream.DataAvailable);

                            if (MAXIMUM_DOWNLOAD_SIZE >= WrittenSize)
                            {
                                Network.ClientPacketDictionary.TryAdd(UNIQUE_CID, new Packet(DateTime.Now, 0, MS.Length, MS.ToArray()));
                            }
                        }
                    }
                }
            }
            catch (Exception _EXCEPTION)
            {
                Clear();

                if(LOG)
                {

                }
            }
        }

        public void Send(byte[] Data)
        {
            try
            {
                Task.Factory.StartNew(() =>
                {
                    if(CLIENT_STATUS && Base_Socket.Connected && Base_NetworkStream.CanWrite)
                    {
                        if(Data.Length <= MAXIMUM_UPLOAD_SIZE)
                        {
                            if(Data.Length < SEND_BUFFER_SIZE)
                            {
                                Base_Socket.Send(Data);
                            }
                            else
                            {
                                using (MemoryStream MS = new MemoryStream(Data))
                                {
                                    int NumberOfBytesRead = 0;

                                    while((NumberOfBytesRead = MS.Read(Data, 0, (int)SEND_BUFFER_SIZE)) > 0)
                                    {
                                        Base_Socket.Send(Data);
                                        Task.Factory.CancellationToken.WaitHandle.WaitOne(1);
                                    }
                                }
                            }
                        }

                        Task.Factory.CancellationToken.WaitHandle.WaitOne(35);
                    }
                });
            }
            catch (Exception _EXCEPTION)
            {
                Clear();

                if(LOG)
                {

                }
            }
        }

        #endregion
    }
}
