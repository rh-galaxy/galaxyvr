using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

public struct JoinInfo
{
    public string szName;
    public string szIP;
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

    public List<JoinInfo> oJoinList = new List<JoinInfo>();
    public bool bIsDone = false;

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
            JoinInfo stJoin = new JoinInfo();

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
            try
            {
                IPAddress addr = IPAddress.Parse("127.0.0.1");

                server = new TcpListener(addr, 1999);
                server.Start();
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }

        bIsDone = true;
    }


    // tcp server / clients vars used for actual game messages
    TcpListener server = null;

    // Thread signal.
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
        if(iNumCI==4) client.Close();
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
    static ConnectionInfo[] ci = new ConnectionInfo[4];

    public SendRecv()
    {
        
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
        for(int i=0; i< iNumCI; i++)
        {
            if (ci[i].ns.CanRead && ci[i].ns.DataAvailable)
            {
                byte[] buf = ci[i].stream_msg_recv;
                int pos = ci[i].stream_msg_recv_pos;
                int iNumRead = ci[i].ns.Read(buf, pos, buf.Length-pos);
                pos += iNumRead;
                if(pos == buf.Length)
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
