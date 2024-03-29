﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Net.NetworkInformation;
namespace CameraTrackingAppv3
{

    [Serializable]
    enum ConnectType
    {
        None,
        FirstSend,
        FirstReceive,
        AddIP,
        LoadIP,
        Broadcast,
        Active,
        Correction
    }

    [Serializable]
    public class RecodeUser
    {
        public string UserName;
        public string MachineName;
        public string IPAddress;
        public string MACAddress;

        public override bool Equals(object obj)
        {
            if (!(obj is RecodeUser)) return false;
            var a = (RecodeUser)obj;

            return UserName.Equals(a.UserName) && 
                   MachineName.Equals(a.MachineName) && 
                   IPAddress.Equals(a.IPAddress);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return "UserName: " + UserName + " , MachineName: " + MachineName + " , IPAddress: " + IPAddress + " , MACAddress: " + MACAddress;
        }

    }

    class Connect
    {
        TcpListener tcpListener;
        UdpClient udpListener;
        int port_num = -1;//62355;
        delegate void SendData(string sender_ip, ConnectType type, byte[] data);


        bool f_connect = false;
        List<string> other_computer = new List<string>();
        List<RecodeUser> users = new List<RecodeUser>();
        RecodeUser myself;
        double timeout_reload = 0.5;
        double timeout_found_ip = 0.5;

        public Connect(int port_num= 62355)
        {
           // this.port_num = port_num;
            Init(this.port_num);
        }

        public List<RecodeUser> GetRecodeUsers()
        {
            return users;
        }

        public bool Init(int port_num)
        {
            if (port_num == this.port_num)
                return true;
            ReloadMyRecode();

            IPEndPoint ip = new IPEndPoint(IPAddress.Any,port_num);
            bool ok = CheckPort(port_num);
            if (ok)
            {

                if (tcpListener != null)
                    tcpListener.Stop();
                if (udpListener != null)
                    udpListener.Close();

                tcpListener = new TcpListener(ip);
                tcpListener.Start();
                udpListener = new UdpClient(port_num);
                this.port_num = port_num;

            }
            return ok;
        }

        public void Start()
        {
            if (f_connect) return;
            f_connect = true;
            Thread tcp_thread = new Thread(new ThreadStart(TCPReceive));
            tcp_thread.Start();

            Thread udp_thread = new Thread(new ThreadStart(UDPReceive));
            udp_thread.Start();
        }

        public void End()
        {
            f_connect = false;
        }


        public bool ReloadMyRecode()
        {
            var a = Utils.GetActivePhysicalAddress();
            if (a.Count == 0) {
                Utils.Alert_Error("インターネットアダプタがありません");
                return false;
            }

            myself = new RecodeUser()
            {
                UserName = Utils.UserName,
                MachineName = Environment.MachineName,
                IPAddress = "",//Dns.GetHostEntry("localhost").AddressList[0].ToString(),
                MACAddress = a[0]
            };
            return true;
        }


        public void ReLoadIPAddress()
        {
            if (!CheckPort(port_num)) return;

            UDPSendBroadcastMessage(Utils.Password);

        }

        public void ReceiveChangeMessage()
        {
            if (!CheckPort(port_num)) return;

            UDPListenBroadcastMessage(out var ip);

        }


        bool CheckPort(int port_num)
        {
            //var port_num = Utils.PortNum;
            var p = IPGlobalProperties.GetIPGlobalProperties();//.GetActiveTcpListeners();
            bool ok = true;
            foreach (var i in p.GetActiveTcpListeners())
            {
                if (i.Port == port_num)
                {
                    ok = false;
                    break;
                }
            }

            if(ok)
            foreach (var i in p.GetActiveUdpListeners())
            {
                if (i.Port == port_num)
                {
                    ok = false;
                    break;
                }
            }

            if (!ok)
            {
                Utils.Alert_Note("ポート番号が重なっています");
            }
            return ok;
        }


        void ReceiveMessage()
        {
            while (f_connect)
            {

               // if (TCP.ReceiveMessage(IPAddress.Any.ToString(), out var sender,out var type , out var data))
               //     Event(sender.Address.ToString(),type, data);

            }

        }

