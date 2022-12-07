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
    class Connect
    {

        public void FirstSend()
        {
            if (!CheckPort()) return;

            UDP.SendBroadcastMessage(Utils.Password);

            IPEndPoint ip = new IPEndPoint(IPAddress.Any, Utils.PortNum);
            TCP.ReceiveMessage(ip, out string text);
        }

        public void FirstReceive()
        {
            if (!CheckPort()) return;

            UDP.ListenBroadcastMessage(out var ip);

            ip.Port = Utils.PortNum;
            TCP.SendMessage(ip, "data");


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

    }

    static class UDP
    {

        static public void SendBroadcastMessage(string data)
        {
            // 送受信に利用するポート番号
            var port = Utils.PortNum;

            // 送信データ
            var buffer = Encoding.UTF8.GetBytes(data);

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
        static public bool SendMessage(IPEndPoint target,string text)
        {
            
            byte[] buf1;// = new byte[1024];
            //   Regex reg = new Regex("\0");
            bool ok = false;
            try
            {
                using (var client = new TcpClient())
                {
                    Utils.WriteLine("TCPコネクト");

                    client.Connect(target);
                    using (var stream = client.GetStream())
                    {
                        Utils.WriteLine("パスワードの送信");

                        buf1 = Encoding.UTF8.GetBytes(Utils.Password);
                        stream.Write(buf1, 0, buf1.Length);
                        Console.WriteLine("以下サーバへ送信");


                        buf1 = Encoding.UTF8.GetBytes(text);
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

        static public bool ReceiveMessage(IPEndPoint target,out string text)
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

            bool ok = false;
            text = "";
            try
            {
                server = new TcpListener(target);
                Utils.WriteLine("クライアント待ち状態");
                server.Start();

                using (var client = server.AcceptTcpClient())
                {
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
                            recvline = reg.Replace(Encoding.UTF8.GetString(buf), "");
                            Utils.WriteLine(recvline);
                            text = recvline;
                            //if (recvline.Equals("q"))
                            //    break;
                            Array.Clear(buf, 0, buf.Length);
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
