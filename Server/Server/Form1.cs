using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
    
        private const int PORT_NUMBER = 9999;
        private String nameServer;
        private TcpListener tcpListener;
        private IPAddress ipAddr;
        struct Room
        {
            public int numberOfPlayer;
            public string playerOne;
            public string playerTwo;
            public int ready;

        }
        private Socket sock;
        private Dictionary<string, TcpClient> userDict = new Dictionary<string, TcpClient>();
        private Dictionary<string, Room> roomDict = new Dictionary<string, Room>();
        private Thread listenThread;
        private string[] listRoomName;
        private bool running = false;
        private int numberReady = 0;
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
        public void Listen()
        {
            this.nameServer = Dns.GetHostName();
            IPHostEntry ipEnTry = Dns.GetHostEntry(nameServer);
            foreach (IPAddress ip  in  ipEnTry.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork && ip != IPAddress.Loopback)
                    this.ipAddr = ip;
            }
            this.tcpListener = new TcpListener(ipAddr, PORT_NUMBER);
            this.tcpListener.Start();
            
            while(running)
            {
                TcpClient _client = tcpListener.AcceptTcpClient();
                StreamReader sr = new StreamReader(_client.GetStream());
                StreamWriter sw = new StreamWriter(_client.GetStream());
                sw.AutoFlush = true;
                string username = sr.ReadLine();
                if (username=="")
                {
                    sw.WriteLine("Vui lòng nhập username!");
                    _client.Close();
                }
                else
                {
                    if (!userDict.ContainsKey(username))
                    {
                        userDict.Add(username, _client);
                        sw.WriteLine("OK");
                        log_rtb.BeginInvoke((Action)(() => { log_rtb.AppendText(  username + " has login"+ Environment.NewLine); }));
                        Thread clientThread = new Thread(() => ClientRecv(username, _client));
                        clientThread.Start();
                         
                        
                    }
                    else
                    {
                        sw.WriteLine("User name đã tồn tại, vui lòng đặt tên khác!");
                        _client.Close();
                    }
                }
            }
        }
        private string createPacket(MessageType msgType, string sender,string receiver,string content=null,string anothercontent=null)
        {
            string headerAndpayload = msgType.ToString() + "@@" + sender + "@@" + receiver + "@@" + content+"@@"+anothercontent;

            return headerAndpayload;
        }
        private string[] readPacket(string content)
        {
            if (content == null)
                return null;
            else
            {
                string[] result = content.Split("@@");
                return result;
            }
        }
        public void ClientRecv(string username, TcpClient tcpClient)
        {
            StreamReader sReader = new StreamReader(tcpClient.GetStream());
            StreamWriter sWriter = new StreamWriter(tcpClient.GetStream());
            sWriter.AutoFlush = true;
            try
            {
                while (running && tcpClient.Connected)
                {
                    Application.DoEvents();
                    string forwardBuffer = sReader.ReadLine();
                    string[] message = readPacket(forwardBuffer);
                    if (message != null)
                    {
                        // Neu là gửi nước đi, chat, win thì forward
                        MessageType msgType = (MessageType)Enum.Parse(typeof(MessageType), message[0], true);
                        if (msgType == MessageType.Move || msgType == MessageType.Text)
                        {
                            string sender = message[1];
                            string receiver = message[2];
                            string content = message[3];
                            string forwardHeaderAndMessage = createPacket(msgType, sender, receiver, content);
                            if (userDict.TryGetValue(receiver, out TcpClient enemyTcpClient))
                            {
                                StreamWriter enemysWriter = new StreamWriter(enemyTcpClient.GetStream());
                                enemysWriter.AutoFlush = true;
                                enemysWriter.WriteLine(createPacket(msgType,"Server",receiver,content));

                            }
                        }
                        else if  (msgType == MessageType.Win|| msgType==MessageType.Draw)
                        {
                            string sender = message[1];
                            string receiver = message[2];
                            string content = message[3];
                            string roomname = message[4];
                            string forwardHeaderAndMessage = createPacket(msgType, sender, receiver, content);
                            if (userDict.TryGetValue(receiver, out TcpClient enemyTcpClient))
                            {
                                StreamWriter enemysWriter = new StreamWriter(enemyTcpClient.GetStream());
                                enemysWriter.AutoFlush = true;
                                enemysWriter.WriteLine(createPacket(msgType, "Server", receiver, content));

                            }
                            roomDict.TryGetValue(roomname, out Room roomToWinOrDraw);
                            roomToWinOrDraw.ready = 0;
                            roomDict.Remove(roomname);
                            roomDict.Add(roomname, roomToWinOrDraw);
                            log_rtb.BeginInvoke((Action)(() => { log_rtb.AppendText(sender+ " "+ msgType.ToString()+" in " + roomname + Environment.NewLine); }));

                        }    

                        //Nếu gửi tạo phòng 
                        else if (msgType == MessageType.createRoom)
                        {
                            string sender = message[1];
                            string receiver = message[2];
                            string nameRoom = message[3];
                            // log_rtb.Text += msgType.ToString() + nameRoom + Environment.NewLine;
                            if (nameRoom == "")
                            {
                                sWriter.WriteLine(createPacket(MessageType.responseCreateRoom, "Server", sender, "Please enter the name room"));
                            }
                            //Tạo phòng và xác nhận
                            else if (!roomDict.ContainsKey(nameRoom))
                            {
                                Room newRoom;
                                newRoom.numberOfPlayer = 1;
                                newRoom.playerOne = sender;
                                newRoom.playerTwo = "";
                                newRoom.ready = 0;
                                roomDict.Add(nameRoom, newRoom);
                                sWriter.WriteLine(createPacket(MessageType.responseCreateRoom, "Server", sender, "Create room OK",nameRoom));
                                log_rtb.BeginInvoke((Action)(() => { log_rtb.AppendText(sender + " create room: " + nameRoom + Environment.NewLine); }));
                            }
                            // Phòng đã tồn tại
                            else if (roomDict.ContainsKey(nameRoom))
                                sWriter.WriteLine(createPacket(MessageType.responseCreateRoom, "Server", sender, "Roomname already exist"));

                        }
                        // Nếu gửi join phòng
                        else if (msgType == MessageType.joinRoom)
                        {
                            string sender = message[1];
                            string receiver = message[2];
                            string nameRoom = message[3];
                            if (!roomDict.ContainsKey(nameRoom))
                                sWriter.WriteLine(createPacket(MessageType.responseJoinRoom, "Server", sender, "Room do not exist"));
                            else
                            {
                                try
                                {
                                    roomDict.TryGetValue(nameRoom, out Room roomToJoin);
                                    // Phòng đủ người
                                    if (roomToJoin.numberOfPlayer >= 2)
                                    {
                                        sWriter.WriteLine(createPacket(MessageType.responseJoinRoom, "Server", sender, "Room already 2 player"));
                                    }
                                    //Phòng có 1 người
                                    else if (roomToJoin.numberOfPlayer == 1)
                                    {
                                        roomDict.Remove(nameRoom);
                                        sWriter.WriteLine(createPacket(MessageType.responseJoinRoom, "Server", sender, "Join room OK", roomToJoin.playerOne));
                                        log_rtb.BeginInvoke((Action)(() => { log_rtb.AppendText(sender + " join room: " + nameRoom + Environment.NewLine); }));
                                        if (roomToJoin.playerTwo == "")
                                        {
                                            roomToJoin.playerTwo = sender;
                                            string firstUser = roomToJoin.playerOne;
                                            roomToJoin.numberOfPlayer += 1;
                                            userDict.TryGetValue(firstUser, out TcpClient firstUserTcpClient);
                                            StreamWriter firstUserSWriter = new StreamWriter(firstUserTcpClient.GetStream());
                                            roomDict.Add(nameRoom, roomToJoin);
                                            firstUserSWriter.AutoFlush = true;
                                            firstUserSWriter.WriteLine(createPacket(MessageType.noticeEnemyJoinRoom, "Server", firstUser, username));
                                        }    
                                           
                                        else if (roomToJoin.playerOne == "")
                                        {
                                            roomToJoin.playerOne = sender;
                                            string firstUser = roomToJoin.playerTwo;
                                            roomToJoin.numberOfPlayer += 1;
                                            userDict.TryGetValue(firstUser, out TcpClient firstUserTcpClient);
                                            StreamWriter firstUserSWriter = new StreamWriter(firstUserTcpClient.GetStream());
                                            roomDict.Add(nameRoom, roomToJoin);
                                            firstUserSWriter.AutoFlush = true;
                                            firstUserSWriter.WriteLine(createPacket(MessageType.noticeEnemyJoinRoom, "Server", firstUser, username));
                                        }    
                                    }
                                }
                                // Không tìm thấy tên phòng
                                catch
                                {
                                    MessageBox.Show(" Lỗi");
                                }
                            }

                        }
                        // rời phòng
                        else if (msgType == MessageType.leaveRoom)
                        {
                            string sender = message[1];
                            string receiver = message[2];
                            string roomName = message[3];
                            roomDict.TryGetValue(roomName, out Room roomToLeave);
                            if (roomToLeave.playerOne == "" || roomToLeave.playerTwo=="") // Phòng có 1 người
                            {
                                roomDict.Remove(roomName);
                                log_rtb.BeginInvoke((Action)(() => { log_rtb.AppendText("Phòng " + roomName+" đã bị xóa" + Environment.NewLine); }));


                            }
                            else if (roomToLeave.playerOne != "" && roomToLeave.playerTwo != "")//Phòng có 2 người
                            {
                                if (roomToLeave.playerOne==sender)
                                {
                                    roomDict.Remove(roomName);
                                    roomToLeave.numberOfPlayer -= 1;
                                    roomToLeave.playerOne = "";
                                    roomDict.Add(roomName, roomToLeave);
                                    userDict.TryGetValue(roomToLeave.playerTwo, out TcpClient enemyLeaveTcpClient);
                                    StreamWriter enemyLeaveSwriter = new StreamWriter(enemyLeaveTcpClient.GetStream());
                                    enemyLeaveSwriter.AutoFlush = true;
                                    enemyLeaveSwriter.WriteLine(createPacket(MessageType.noticeEnemyLeaveRoom, "Server",roomToLeave.playerTwo, roomToLeave.playerOne, roomName));
                                }    
                                else if ( roomToLeave.playerTwo==sender)
                                {
                                    roomDict.Remove(roomName);
                                    roomToLeave.numberOfPlayer -= 1;
                                    roomToLeave.playerTwo = "";
                                    roomDict.Add(roomName, roomToLeave);
                                    userDict.TryGetValue(roomToLeave.playerOne, out TcpClient enemyLeaveTcpClient);
                                    StreamWriter enemyLeaveSwriter = new StreamWriter(enemyLeaveTcpClient.GetStream());
                                    enemyLeaveSwriter.AutoFlush = true;
                                    enemyLeaveSwriter.WriteLine(createPacket(MessageType.noticeEnemyLeaveRoom, "Server",roomToLeave.playerOne, roomToLeave.playerTwo, roomName));
                                }

                            }
                        }
                        else if (msgType == MessageType.ready)// Nếu sẵn sằng
                        {
                            string sender=message[1];
                            string nameroom = message[3];
                            roomDict.TryGetValue(nameroom, out Room roomToReady);
                            roomToReady.ready += 1;
                            roomDict.Remove(nameroom);
                            roomDict.Add(nameroom, roomToReady);
                            if(roomToReady.numberOfPlayer==2)
                            {
                                string anotherclient = message[2];
                                userDict.TryGetValue(anotherclient, out TcpClient enemyStartTcpClient);
                                StreamWriter enemyStartSwriter = new StreamWriter(enemyStartTcpClient.GetStream());
                                enemyStartSwriter.AutoFlush = true;
                                enemyStartSwriter.WriteLine(createPacket(MessageType.noticeEnemyReady, "Server", anotherclient, username));
                            }    
                            if (roomToReady.ready == 2)//Cả 2 cùng sẵn sàng sẽ cho phép start game
                            {
                                sWriter.WriteLine(createPacket(MessageType.startGame, "Server",username, "X", "Enemy turn"));
                                string anotherclient = message[2];
                                userDict.TryGetValue(anotherclient, out TcpClient enemyStartTcpClient);
                                StreamWriter enemyStartSwriter = new StreamWriter(enemyStartTcpClient.GetStream());
                                enemyStartSwriter.AutoFlush = true;
                                enemyStartSwriter.WriteLine(createPacket(MessageType.startGame, "Server", anotherclient,"X", "Your turn"));
                            }
                        }
                        else if (msgType == MessageType.requestListRoom)// Yêu cầu lấy danh sách phòng
                        {
                            if (roomDict.Count == 0)
                            {
                                sWriter.WriteLine(createPacket(MessageType.responseListRoom, "Server", username, ""));
                            }
                            else
                            {
                                string content = "";
                                foreach (string room in roomDict.Keys)
                                    content += room + "||";
                                sWriter.WriteLine(createPacket(MessageType.responseListRoom, "Server", username, content));
                            }
                        }

                    }
                    else
                    {
                        tcpClient.Close();
                        sReader.Close();
                        sWriter.Close();
                        log_rtb.BeginInvoke((Action)(() => { log_rtb.AppendText(username + " logged out" + Environment.NewLine); }));
                        userDict.Remove(username);
                    }
                }
            }
            catch (SocketException ex)
            {
                sReader.Close();
                sWriter.Close();
                tcpClient.Close();
                log_rtb.BeginInvoke((Action)(() => { log_rtb.AppendText(username+" logged out"+Environment.NewLine); }));
                userDict.Remove(username);

            }
        }
        private void start_server_btn_Click(object sender, EventArgs e)
        {
            listenThread = new Thread(this.Listen);
            MessageBox.Show("Start Server!");
            start_server_btn.Enabled = false;
            this.running = true;
            listenThread.Start();
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.tcpListener.Stop();
            MessageBox.Show("Server đã dừng!");
        }
    }
    ;

}