      //  void Event(string sender_ip,ConnectType type,byte[] data)
        void Event(object sender)
        {
            

            string sender_ip;
            ConnectType type;
            object data;

            {
                object[] a = sender as object[];
                sender_ip = a[0] as string;
                type = (ConnectType)a[1];
                data = a[2];// as byte[];
            }


            if (type == ConnectType.AddIP)
            {
                Utils.WriteLine("AddIP受け取りました!!!");



                var a = (RecodeUser)(data);
                a.IPAddress = sender_ip;
                users.Add(a);


            }


            if(type == ConnectType.Broadcast)
            {

                string a = (string)(data);

                var head = a.Substring(0, 1);
                var body = a.Substring(1, a.Length - 1);
                Utils.WriteLine("Broadcast受け取りました!!!");

                if (head.Equals("1"))
                {

                    TCPSendMessage(sender_ip, ConnectType.AddIP, myself);
                }else
                if (head.Equals("2"))
                {
                    if (Utils.GetAllPhysicalAddress().Contains(body))
                    {
                        //ReloadMyRecode();
                        //アクティブになる処理
                        //TCP.SendMessage(sender_ip, ConnectType.Correction, Utils.ObjectToByteArray(body));
                        TCPSendMessage(sender_ip, ConnectType.Correction, body);
                    }
                }
            }


            if(type == ConnectType.LoadIP)
            {

                users = (List<RecodeUser>)data;

            }

            if (type == ConnectType.Active)
            {
                //アクティブになる処理
                Utils.MainForm.WaitProcess.ReceiveActiveSignal();
            }

            if(type == ConnectType.Correction)
            {
              //  var mac = (string)Utils.ByteArrayToObject(data);
                var mac = (string)(data);
                for(int i = 0; i < users.Count; i++)
                {
                    if (mac.Equals(users[i].MACAddress))
                    {
                        users[i].IPAddress = sender_ip;
                        break;
                    }
                }
            }

        }



        void TCPReceive()
        {

            while (f_connect)
            {
                try
                {
                    Utils.WriteLine("TCP受け取り待ち");
                    var client = tcpListener.AcceptTcpClient();
                    var thre = new Thread(new ParameterizedThreadStart(TCPReadData));
                    thre.Start(client);
                    Utils.WriteLine("TCP受け取けとりました!!!!");
                }
                catch//SocketException e)
                {
                    Utils.WriteLine("tcplisnearがエラーを起こしました");
                    Thread.Sleep(1000);
                }

            }
        }

        void TCPReadData(object sender)
        {
            Utils.WriteLine("TCPを読み取る");
            TcpClient client = sender as TcpClient;
            using (var stream = client.GetStream())
            {
                byte[] buf = new byte[1024];


                if (stream.Read(buf, 0, buf.Length) > 0)
                {
                    var thre = new Thread(new ParameterizedThreadStart(Event));
                    object[] data = (object[])Utils.ByteArrayToObject(buf);
                    object[] a = new object[3];

                    a[0] = (client.Client.RemoteEndPoint as IPEndPoint).Address.ToString();
                    a[1] = (ConnectType)data[0];//type;
                    a[2] = data[1];//buf;
                    thre.Start(a);
                }


                stream.Close();
            }
            client.Dispose();
        }

        void UDPReceive()
        {
            while (f_connect)
            {
                try
                {
                    IPEndPoint ip = null;
                    Utils.WriteLine("UDP受け取り待ち");

                    byte[] buf = udpListener.Receive(ref ip);
                    Utils.WriteLine("UDP受け取りました!!!!");

                    var thre = new Thread(new ParameterizedThreadStart(Event));
                    object[] a = new object[3];

                    a[0] = ip.Address.ToString();
                    a[1] = ConnectType.Broadcast;
                    a[2] = Utils.ByteArrayToObject(buf);
                    thre.Start(a);
                }
                catch
                {
                    Utils.WriteLine("udplisnearがエラーを起こしました");
                    Thread.Sleep(1000);
                }
            }
        }

        //更新ボタンを押したときの処理
        public void BroadcastConnectSignal()
        {
            users.Clear();
            //users.Add(myself);
            UDPSendBroadcastMessage("1");
        }

        //一定時間が経過した後、他のパソコンへデータを送信する処理
        public void SequenceLoadSignal()
        {

            foreach(var i in users)
            {

                TCPSendMessage(i.IPAddress, ConnectType.LoadIP,users);
            }
        }

        //起動/待機の切り替え
        public bool SendActiveSignal(int selected_index)
        {
            //暫定
            var a = users[selected_index];
          //  var ok = TCP.SendMessage(a.IPAddress, ConnectType.Active, Utils.ObjectToByteArray("1"));
            var ok = TCPSendMessage(a.IPAddress, ConnectType.Active, "1");
            if (!ok)
            {
                UDPSendBroadcastMessage("2" + a.MACAddress);

            }
            return ok;
        }

