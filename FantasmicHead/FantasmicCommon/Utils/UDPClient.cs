using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FantasmicCommon.Utils
{
    public class UDPBroadcastClient
    {
        /*private void ListenBroadcastMessage()
        {
            // 送受信に利用するポート番号
            var port = 8000;

            // ブロードキャストを監視するエンドポイント
            var remote = new IPEndPoint(IPAddress.Any, port);

            // UdpClientを生成
            var client = new UdpClient(port);

            // データ受信を待機（同期処理なので受信完了まで処理が止まる）
            // 受信した際は、 remote にどの IPアドレス から受信したかが上書きされる
            var buffer = client.Receive(ref remote);

            // 受信データを変換
            var data = Encoding.UTF8.GetString(buffer);

            // 受信イベントを実行
            this.OnReceive(data);
        }

        private void OnReceive(string data)
        {
            // 受信処理...
        }

        private void SendBroadcastMessage(string data)
        {
            // 送受信に利用するポート番号
            var port = 8000;

            // 送信データ
            var buffer = Encoding.UTF8.GetBytes(data);

            // ブロードキャスト送信
            var client = new UdpClient(port);
            client.EnableBroadcast = true;
            client.Client.Connect(new IPEndPoint(IPAddress.Broadcast, port));
            client.Client.Send(buffer, buffer.Length);
            client.Client.;
        }*/
    }
}
