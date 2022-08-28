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
    public string szGameName;
    public int iClientNum; //index in list of clients (GameInfo) (master always index 0)
    public string szIP;
    public System.Net.Sockets.TcpClient tcp;
    public System.Net.Sockets.NetworkStream ns;

    //impl of "peek buffer", or gather a full message before processing it
    public bool stream_recv_len_valid;
    public int stream_recv_len;
    public int stream_recv_pos;
    public byte[] stream_recv;

    public bool bPingSent;
    public int iLastPingSeq;
    public DateTime dtLastPingResponse;
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
    //player id 0..3, ""=no player with this id/pos
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 38)]
    public string szName1;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 38)]
    public string szName2;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 38)]
    public string szName3;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 38)]
    public string szName4;
}

public struct GameStart //type 3, from master to all
{
    public char type;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 38)] //max len?
    public string szLevel;
}

public struct GamePing //type 4, to server, to client
{
    public char type;
    public char origin_server; //1 is first send from server to a client
    public int seq;
}

public class SendRecv
{
    const double PingTime = 3.0;
    const double PingAnswerTime = 9.0; //high during debug later 9.0 or something

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

        if ((www.result == UnityWebRequest.Result.ConnectionError) || (www.result == UnityWebRequest.Result.ProtocolError))
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
                if (task2.IsCompleted)
                {
                    externalIP = task2.Result;

                    //test mapping 1999, 2009, 2019 and so on for 20 retries
                    for(int i=0; i<20; i++)
                    {
                        System.Threading.Tasks.Task task3 = device.CreatePortMapAsync(new Mapping(Protocol.Tcp, iBasePort, iBasePort, "GFVR"));
                        while (!task3.IsCompleted && !task3.IsFaulted) yield return null;
                        if (task3.IsCompleted)
                        {
                            Debug.Log("Port "+ iBasePort.ToString() + " mapped on " + externalIP.ToString());
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
                Debug.Log("Port not mapped, making the best of it and use IP " + externalIP.ToString());
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

        if ((www.result == UnityWebRequest.Result.ConnectionError) || (www.result == UnityWebRequest.Result.ProtocolError))
        {
            Debug.Log(www.error);
        }
        else
        {
            //retrieve results as text
            string szResult = www.downloadHandler.text;

            Debug.Log("Created game as \"" + GameManager.szUser + "\" [" + externalIP.ToString() + "], [" + localIP +"]");
        }
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
        if (iNumCI >= 3) client.Close();
        else
        {
            for (int i = 0; i < 3; i++)
            {
                if (ci[i].tcp == null) //a free pos
                {
                    ci[i].szIP = client.Client.RemoteEndPoint.ToString();
                    ci[i].tcp = client;
                    ci[i].ns = client.GetStream();
                    ci[i].stream_recv_pos = 0;
                    ci[i].stream_recv = new byte[256];
                    ci[i].stream_recv_len_valid = false;
                    ci[i].iLastPingSeq = 0;
                    ci[i].dtLastPingResponse = DateTime.Now;
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

    internal GameInfo gi = new GameInfo();
    internal GameStart gs = new GameStart();

    void ClearStructs()
    {
        gi.szName1 = "";
        gi.szName2 = "";
        gi.szName3 = "";
        gi.szName4 = "";
    }

    public SendRecv()
    {
        ClearStructs();
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

    public void CancelCI(ConnectionInfo ci_to_cancel)
    {
        if (ci_to_cancel.iClientNum == 1) gi.szName2 = "";
        if (ci_to_cancel.iClientNum == 2) gi.szName3 = "";
        if (ci_to_cancel.iClientNum == 3) gi.szName4 = "";

        if (ci_to_cancel.tcp != null)
        {
            try
            {
                ci_to_cancel.tcp.Client.Close();
                ci_to_cancel.tcp.Close();
                ci_to_cancel.ns.Close();
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }
        //proper reset all vars with or witout error
        ci_to_cancel.bPingSent = false;
        ci_to_cancel.iClientNum = 0;
        ci_to_cancel.szIP = "";
        ci_to_cancel.ns = null;
        ci_to_cancel.tcp = null;
    }
    public void Cancel()
    {
        iNumCI = 0;
        bIsMaster = false;
        oJoinList.Clear();
        ClearStructs();
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
            CancelCI(ci[i]);
        }
        CancelCI(ci_toserver);

        SendRecv.tcpClientConnected.Reset();
        server = null;
    }

    //done by each client
    public void DoJoin(int iNum)
    {
        ci_toserver.szIP = oJoinList[iNum].szIP;
        ci_toserver.szGameName = oJoinList[iNum].szName;
        try
        {
            //TODO async connect (non blocking), this freezes the game for 2 sec
            ci_toserver.tcp = new TcpClient(oJoinList[iNum].szIP, oJoinList[iNum].iPort);
            ci_toserver.ns = ci_toserver.tcp.GetStream();
        }
        catch (Exception e)
        {
            Debug.Log("DoJoin: " + e.Message);
            try
            {
                //TODO async connect (non blocking), this freezes the game for 2 sec
                ci_toserver.tcp = new TcpClient(oJoinList[iNum].szLocalIP, oJoinList[iNum].iPort);
                ci_toserver.ns = ci_toserver.tcp.GetStream();
            }
            catch (Exception e2)
            {
                Debug.Log("DoJoin: " + e2.Message);
            }
        }
        ci_toserver.stream_recv = new byte[256];
        ci_toserver.stream_recv_pos = 0;
        ci_toserver.stream_recv_len_valid = false;
        ci_toserver.iLastPingSeq = 0;
        ci_toserver.dtLastPingResponse = DateTime.Now;

        GameJoin toServer;
        toServer.type = (char)1;
        toServer.szName = GameManager.szUser;
        byte[] b = getBytes<GameJoin>(toServer);
        byte[] bl = { (byte)b.Length };
        try
        {
            ci_toserver.ns.Write(bl, 0, bl.Length);
            ci_toserver.ns.Write(b, 0, b.Length);
        }
        catch (Exception e)
        {
            Debug.Log("DoJoin: " + e.Message);
        }
    }

    //done by master
    public void DoStartGame(string szLevel)
    {
        gs.type = (char)3;
        byte[] b = getBytes<GameStart>(gs);
        byte[] bl = { (byte)b.Length };
        for (int j = 0; j < 3; j++)
        {
            try
            {
                if (ci[j].tcp != null)
                {
                    ci[j].ns.Write(bl, 0, bl.Length);
                    ci[j].ns.Write(b, 0, b.Length);
                }
            }
            catch (Exception e)
            {
                Debug.Log("DoStartGame: " + e.Message);
            }
        }
    }

    byte[] tmp_len = new byte[1];
    public int ClientCheck()
    {
        //send ping
        if ((DateTime.Now - ci_toserver.dtLastPingResponse) > TimeSpan.FromSeconds(PingTime) && !ci_toserver.bPingSent && ci_toserver.ns.CanWrite)
        {
            ci_toserver.bPingSent = true;
            GamePing gp = new GamePing();
            gp.type = (char)4;
            gp.seq = ci_toserver.iLastPingSeq++;
            gp.origin_server = (char)0;
            byte[] b = getBytes<GamePing>(gp);
            byte[] bl = { (byte)b.Length };
            try
            {
                ci_toserver.ns.Write(bl, 0, bl.Length);
                ci_toserver.ns.Write(b, 0, b.Length);
            }
            catch (Exception e)
            {
                Debug.Log("ClientCheck: " + e.Message);
            }
        }

        //check connection valid (ping)
        if ((DateTime.Now - ci_toserver.dtLastPingResponse) > TimeSpan.FromSeconds(PingAnswerTime))
        {
            //too long period, connection dead?
            //cancel
            Cancel();
            return 2;
        }

        //recv data
        if (ci_toserver.ns.CanRead && ci_toserver.ns.DataAvailable)
        {
            int action = 0;
            //get length of next message
            if (!ci_toserver.stream_recv_len_valid)
            {
                ci_toserver.ns.Read(tmp_len, 0, 1);
                ci_toserver.stream_recv_len = tmp_len[0];
                ci_toserver.stream_recv_len_valid = true;
            }

            byte[] buf = ci_toserver.stream_recv;
            int pos = ci_toserver.stream_recv_pos;
            int iNumRead = ci_toserver.ns.Read(buf, pos, ci_toserver.stream_recv_len - pos);
            pos += iNumRead;
            if (pos == ci_toserver.stream_recv_len)
            {
                pos = 0;
                ci_toserver.stream_recv_len_valid = false;
                //buf contains a full message

                //construct message
                //handle message
                switch (ci_toserver.stream_recv[0])
                {
                    //msgs in loby
                    case 2:
                        gi = fromBytes<GameInfo>(ci_toserver.stream_recv);
                        action = 2;
                        break;
                    case 3:
                        gs = fromBytes<GameStart>(ci_toserver.stream_recv);
                        action = 3;
                        break;
                    case 4:
                        {
                            GamePing gp = fromBytes<GamePing>(ci_toserver.stream_recv);
                            if(gp.origin_server!=(char)0)
                            {
                                //respond with the same
                                byte[] b = getBytes<GamePing>(gp);
                                byte[] bl = { (byte)b.Length };
                                try {
                                    ci_toserver.ns.Write(bl, 0, bl.Length);
                                    ci_toserver.ns.Write(b, 0, b.Length);
                                }
                                catch (Exception e)
                                {
                                    Debug.Log("ClientCheck: " + e.Message);
                                }
                            }
                            else
                            {
                                //update ping to valid
                                ci_toserver.dtLastPingResponse = DateTime.Now;
                                ci_toserver.bPingSent = false;
                            }
                        }
                        break;
                    //msgs in game
                    //case ?:
                        //TODO
                        //break;
                }

            }
            ci_toserver.stream_recv_pos = pos;
            return action;
        }
        return 0;
    }

    public int ServerCheck()
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

        //send ping
        for (int i = 0; i < 3; i++)
        {
            if (ci[i].tcp != null && (DateTime.Now - ci[i].dtLastPingResponse) > TimeSpan.FromSeconds(PingTime) && !ci[i].bPingSent && ci[i].ns.CanWrite)
            {
                ci[i].bPingSent = true;
                GamePing gp = new GamePing();
                gp.type = (char)4;
                gp.seq = ci[i].iLastPingSeq++;
                gp.origin_server = (char)1;
                byte[] b = getBytes<GamePing>(gp);
                byte[] bl = { (byte)b.Length };
                try
                {
                    ci[i].ns.Write(bl, 0, bl.Length);
                    ci[i].ns.Write(b, 0, b.Length);
                }
                catch(Exception e)
                {
                    Debug.Log("ServerCheck: " + e.Message);
                }
            }
        }

        //check connection valid (ping)
        for (int i = 0; i < 3; i++)
        {
            if (ci[i].tcp != null && (DateTime.Now - ci[i].dtLastPingResponse) > TimeSpan.FromSeconds(PingAnswerTime))
            {
                //too long period, connection dead?
                //cancel this client
                CancelCI(ci[i]);
                iNumCI--;
                return 2;
            }
        }

        //recv data
        int action = 0;
        for (int i = 0; i < 3; i++)
        {
            if (ci[i].tcp != null && ci[i].ns.CanRead && ci[i].ns.DataAvailable)
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
                        //msgs in loby
                        case 1:
                            {
                                GameJoin gj = fromBytes<GameJoin>(ci[i].stream_recv);
                                if (gi.szName2 == "") { gi.szName2 = gj.szName; ci[i].iClientNum = 1; }
                                else if (gi.szName3 == "") { gi.szName3 = gj.szName; ci[i].iClientNum = 2; }
                                else if (gi.szName4 == "") { gi.szName4 = gj.szName; ci[i].iClientNum = 3; }
                                //else full, should not happen

                                //send to all
                                gi.type = (char)2;
                                byte[] b = getBytes<GameInfo>(gi);
                                byte[] bl = { (byte)b.Length };
                                for (int j = 0; j < 3; j++)
                                {
                                    if (ci[j].tcp != null)
                                    {
                                        try
                                        {
                                            ci[j].ns.Write(bl, 0, bl.Length);
                                            ci[j].ns.Write(b, 0, b.Length);
                                        }
                                        catch(Exception e)
                                        {
                                            Debug.Log("ServerCheck: " + e.Message);
                                        }
                                    }
                                }

                                action = 2;
                            }
                            break;
                        case 4:
                            {
                                GamePing gp = fromBytes<GamePing>(ci[i].stream_recv);

                                if (gp.origin_server == (char)0)
                                {
                                    //respond with the same
                                    byte[] b = getBytes<GamePing>(gp);
                                    byte[] bl = { (byte)b.Length };
                                    try
                                    {
                                        ci[i].ns.Write(bl, 0, bl.Length);
                                        ci[i].ns.Write(b, 0, b.Length);
                                    }
                                    catch (Exception e)
                                    {
                                        Debug.Log("ServerCheck: " + e.Message);
                                    }
                                }
                                else
                                {
                                    //update ping to valid
                                    ci[i].dtLastPingResponse = DateTime.Now;
                                    ci[i].bPingSent = false;
                                }
                            }
                            break;
                            //msgs in game
                            //case ?:
                            //TODO
                            //break;
                    }

                }
                ci[i].stream_recv_pos = pos;
                return action;
            }
        }
        return 0;
    }


}

