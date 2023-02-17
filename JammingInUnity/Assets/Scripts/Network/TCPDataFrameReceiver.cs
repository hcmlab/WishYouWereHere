using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

/// <summary>
/// TCP server component that listens for a configurable amount of bytes (a frame) at a regular interval (expectedFramesPerSecond).
/// Exposes the data to other components via the properties defined in INetworkDataFrameProvider.
/// 
/// Start() starts the server, which calls a callback as soon as a client connetcs
/// 
/// Update() checks whether a new client has connected since the last Update() call. If it is, a coroutine is started that triggers an async listen for data.
///     This async listen for data triggers a callback once data was received and within that callback it is checked whether a full frame
///     (as configured with "bytesPerFrame") has been received. If yes, the data is copied to the m_lastCompleteFrame buffer. If no, another async listen is
///     triggered -> this recursion goes on until a complete frame was received or no data was received.
///     
///     If no data was received during a read by the underlying .net socket, the connection is closed.
///     WARNING: in the function CloseClientConnection() there is still a bug which prevents the server from accepting new clients after a disconnect.
///     You have to restart the Game to accept new clients. This is WIP and should hopefully be fixed soon.
/// 
/// Adapted from https://github.com/EricBatlle/SimpleUnityTCP/blob/master/Assets/Scripts/Server.cs
/// </summary>
public class TCPDataFrameReceiver : MonoBehaviour, INetworkDataFrameProvider
{
    #region Public Variables
    [Header("Network")]
    public string ipAdress = "127.0.0.1";
    public int port = 8888;

    [Header("Frame properties")]
    public float expectedFramesPerSecond = 30.0f;
    public int bytesPerFrame = 1500000;
    //public float ReceiveTimeoutMs = 90.0f;

    [Header("Debug")]
    public bool printFrameReceiveTimings = true;
    public bool printFirstBytesAsFloat = true;
    #endregion

    #region  Private members
    private TcpListener m_Server = null;
    private TcpClient m_Client = null;
    private NetworkStream m_NetStream = null;
    private byte[] m_receiveBuffer = null;
    private byte[] m_lastCompleteFrame = null;
    private int m_receiveOffset = 0;
    private bool m_receivingFrame = false;
    private bool m_firstFrame = false;
    private System.Diagnostics.Stopwatch m_frameStopwatch = new System.Diagnostics.Stopwatch();
    private float m_firstFrameTimeout = 1;
    private IEnumerator m_ListenClientMsgsCoroutine = null;

    private bool m_stopListening = false;
    #endregion

    #region Callbacks
    [HideInInspector]
    public Action OnServerStarted = null;    //Delegate triggered when server start
    [HideInInspector]
    public Action OnServerClosed = null;     //Delegate triggered when server close
    [HideInInspector]
    public Action OnClientConnected = null;  //Delegate triggered when the server stablish connection with client
    [HideInInspector]
    public Action onCompleteFrameReceived = null;  //Delegate triggered when the server has a complete frame in its buffer connection with client
    #endregion

    long nanosecPerTick = (1000L* 1000L * 1000L) / System.Diagnostics.Stopwatch.Frequency;

    #region implement INetworkDataFrameProvider
    public byte[] CurrentFrame
    {
        get
        {
            return m_lastCompleteFrame;
        }
    }

    public bool Connected
    {
        get
        {
            return m_Client != null && m_Client.Connected;
        }
    }

    public void addOnCompleteFrameReceivedHandler(Action handler)
    {
        Debug.Log("Added handler for frame received");
        onCompleteFrameReceived += handler;
    }
    #endregion

    private void Start()
    {
        m_receiveBuffer = new byte[bytesPerFrame];
        m_lastCompleteFrame = new byte[bytesPerFrame];

        m_firstFrameTimeout = Math.Max(0.001f, (float)((1.0 / expectedFramesPerSecond) * 1.1f)); // wait a little longer on first frame

        StartServer();
    }
    
    private void OnDestroy()
    {
        CloseServer();
    }

    //Start server and wait for clients
    protected virtual void StartServer()
    {
        //Set and enable Server 
        IPAddress ip = IPAddress.Parse(ipAdress);
        m_Server = new TcpListener(ip, port);
        m_Server.Start();
        Debug.Log("Server Started");
        //Wait for async client connection 
        m_Server.BeginAcceptTcpClient(ClientConnected, null);
        OnServerStarted?.Invoke();
    }

    //Check if any client trys to connect
    private void Update()
    {
        if (m_stopListening)
        {
            CloseClientConnection();
            resetConnectionVariables();
            Debug.Log("Accept new client connection");
            m_Server.BeginAcceptTcpClient(ClientConnected, null);
        }

        //new client arrived
        if (m_Client != null && m_ListenClientMsgsCoroutine == null)
        {
            Debug.Log("Start Listening");
            //Start Listening Client Messages coroutine
            m_ListenClientMsgsCoroutine = ListenClientMessages();
            StartCoroutine(m_ListenClientMsgsCoroutine);
        }
    }

