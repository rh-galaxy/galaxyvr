using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

using Open.Nat;

public struct HttpJoinInfo
{
    public string szName;
    public string szIP; //internet ip
    //public string szLocalIP; //not needed?, remove
    public int iPort; //port forwarded and local, that we listen on
}

public struct ConnectionInfo
{
    public string szName;
    public string szIP;
    public System.Net.Sockets.TcpClient tcp;
    public System.Net.Sockets.NetworkStream ns;

    //impl of "peek buffer", or gather a full message before processing it
    public int stream_msg_recv_pos;
    public byte[] stream_msg_recv;
}


public class SendRecv
{
    // unity web vars used in the create/join process
    const string WEB_HOST = "https://galaxy-forces-vr.com";

    public List<HttpJoinInfo> oJoinList = new List<HttpJoinInfo>();
    public bool bIsJoinDone = false;
    public bool bIsCreateDone = false;

    UnityWebRequest www;

    //run periodically when game not created (from start) until a game is joined or created
    internal bool bRunJoin = true;
    public IEnumerator UpdateJoin()
    {
        bIsJoinDone = false;
        bIsMaster = false;

        string url = WEB_HOST + "/mpgame_join.php";
        www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            //retrieve results as text
            string szResult = www.downloadHandler.text;
            HttpJoinInfo stJoin = new HttpJoinInfo();

            string[] szLines = szResult.Split((char)10);
            oJoinList.Clear(); //rebuild list

            //parse result
            for (int i = 0; i < szLines.Length; i++)
            {
                //handle spaces in names
                char[] szSeparator = { (char)' ' };
                char[] szSeparator2 = { (char)'\"' };
                string[] szWithin = szLines[i].Trim('\r', '\n').Split(szSeparator2, StringSplitOptions.RemoveEmptyEntries);
                stJoin.szName = szWithin[1].Trim(' ');
                stJoin.szIP = szWithin[3].Trim(' ');

                string[] szPort = szWithin[4].Split(szSeparator, StringSplitOptions.RemoveEmptyEntries);
                stJoin.iPort = int.Parse(szPort[0].Trim(' '));

                oJoinList.Add(stJoin);
            }
        }
        bIsJoinDone = true;
    }

    //run periodically from game created until begin play level
    NatDevice device;
    IPAddress externalIP;
    int iBasePort = 1999;
    internal bool bRunCreate = false;
    public IEnumerator UpdateCreate()
    {
        bIsCreateDone = false;
        bIsMaster = true;

        if (server == null)
        {
            //do upnp setup
            NatDiscoverer discoverer = new NatDiscoverer();
            CancellationTokenSource cts = new CancellationTokenSource(10000);

            bool mapped = false;
            System.Threading.Tasks.Task<NatDevice> task = discoverer.DiscoverDeviceAsync(PortMapper.Upnp, cts);
            while (!task.IsCompleted && !task.IsFaulted && !task.IsCanceled) yield return null;
            if(task.IsCompleted)
            {
                device = task.Result;

                System.Threading.Tasks.Task<IPAddress> task2 = device.GetExternalIPAsync();
                while (!task2.IsCompleted && !task2.IsFaulted) yield return null;
                if (task.IsCompleted)
                {
                    externalIP = task2.Result;

                    for(int i=0; i<20; i++)
                    {
                        System.Threading.Tasks.Task task3 = device.CreatePortMapAsync(new Mapping(Protocol.Tcp, iBasePort, iBasePort + 10, "GFVR"));
                        while (!task3.IsCompleted && !task3.IsFaulted) yield return null;
                        if (task3.IsCompleted)
                        {
                            Debug.Log("Ports "+ iBasePort.ToString() + "+10 mapped on " + externalIP.ToString());
                            mapped = true;
                            break;
                        }
                        iBasePort += 10;
                    }
                }
            }

            IPAddress localIP;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530); //this does nothing on the cable
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                localIP = endPoint.Address;
            }
            if (!mapped)
            {
                externalIP = localIP;
                Debug.Log("Ports not mapped, making the best of it and use IP " + externalIP.ToString());
            }

            //create listening socket
            try
            {
                server = new TcpListener(IPAddress.Any, iBasePort);
                server.Start();
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        string url = WEB_HOST + "/mpgame_create.php?User=" + UnityWebRequest.EscapeURL(GameManager.szUser) +"&IP="+ externalIP.ToString() +"&Port="+ iBasePort.ToString();
        www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            //retrieve results as text
            string szResult = www.downloadHandler.text;

            Debug.Log("Created game as \"" + GameManager.szUser + "\" [" + szResult + "]");
        }

        bIsCreateDone = true;
    }


    // tcp server / clients vars used for actual game messages
    TcpListener server = null;
    // Thread signal
    public static ManualResetEvent tcpClientConnected = new ManualResetEvent(false);

    // Accept one client connection asynchronously.
    public static void DoBeginAcceptTcpClient(TcpListener listener)
    {
        // Set the event to nonsignaled state.
        tcpClientConnected.Reset();

        // Start to listen for connections from a client.
        Console.WriteLine("Waiting for a connection...");

        // Accept the connection.
        // BeginAcceptSocket() creates the accepted socket.
        listener.BeginAcceptTcpClient(
            new AsyncCallback(DoAcceptTcpClientCallback),
            listener);

        // Wait until a connection is made and processed before
        // continuing.
        //tcpClientConnected.WaitOne();
    }

    // Process the client connection.
    public static void DoAcceptTcpClientCallback(IAsyncResult ar)
    {
        // Get the listener that handles the client request.
        TcpListener listener = (TcpListener)ar.AsyncState;

        // End the operation and display the received data on
        // the console.
        TcpClient client = listener.EndAcceptTcpClient(ar);
        client.NoDelay = true;
        if (iNumCI == 3) client.Close();
        else
        {
            ci[iNumCI].tcp = client;
            ci[iNumCI].ns = client.GetStream();
            ci[iNumCI].stream_msg_recv_pos = 0;
            ci[iNumCI].stream_msg_recv = new byte[32];
            iNumCI++;
        }

        // Process the connection here. (Add the client to a
        // server table, read data, etc.)
        Console.WriteLine("Client connected completed");

        // Signal the calling thread to continue.
        tcpClientConnected.Set();
    }



    bool bIsMaster;
    bool bListening; //true while master is listening for joining players
    static int iNumCI = 0;
    static ConnectionInfo[] ci = new ConnectionInfo[3]; //the other players (1..3)

    public SendRecv()
    {
    }

    void ClientCheck()
    {
        //recv data
    }

    void ServerCheck()
    {
        //accept new joining players
        if (!bListening)
        {
            DoBeginAcceptTcpClient(server);
            bListening = true;
        }
        else if (tcpClientConnected.WaitOne(0))
        {
            DoBeginAcceptTcpClient(server);
        }

        //recv data
        for (int i = 0; i < iNumCI; i++)
        {
            if (ci[i].ns.CanRead && ci[i].ns.DataAvailable)
            {
                byte[] buf = ci[i].stream_msg_recv;
                int pos = ci[i].stream_msg_recv_pos;
                int iNumRead = ci[i].ns.Read(buf, pos, buf.Length - pos);
                pos += iNumRead;
                if (pos == buf.Length)
                {
                    pos = 0;
                    //buf contains a full message

                    //construct message

                    //handle message

                    //resend message

                }
                ci[i].stream_msg_recv_pos = pos;
            }
        }
    }


}