        void ShowUsers()
        {
            Utils.WriteLine("AddRecodeUsers:--------");
            lock (users)
            {
                foreach (var i in users)
                {
                    Utils.WriteLine(i.ToString());
                }
                Utils.WriteLine("EndRecodeUsers:--------");
            }
        }



        void UDPSendBroadcastMessage(string data)
        {
            // 送受信に利用するポート番号
            var port = port_num;//Utils.PortNum;

            // 送信データ
            var buffer = Utils.ObjectToByteArray(data);//Encoding.UTF8.GetBytes(data);

            // ブロードキャスト送信
            using (var client = new UdpClient())
            {
                client.EnableBroadcast = true;
                client.Connect(new IPEndPoint(IPAddress.Broadcast, port));
                client.Send(buffer, buffer.Length);
                client.Close();
            }
            Utils.WriteLine("ブロードキャスト送信済み");
        }


        void UDPListenBroadcastMessage(out IPEndPoint remote)
        {
            // 送受信に利用するポート番号
            var port = port_num;//Utils.PortNum;

            // ブロードキャストを監視するエンドポイント


            // UdpClientを生成
            var client = new UdpClient(port);

            // データ受信を待機（同期処理なので受信完了まで処理が止まる）
            // 受信した際は、 remote にどの IPアドレス から受信したかが上書きされる
            bool ok = false;
            while (true)
            {
                remote = new IPEndPoint(IPAddress.Any, port);
                Utils.WriteLine("受信待ち");

                var buffer = client.Receive(ref remote);

                // 受信データを変換
                var data = Encoding.UTF8.GetString(buffer);
                Utils.WriteLine("パスワードの確認");

                // 受信イベントを実行
                if (data.Equals(Utils.Password))
                    break;
            }
            Utils.WriteLine("パスワード通過");
        }

        bool TCPSendMessage(string target_ip, ConnectType type, object data)
        {

            byte[] buf1;// = new byte[1024];
            //   Regex reg = new Regex("\0");
            bool ok = false;
            IPEndPoint ip = new IPEndPoint(IPAddress.Parse(target_ip), Utils.PortNum);

            try
            {
                using (var client = new TcpClient())
                {
                    Utils.WriteLine("TCPコネクト");

                    client.Connect(ip);
                    using (var stream = client.GetStream())
                    {

                        object[] send_data = new object[2];
                        send_data[0] = type;
                        send_data[1] = data;
                        buf1 = Utils.ObjectToByteArray(send_data);
                        stream.Write(buf1, 0, buf1.Length);

                        ok = true;

                        stream.Close();
                    }
                    client.Close();
                }
            }
            catch (Exception ex)
            {
                Utils.WriteLine(ex.Message);
            }

            return ok;

        }

        bool TCPReceiveMessage(string target_ip, out IPEndPoint end_point, out ConnectType type, out byte[] data)
        {

            TcpListener server;
            string recvline, sendline = null;

            int num, i = 0;
            Boolean outflg = false;
            byte[] buf = new byte[1024];
            Regex reg = new Regex("\0");
            end_point = null;
            bool ok = false;
            // data = "";
            data = null;
            type = ConnectType.None;
            IPEndPoint ip = new IPEndPoint(IPAddress.Parse(target_ip), Utils.PortNum);
            try
            {
                server = new TcpListener(ip);
                Utils.WriteLine("クライアント待ち状態");
                server.Start();

                using (var client = server.AcceptTcpClient())
                {
                    end_point = client.Client.RemoteEndPoint as IPEndPoint;
                    using (var stream = client.GetStream())
                    {
                        if (stream.Read(buf, 0, buf.Length) > 0)
                        {
                            recvline = reg.Replace(Encoding.UTF8.GetString(buf), "");
                            Utils.WriteLine("パスワードの確認");

                            if (recvline.Equals(Utils.Password))
                            {
                                ok = true;
                                Utils.WriteLine("パスワード通過");

                            }
                            Array.Clear(buf, 0, buf.Length);
                        }

                        Utils.WriteLine("以下相手からの入力");
                        if (ok && stream.Read(buf, 0, buf.Length) > 0)
                        {

                            type = (ConnectType)Utils.ByteArrayToObject(buf);

                            Array.Clear(buf, 0, buf.Length);
                        }

                        if (ok && stream.Read(buf, 0, buf.Length) > 0)
                        {

                            data = buf;

                        }

                    }

                    client.Close();
                    Console.WriteLine("通信終了");


                }
                server.Stop();
            }
            catch (Exception e)
            {
                Utils.WriteLine(e.Message);

            }
            return ok;
        }

    }


}