    //Callback called when "BeginAcceptTcpClient" detects new client connection
    private void ClientConnected(IAsyncResult res)
    {
        Debug.Log("Client has connected");
        //set the client reference
        m_Client = m_Server.EndAcceptTcpClient(res);
        OnClientConnected?.Invoke();
    }

    #region Communication Server<->Client
    //Coroutine that checks for new frames from the client every m_receivTimeout seconds
    private IEnumerator ListenClientMessages()
    {
        //Establish Client NetworkStream information
        m_NetStream = m_Client.GetStream();

        while (!m_stopListening && m_NetStream != null && m_Client != null && m_Client.Connected)
        {
            //start listening for the next frame if we're not still waiting on the last one
            //if (!m_receivingFrame || m_receiveOffset > 0)
            if (!m_receivingFrame)
            {
                Debug.Log("Waiting for next frame");

                m_receivingFrame = true;
                m_receiveOffset = 0;
                m_frameStopwatch.Start();
                
                if (m_firstFrame)
                {
                    m_firstFrame = false;
                    m_NetStream.BeginRead(m_receiveBuffer, 0, m_receiveBuffer.Length, BytesReceived, m_NetStream); 
                    yield return new WaitForSeconds(m_firstFrameTimeout);
                    
                }
                else
                {
                    m_NetStream.BeginRead(m_receiveBuffer, 0, m_receiveBuffer.Length, BytesReceived, m_NetStream);
                    yield return new WaitForSeconds(0.001f);
                }
                
            } else
            {
                Debug.Log("Waiting for current frame to be complete");
                //only wait a little bit to quickly check again if the last frame has arrived completely by then
                yield return new WaitForSeconds(0.001f);
            }
        }
    }

    //AsyncCallback called when "BeginRead" is ended, handling the message response from client
    private void BytesReceived(IAsyncResult result)
    {
        if (m_stopListening) return;

        if (result.IsCompleted && m_Client.Connected)
        {
            //build message received from client
            int bytesReceived = m_NetStream.EndRead(result);

            if(bytesReceived <= 0)
            {
                Debug.LogError("No bytes received from client. Disconnecting");
                m_stopListening = true;
                return;
            }

            if (m_receiveOffset + bytesReceived >= bytesPerFrame)
            {
                //we've received a complete frame
                
                m_receiveOffset = 0;


                if (printFrameReceiveTimings)
                {
                    m_frameStopwatch.Stop();
                    Debug.Log("Received frame (" + (m_frameStopwatch.ElapsedTicks * nanosecPerTick / 1000.0f) + "microseconds)!");
                    m_frameStopwatch.Reset();
                }


                if (printFirstBytesAsFloat)
                {
                    Debug.Log("Frame value: " + BitConverter.ToSingle(m_receiveBuffer, 0));
                }

                m_receiveBuffer.CopyTo(m_lastCompleteFrame, 0);
                Debug.Log("Frame received, firing event.");
                onCompleteFrameReceived?.Invoke();

                m_receivingFrame = false;

            } else
            {
                if (printFrameReceiveTimings)
                {
                    m_frameStopwatch.Stop();
                    Debug.Log("Received only " + bytesReceived + " bytes at " + (m_frameStopwatch.ElapsedTicks * nanosecPerTick / 1000.0f) + "microseconds)!");
                    m_frameStopwatch.Reset();
                }
                //the frame has not arrived completely -> listen for the remaining bytes
                m_receiveOffset += bytesReceived;
                int bytesRemaining = m_receiveBuffer.Length - m_receiveOffset;
                m_NetStream.BeginRead(m_receiveBuffer, m_receiveOffset, bytesRemaining, BytesReceived, m_NetStream); // ATTENTION: Async recursion!
            }
        } else
        {
            m_stopListening = true;
            Debug.LogError("Error during Bytes Received");
        }
    }
    #endregion    

    #region Close Server/ClientConnection
    //Close client connection and disables the server
    protected virtual void CloseServer()
    {
        Debug.Log("Server Closed");
        //Close client connection
        if (m_Client != null)
        {
            m_NetStream.Close();
            m_NetStream = null;
            m_Client.Close();
            m_Client = null;
        }
        //Close server connection
        if (m_Server != null)
        {
            m_Server.Stop();
            m_Server = null;
        }

        OnServerClosed?.Invoke();
    }

    //Close connection with the current client
    protected virtual void CloseClientConnection()
    {
        Debug.Log("Close Connection with Client");
        //Reset everything to defaults
        StopCoroutine(m_ListenClientMsgsCoroutine);
        m_NetStream.Close();
        m_Client.Close();
        m_ListenClientMsgsCoroutine = null;
        m_Client = null;
        m_NetStream = null;
    }
    #endregion

    private void resetConnectionVariables()
    {
        m_receivingFrame = false;
        m_receiveOffset = 0;
        m_stopListening = false;
    }
}