/*
public class GameInfo //type 1
{
    public int iNumPlayers = 0;
    //player id 0..3
    public string[] szName = new string[4];
    //public string[] szIP = new string[4];
}

public class GameJoin //type 2
{
    public string szName;
}

public class GameStart //type 3
{
    public string szLevel;
}

public class SendRecv
{
    // unity web vars used in the create/join process
    const string WEB_HOST = "https://galaxy-forces-vr.com";

    internal List<HttpJoinInfo> oJoinList = new List<HttpJoinInfo>();
    internal bool bIsDone = false;
    internal bool bIsMaster;

    NetPeerConfiguration config = new NetPeerConfiguration("GFVR");
    NetServer server = null;
    NetClient client = null;

    GameInfo gi = new GameInfo();
    int iMyPlayerId = -1;

    UnityWebRequest www;
    public IEnumerator UpdateJoin()
    {
        bIsDone = false;
        bIsMaster = false;

        string url = WEB_HOST + "/mpgame_join.php";
        www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            //retrieve results as text
            string szResult = www.downloadHandler.text;
            HttpJoinInfo stJoin = new HttpJoinInfo();

            string[] szLines = szResult.Split((char)10);
            oJoinList.Clear(); //rebuild list

            //parse result
            for (int i = 0; i < szLines.Length; i++)
            {
                //handle spaces in names
                char[] szSeparator2 = { (char)'\"' };
                string[] szWithin = szLines[i].Trim('\r', '\n').Split(szSeparator2, StringSplitOptions.RemoveEmptyEntries);
                stJoin.szName = szWithin[1].Trim(' ');
                stJoin.szIP = szWithin[3].Trim(' ');

                oJoinList.Add(stJoin);
            }
        }
        bIsDone = true;
    }

    public IEnumerator UpdateCreate()
    {
        bIsDone = false;
        bIsMaster = true;

        string url = WEB_HOST + "/mpgame_create.php?User=" + UnityWebRequest.EscapeURL(GameManager.szUser);
        www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            //retrieve results as text
            string szResult = www.downloadHandler.text;

            Debug.Log("Created game as \"" + GameManager.szUser + "\" [" + szResult + "]");
        }

        if (server == null)
        {
            config.Port = 14242;
            server = new NetServer(config);
            server.Start();

            gi.iNumPlayers = 1;
            gi.szName[0] = GameManager.szUser;
            iMyPlayerId = 0;
        }

        bIsDone = true;
    }

    void DoJoin(string szIP)
    {
        NetPeerConfiguration c = new NetPeerConfiguration("GFVR");
        client = new NetClient(c);
        client.Start();
        client.Connect(szIP, 14242);

        //send name
        GameJoin gj = new GameJoin();
        gj.szName = GameManager.szUser;
        NetOutgoingMessage message = client.CreateMessage();
        message.Write( (byte)2 );
        message.WriteAllFields(gj);
        client.SendMessage(message, NetDeliveryMethod.ReliableOrdered);
    }

    void DoStart(string szLevel)
    {
        GameStart gs = new GameStart();
        gs.szLevel = szLevel;
        NetOutgoingMessage message = server.CreateMessage();
        message.Write((byte)3);
        message.WriteAllFields(gs);
        server.SendToAll(message, NetDeliveryMethod.ReliableOrdered);
    }

    int ClientCheck()
    {
        int iResult = 0;
        //recv data
        // construct message
        // handle message
        NetIncomingMessage msg;
        while ((msg = client.ReadMessage()) != null)
        {
            switch (msg.MessageType)
            {
                case NetIncomingMessageType.VerboseDebugMessage:
                case NetIncomingMessageType.DebugMessage:
                case NetIncomingMessageType.WarningMessage:
                case NetIncomingMessageType.ErrorMessage:
                    Console.WriteLine(msg.ReadString());
                    break;
                case NetIncomingMessageType.Data:
                    byte type = msg.ReadByte();
                    switch(type)
                    {
                        case 1:
                            bool found = false;
                            msg.ReadAllFields(gi);
                            for(int i=0; i<gi.iNumPlayers; i++)
                            {
                                if (gi.szName[i].CompareTo(GameManager.szUser) == 0)
                                {
                                    iMyPlayerId = i;
                                    found = true;
                                    break;
                                }
                            }
                            if(!found && gi.iNumPlayers==4) //gi.iNumPlayers==4 is a hack to not disconnect if two or more players have joined at the same time
                            {
                                //disconnect
                                client.Disconnect("game full");
                            }
                            break;
                        case 3:
                            GameStart gs = new GameStart();
                            msg.ReadAllFields(gs);
                            iResult = 1;
                            break;
                    }
                    break;
                default:
                    Console.WriteLine("Unhandled type: " + msg.MessageType);
                    break;
            }
            client.Recycle(msg);
        }
        return iResult;
    }

    void ServerCheck()
    {
        //accept new joining players
        //server.

        //recv data
        // construct message
        // handle message
        // resend message
        NetIncomingMessage msg;
        while ((msg = server.ReadMessage()) != null)
        {
            switch (msg.MessageType)
            {
                case NetIncomingMessageType.VerboseDebugMessage:
                case NetIncomingMessageType.DebugMessage:
                case NetIncomingMessageType.WarningMessage:
                case NetIncomingMessageType.ErrorMessage:
                    Console.WriteLine(msg.ReadString());
                    break;
                case NetIncomingMessageType.Data:
                    byte type = msg.ReadByte();
                    switch (type)
                    {
                        case 2:
                            GameJoin gj = new GameJoin();
                            msg.ReadAllFields(gj);

                            if(gi.iNumPlayers<=3)
                            {
                                gi.szName[gi.iNumPlayers] = gj.szName;
                                gi.iNumPlayers++;
                            }
                            //build reply
                            NetOutgoingMessage message = server.CreateMessage();
                            message.Write( (byte)1 );
                            message.WriteAllFields(gi);
                            //send to all
                            server.SendToAll(message, NetDeliveryMethod.ReliableOrdered);
                            break;
                    }
                    break;
                default:
                    Console.WriteLine("Unhandled type: " + msg.MessageType);
                    break;
            }
            server.Recycle(msg);
        }
    }

}
*/