using System;
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
    class RecodeUser
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

        delegate void SendData(string sender_ip, ConnectType type, byte[] data);


        bool f_connect = false;
        List<string> other_computer = new List<string>();
        List<RecodeUser> users = new List<RecodeUser>();
        RecodeUser myself;
        double timeout_reload = 0.5;
        double timeout_found_ip = 0.5;

        public Connect()
        {
            Init();
        }

        public List<RecodeUser> GetRecodeUsers()
        {
            return users;
        }

        public bool Init()
        {

            ReloadMyRecode();

            IPEndPoint ip = new IPEndPoint(IPAddress.Any, Utils.PortNum);
            bool ok = CheckPort();
            if (ok)
            {
                tcpListener = new TcpListener(ip);
                tcpListener.Start();
                udpListener = new UdpClient(Utils.PortNum);
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
            if (!CheckPort()) return;

            UDP.SendBroadcastMessage(Utils.Password);

        }

        public void ReceiveChangeMessage()
        {
            if (!CheckPort()) return;

            UDP.ListenBroadcastMessage(out var ip);

          //  other_computer.Add(ip.Address.ToString());

           // TCP.SendMessage(ip.Address.ToString(),ConnectType.FirstSend,Utils.ObjectToByteArray(other_computer));



        }


        bool CheckPort()
        {
            var port_num = Utils.PortNum;
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

         /*   if (type == ConnectType.FirstSend)
            {

                TCP.SendMessage(sender_ip,ConnectType.FirstReceive,Utils.ObjectToByteArray(other_computer));

                SendOtherComputer(sender_ip, data);

            }
            
            if(type == ConnectType.FirstReceive)
            {
                SendOtherComputer(sender_ip, data);
            }*/
            
            
            if (type == ConnectType.AddIP)
            {
                Utils.WriteLine("AddIP受け取りました!!!");

             //   var a = (RecodeUser)Utils.ByteArrayToObject(data);
                var a = (RecodeUser)(data);
                a.IPAddress = sender_ip;
                users.Add(a);

                Utils.WriteLine("AddRecodeUsers:--------");
                foreach (var i in users)
                {
                    Utils.WriteLine(i.ToString());
                }
                Utils.WriteLine("EndRecodeUsers:--------");
                // var temp = (List<string>)Utils.ByteArrayToObject(data);

                // foreach (var i in temp)
                //     other_computer.Add(i);

            }


            if(type == ConnectType.Broadcast)
            {
                //string a = (string)Utils.ByteArrayToObject(data);
                string a = (string)(data);

                var head = a.Substring(0, 1);
                var body = a.Substring(1, a.Length - 1);
                Utils.WriteLine("Broadcast受け取りました!!!");

                if (head.Equals("1"))
                {
                  //  TCP.SendMessage(sender_ip, ConnectType.AddIP, Utils.ObjectToByteArray(myself));
                    TCP.SendMessage(sender_ip, ConnectType.AddIP, myself);
                }else
                if (head.Equals("2"))
                {
                    if (Utils.GetAllPhysicalAddress().Contains(body))
                    {
                        //ReloadMyRecode();
                        //アクティブになる処理
                        //TCP.SendMessage(sender_ip, ConnectType.Correction, Utils.ObjectToByteArray(body));
                        TCP.SendMessage(sender_ip, ConnectType.Correction, body);
                    }
                }
            }


            if(type == ConnectType.LoadIP)
            {
                //object[] a = (object[])Utils.ByteArrayToObject(data);
                object[] a = (object[])(data);
                RecodeUser b = (RecodeUser)a[1];
                b.IPAddress = sender_ip;
                users = (List<RecodeUser>)a[0];
                users.Add(b);
                Utils.WriteLine("AddRecodeUsers:--------");
                foreach(var i in users)
                {
                    Utils.WriteLine(i.ToString());
                }
                Utils.WriteLine("EndRecodeUsers:--------");

            }

            if (type == ConnectType.Active)
            {
                //アクティブになる処理
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


       /* void SendOtherComputer(string sender_ip, byte[] data)
        {
            var temp = (List<string>)Utils.ByteArrayToObject(data);
            temp.Add(sender_ip);
            var buf = Utils.ObjectToByteArray(temp);

            foreach (var i in other_computer)
            {
                TCP.SendMessage(i, ConnectType.AddIP, buf);
            }

            foreach (var i in temp)
                other_computer.Add(i);
        }*/


        void TCPReceive()
        {
            while (f_connect)
            {
                var thre = new Thread(new ParameterizedThreadStart(TCPReadData));
                Utils.WriteLine("TCP受け取り待ち");
                thre.Start(tcpListener.AcceptTcpClient());
                Utils.WriteLine("TCP受け取けとりました!!!!");

            }
        }

        void TCPReadData(object sender)
        {
            Utils.WriteLine("TCPを読み取る");
            TcpClient client = sender as TcpClient;
            using (var stream = client.GetStream())
            {
                byte[] buf = new byte[1024];
              //  ConnectType type = ConnectType.None;

              //  if (stream.Read(buf, 0, buf.Length) > 0)
             //   {

                   // type = (ConnectType)Utils.ByteArrayToObject(buf);
                   // Array.Clear(buf, 0, buf.Length);
             //   }

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


              //  stream.Write(new byte[] { 1 }, 0, 1);
                stream.Close();
            }
            client.Dispose();
        }

        void UDPReceive()
        {
            while (f_connect)
            {
                IPEndPoint ip = null;//new IPEndPoint(IPAddress.Any,Utils.PortNum);
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
        }

        //更新ボタンを押したときの処理
        public void BroadcastConnectSignal()
        {
            users.Clear();
            //users.Add(myself);
            UDP.SendBroadcastMessage("1");
        }

        //一定時間が経過した後、他のパソコンへデータを送信する処理
        public void SequenceLoadSignal()
        {
            //  var temp = new List<RecodeUser>(users);
            //   temp.Add(myself);
            object[] send = new object[2];
            send[0] = users;
            send[1] = myself;
            foreach(var i in users)
            {
              //  if (i.Equals(myself)) continue;
              //  TCP.SendMessage(i.IPAddress, ConnectType.LoadIP, Utils.ObjectToByteArray(send));
                TCP.SendMessage(i.IPAddress, ConnectType.LoadIP, send);
            }
        }

        //起動/待機の切り替え
        void SendActiveSignal(int selected_index)
        {
            //暫定
            var a = users[selected_index];
          //  var ok = TCP.SendMessage(a.IPAddress, ConnectType.Active, Utils.ObjectToByteArray("1"));
            var ok = TCP.SendMessage(a.IPAddress, ConnectType.Active, "1");
            if (!ok)
            {
                UDP.SendBroadcastMessage("2" + a.MACAddress);

            } 
        }



    }

    static class UDP
    {

        static public void SendBroadcastMessage(string data)
        {
            // 送受信に利用するポート番号
            var port = Utils.PortNum;

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


        static public void ListenBroadcastMessage(out IPEndPoint remote)
        {
            // 送受信に利用するポート番号
            var port = Utils.PortNum;

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
    }

    static class TCP
    {
        //static public bool SendMessage(string target_ip,ConnectType type,byte[] data)
        static public bool SendMessage(string target_ip,ConnectType type,object data)
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
                        //   Utils.WriteLine("パスワードの送信");

                        //   buf1 = Encoding.UTF8.GetBytes(Utils.Password);
                        //   stream.Write(buf1, 0, buf1.Length);
                        //   Console.WriteLine("以下サーバへ送信");
                        object[] send_data = new object[2];
                        send_data[0] = type;
                        send_data[1] = data;
                        buf1 = Utils.ObjectToByteArray(send_data);//Encoding.UTF8.GetBytes(type);
                        stream.Write(buf1, 0, buf1.Length);

                      //  buf1 = Encoding.UTF8.GetBytes(type);
                      //  stream.Write(data, 0, data.Length);

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

        static public bool ReceiveMessage(string target_ip,out IPEndPoint end_point ,out ConnectType type , out byte[] data)
        {
         //   IPAddress host1 = IPAddress.Any;//System.Net.Dns.GetHostEntry("localhost").AddressList[0];;
        //    int port1 = PortNum;
        //    IPEndPoint ipe1 = new IPEndPoint(host1, port1);
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
                            //recvline = reg.Replace(Encoding.UTF8.GetString(buf), "");
                          //  Utils.WriteLine("recieve_type:" + recvline.ToString());
                            //data = recvline;
                            type = (ConnectType)Utils.ByteArrayToObject(buf);//recvline;
                            //if (recvline.Equals("q"))
                            //    break;
                             Array.Clear(buf, 0, buf.Length);
                        }

                        if (ok && stream.Read(buf, 0, buf.Length) > 0)
                        {
                            //recvline = reg.Replace(Encoding.UTF8.GetString(buf), "");
                         //   Utils.WriteLine("recieve_data:"+buf.ToString());
                            //data = recvline;
                            data = buf;
                            //if (recvline.Equals("q"))
                            //    break;
                           // Array.Clear(buf, 0, buf.Length);
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
