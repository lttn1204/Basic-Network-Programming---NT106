using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace Client
{
    public partial class load_room_form : Form
    {
        public load_room_form(string username, TcpClient tcpClient)
        {
            InitializeComponent();
            input_name_room_label.BackColor = Color.Transparent;
            this.username = username;
            this.tcpClient = tcpClient;
            sReader = new StreamReader(tcpClient.GetStream());
            sWriter = new StreamWriter(tcpClient.GetStream());
            sWriter.AutoFlush = true;

        }
        private string username;
        private TcpClient tcpClient;
        private StreamReader sReader;
        private StreamWriter sWriter;
        private string enemyname = "";
        private string room;
        private delegate void SafeCallDelegate(string text);
        public enum
          MessageType
        {
            Move,
            Text,
            Win,
            Draw,
            setEnemy,
            createRoom,
            joinRoom,
            responseCreateRoom,
            responseJoinRoom,
            leaveRoom,
            noticeEnemyLeaveRoom,
            startGame,
            ready,
            newRoom,
            noticeEnemyJoinRoom,
            noticeEnemyReady,
            requestListRoom,
            responseListRoom,
            closeConnect,
        }
        private string[] readPacket(string content)
        {
            string[] result = content.Split("@@");
            return result;
        }
        private string createPacket(MessageType msgType, string sender, string receiver, string content = null, string anothercontent = null)
        {
            string headerAndpayload = msgType.ToString() + "@@" + sender + "@@" + receiver + "@@" + content + "@@" + anothercontent;

            return headerAndpayload;
        }

        // Hàm để update chat
        private void UpdateChatHistoryThreadSafe(string text)
        {
            if (list_room_rtb.InvokeRequired)
            {
                var d = new SafeCallDelegate(UpdateChatHistoryThreadSafe);
                list_room_rtb.Invoke(d, new object[] { text });
            }
            else
            {
                list_room_rtb.Text += text + "\r\n";
            }
        }
        private void create_room_btn_Click(object sender, EventArgs e)
        {
            sWriter.AutoFlush = true;
            sWriter.WriteLine(createPacket(MessageType.createRoom, username, "Server", enter_room_rtb.Text));
            enter_room_rtb.Text = "";
            string response =sReader.ReadLine();
            string[] message = readPacket(response);
            if (message[3] == "Roomname already exist"|| message[3]== "Please enter the name room")
                MessageBox.Show(message[3]);
            else if (message[3] == "Create room OK")
            {
                play_game_form playgame = new play_game_form(username, tcpClient,"" ,message[4]);
                this.Hide();
                if (!playgame.IsDisposed)
                    playgame.Show();
            }    
        }

        private void join_roon_btn_Click(object sender, EventArgs e)
        {
            sWriter.AutoFlush = true;
            sWriter.WriteLine(createPacket(MessageType.joinRoom, username, "Server", enter_room_rtb.Text));
            string response = sReader.ReadLine();
            string[] message = readPacket(response);
            if (message[3] == "Room already 2 player" || message[3] == "Room do not exist")
                MessageBox.Show(message[3]);
            else if (message[3] == "Join room OK")
            {

                enemyname = message[4];
                play_game_form playgame = new play_game_form(username, tcpClient,enemyname, enter_room_rtb.Text);
                this.Close();
                if (!playgame.IsDisposed)
                    playgame.Show();
            }    
        }

        private void request_list_roon_btn_Click(object sender, EventArgs e)
        {
            sWriter.WriteLine(createPacket(MessageType.requestListRoom, username, "Server"));
            string response = sReader.ReadLine();
            string[] message = readPacket(response);
            if (message[3] == "")
            {
                UpdateChatHistoryThreadSafe("Chưa có phòng được tạo");
            }
            else
            {
                list_room_rtb.Text = "";
                string[] listRoom = message[3].Split("||");
                foreach (string nameRoom in listRoom)
                    UpdateChatHistoryThreadSafe(nameRoom);
            }
        }
    }
}
