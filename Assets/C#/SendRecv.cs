using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

using Lidgren.Network;

public struct HttpJoinInfo
{
    public string szName;
    public string szIP;
}

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
