using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

using System.Runtime.InteropServices;

using Open.Nat;

public struct HttpJoinInfo
{
    public string szName;
    public string szIP; //internet ip
    public string szLocalIP; //not needed?
    public int iPort; //port forwarded and local, that we listen on
}

public struct ConnectionInfo
{
    public string szName;
    public string szIP;
    public System.Net.Sockets.TcpClient tcp;
    public System.Net.Sockets.NetworkStream ns;

    //impl of "peek buffer", or gather a full message before processing it
    public bool stream_recv_len_valid;
    public int stream_recv_len;
    public int stream_recv_pos;
    public byte[] stream_recv;
}

public struct GameJoin //type 1, to server
{
    public char type;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 38)]
    public string szName;
}

public struct GameInfo //type 2, to client
{
    public char type;
    public char iNumPlayers;
    //player id 0..3
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 38)]
    public string szName1;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 38)]
    public string szName2;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 38)]
    public string szName3;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 38)]
    public string szName4;
}

public struct GameStart //type 3, from master to server from server to all
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 38)] //max len?
    public string szLevel;
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

        string url = WEB_HOST + "/mpgames_join.php";
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
                if (szWithin.Length < 4) break;

                stJoin.szName = szWithin[0].Trim(' ');
                stJoin.szIP = szWithin[2].Trim(' ');
                stJoin.szLocalIP = szWithin[4].Trim(' ');

                string[] szPort = szWithin[3].Split(szSeparator, StringSplitOptions.RemoveEmptyEntries);
                stJoin.iPort = int.Parse(szPort[0].Trim(' '));

                oJoinList.Add(stJoin);
            }
        }
        bIsJoinDone = true;
    }

    //run periodically from game created until begin play level
    NatDevice device;
    IPAddress externalIP;
    IPAddress localIP;
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

        string url = WEB_HOST + "/mpgames_create.php?User=" + UnityWebRequest.EscapeURL(GameManager.szUser) + "&IP=" + externalIP.ToString() + "&Port=" + iBasePort.ToString() + "&LocalIP=" + localIP.ToString();
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

            Debug.Log("Created game as \"" + GameManager.szUser + "\" [" + externalIP.ToString() + "], [" + localIP +"]");
        }
        gi.iNumPlayers = 1;
        gi.szName1 = GameManager.szUser;

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
        Debug.Log("Waiting for a connection...");

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

        // End the operation.
        TcpClient client = null;
        try
        {
            client = listener.EndAcceptTcpClient(ar);
            client.NoDelay = true;
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
        if (client == null) return;
        if (iNumCI == 3) client.Close();
        else
        {
            for (int i = 0; i < 3; i++)
            {
                if (ci[i].tcp == null) //a free pos
                {
                    ci[i].szIP = client.Client.RemoteEndPoint.ToString();
                    ci[i].tcp = client;
                    /**///ci[i].tcp.LingerState = new LingerOption(true, 0);
                    ci[i].ns = client.GetStream();
                    ci[i].stream_recv_pos = 0;
                    ci[i].stream_recv = new byte[256];
                    ci[i].stream_recv_len_valid = false;
                    iNumCI++;
                    break;
                }
            }
        }

        // Process the connection here. (Add the client to a
        // server table, read data, etc.)
        Debug.Log("Client connect completed");

        // Signal the calling thread to continue.
        tcpClientConnected.Set();
    }



    bool bIsMaster;
    bool bListening = false; //true while master is listening for joining players
    static int iNumCI = 0;
    static ConnectionInfo[] ci = new ConnectionInfo[3]; //the other players (1..3) when master
    ConnectionInfo ci_toserver; //the server, when !master

    GameInfo gi = new GameInfo();

    public SendRecv()
    {
    }

    byte[] getBytes<T>(T str)
    {
        int size = Marshal.SizeOf(str);
        byte[] arr = new byte[size];

        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(str, ptr, true);
        Marshal.Copy(ptr, arr, 0, size);
        Marshal.FreeHGlobal(ptr);
        return arr;
    }
    T fromBytes<T>(byte[] arr)
    {
        int size = Marshal.SizeOf<T>();
        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.Copy(arr, 0, ptr, size);

        T str = Marshal.PtrToStructure<T>(ptr);
        Marshal.FreeHGlobal(ptr);

        return str;
    }

    public void Cancel()
    {
        iNumCI = 0;
        bIsMaster = false;
        oJoinList.Clear();
        try
        {
            if (server != null)
            {
                server.Stop();
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
        for(int i=0; i<3; i++)
        {
            try
            {
                if (ci[i].tcp != null)
                {
                    ci[i].tcp.Client.Close();
                    ci[i].tcp.Close();
                    ci[i].ns.Close();
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }
        try
        {
            if (ci_toserver.tcp != null)
            {
                ci_toserver.tcp.Client.Close();
                ci_toserver.tcp.Close();
                ci_toserver.ns.Close();
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }

        //proper reset all vars with or witout error
        SendRecv.tcpClientConnected.Reset();
        server = null;
        for (int i = 0; i < 3; i++)
        {
            ci[i].ns = null;
            ci[i].tcp = null;
        }
        ci_toserver.ns = null;
        ci_toserver.tcp = null;
    }

    public void DoJoin(int iNum)
    {
        ci_toserver.szIP = oJoinList[iNum].szIP;
        ci_toserver.szName = oJoinList[iNum].szName;
        try
        {
            ci_toserver.tcp = new TcpClient(oJoinList[iNum].szIP, oJoinList[iNum].iPort);
            /**///ci_toserver.tcp.LingerState = new LingerOption(true, 0);
            ci_toserver.ns = ci_toserver.tcp.GetStream();
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            try
            {
                ci_toserver.tcp = new TcpClient(oJoinList[iNum].szLocalIP, oJoinList[iNum].iPort);
                /**///ci_toserver.tcp.LingerState = new LingerOption(true, 0);
                ci_toserver.ns = ci_toserver.tcp.GetStream();
            }
            catch (Exception e2)
            {
                Debug.Log(e2.Message);
            }
        }
        ci_toserver.stream_recv = new byte[256];
        ci_toserver.stream_recv_pos = 0;
        ci_toserver.stream_recv_len_valid = false;

        GameJoin toServer;
        toServer.type = (char)1;
        toServer.szName = GameManager.szUser;
        byte[] b = getBytes<GameJoin>(toServer);
        byte[] bl = { (byte)b.Length };
        ci_toserver.ns.Write(bl, 0, bl.Length);
        ci_toserver.ns.Write(b, 0, b.Length);
    }

    public void ClientCheck()
    {
        //recv data
    }

    byte[] tmp_len = new byte[1];
    public void ServerCheck()
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
                //get length of next message
                if(!ci[i].stream_recv_len_valid)
                {
                    ci[i].ns.Read(tmp_len, 0, 1);
                    ci[i].stream_recv_len = tmp_len[0];
                    ci[i].stream_recv_len_valid = true;
                }

                byte[] buf = ci[i].stream_recv;
                int pos = ci[i].stream_recv_pos;
                int iNumRead = ci[i].ns.Read(buf, pos, ci[i].stream_recv_len - pos);
                pos += iNumRead;
                if (pos == ci[i].stream_recv_len)
                {
                    pos = 0;
                    ci[i].stream_recv_len_valid = false;
                    //buf contains a full message

                    //construct message
                    //handle message
                    switch (ci[i].stream_recv[0])
                    {
                        case 1:
                            GameJoin gj = fromBytes<GameJoin>(ci[i].stream_recv);
                            gi.iNumPlayers++;
                            if (gi.iNumPlayers == 2) gi.szName2 = gj.szName;
                            if (gi.iNumPlayers == 3) gi.szName3 = gj.szName;
                            if (gi.iNumPlayers == 4) gi.szName4 = gj.szName;
                            break;
                        case 3:
                            break;
                    }

                    //resend message

                }
                ci[i].stream_recv_pos = pos;
            }
        }
    }


}

/*

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
