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
    public partial class play_game_form : Form
    {
        public play_game_form(string username,TcpClient tcpClient,string enemyname,string roomname)
        {
            InitializeComponent();
            label1.BackColor = Color.Transparent;
            turn_label.BackColor = Color.Transparent;
            clearBoard();
            this.username = username;
            this.enemyName = enemyname;
            this.myroomName = roomname;
            this.tcpClient = tcpClient;
            networkStream = tcpClient.GetStream();
            sWriter = new StreamWriter(networkStream);
            sReader = new StreamReader(networkStream);
            sWriter.AutoFlush = true;
            //Start thread để nhân phản hồi từ server
            if ( this.enemyName=="")
                UpdateChatHistoryThreadSafe("Hệ thống: Hãy đợi đối thủ vào phòng!");
            else
                UpdateChatHistoryThreadSafe("Hệ thống: Đối thủ của bạn là: "+this.enemyName);
            this.clientThread = new Thread(() =>clientRecv(tcpClient));
            clientThread.Start();
        }
        private load_room_form loadroom;
        private string username;
        private Thread clientThread;
        private TcpClient tcpClient;
        private StreamWriter sWriter;
        private StreamReader sReader;
        private NetworkStream networkStream;
        //private bool stopTcpClient = false;
        private const int BufferSize = 2048;
        private string myShape;
        private string enemyShape;
        private string enemyName;
        private bool myTurn = false;
        private string myroomName;
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
        }
        private bool checkEqual(Button one, Button two, Button three)
        {
            if (one.BackgroundImage != null && two.BackgroundImage != null && three.BackgroundImage != null)
            {
                bool check1 = one.BackgroundImage.Size == two.BackgroundImage.Size;
                bool check2 = one.BackgroundImage.Size == three.BackgroundImage.Size;
                bool result = check1 & check2;
                return result;
            }
            else
                return false;
        }
        // Kiểm tra xem có thắng hay không
        private bool checkWin()
        {
            if (checkEqual(button1, button2, button3) == true && button1.BackgroundImage != null)
                return true;
            else if (checkEqual(button4, button5, button6) == true && button4.BackgroundImage != null)
                return true;
            else if (checkEqual(button7, button8, button9) == true && button7.BackgroundImage != null)
                return true;
            else if (checkEqual(button1, button4, button7) == true && button1.BackgroundImage != null)
                return true;
            else if (checkEqual(button2, button5, button8) == true && button2.BackgroundImage != null)
                return true;
            else if (checkEqual(button3, button6, button9) == true && button3.BackgroundImage != null)
                return true;
            else if (checkEqual(button1, button5, button9) == true && button1.BackgroundImage != null)
                return true;
            else if (checkEqual(button3, button5, button7) == true && button3.BackgroundImage != null)
                return true;
            else 
                return false;
        }
        //check xem có hòa hay không
        private bool checkDraw()
        {
            if (button1.BackgroundImage != null && button2.BackgroundImage != null && button3.BackgroundImage != null && button4.BackgroundImage != null && button5.BackgroundImage != null && button6.BackgroundImage != null && button7.BackgroundImage != null && button8.BackgroundImage != null && button9.BackgroundImage != null)
                return true;
            return false;

        }
        // Xóa ban cờ, làm bàn cờ về ban đầu
        private void safeClearBoard()
        {
            // dùng Invoke để tránh lỗi cross-thread
            button1.BeginInvoke((Action)(() => { button1.Enabled = false; }));
            button1.BeginInvoke((Action)(() => { button1.BackgroundImage = null; }));
            button2.BeginInvoke((Action)(() => { button2.Enabled = false; }));
            button2.BeginInvoke((Action)(() => { button2.BackgroundImage = null; }));
            button3.BeginInvoke((Action)(() => { button3.Enabled = false; }));
            button3.BeginInvoke((Action)(() => { button3.BackgroundImage = null; }));
            button4.BeginInvoke((Action)(() => { button4.Enabled = false; }));
            button4.BeginInvoke((Action)(() => { button4.BackgroundImage = null; }));
            button5.BeginInvoke((Action)(() => { button5.Enabled = false; }));
            button5.BeginInvoke((Action)(() => { button5.BackgroundImage = null; }));
            button6.BeginInvoke((Action)(() => { button6.Enabled = false; }));
            button6.BeginInvoke((Action)(() => { button6.BackgroundImage = null; }));
            button7.BeginInvoke((Action)(() => { button7.Enabled = false; }));
            button7.BeginInvoke((Action)(() => { button7.BackgroundImage = null; }));
            button8.BeginInvoke((Action)(() => { button8.Enabled = false; }));
            button8.BeginInvoke((Action)(() => { button8.BackgroundImage = null; }));
            button9.BeginInvoke((Action)(() => { button9.Enabled = false; }));
            button9.BeginInvoke((Action)(() => { button9.BackgroundImage = null; }));
        }
        private void clearBoard()
        { 
            button1.BackgroundImage = null;
            button1.Enabled = false;
            button2.BackgroundImage = null;
            button2.Enabled = false;
            button3.BackgroundImage = null;
            button3.Enabled = false;
            button4.BackgroundImage = null;
            button4.Enabled = false;
            button5.BackgroundImage = null;
            button5.Enabled = false;
            button6.BackgroundImage = null;
            button6.Enabled = false;
            button7.BackgroundImage = null;
            button7.Enabled = false;
            button8.BackgroundImage = null;
            button8.Enabled = false;
            button9.BackgroundImage = null;
            button9.Enabled = false;
        }
        // Đóng băng bàn cờ chờ người chơi kia đi
        private void freezeBoard()
        {
            // dùng Invoke để tránh lỗi cross-thread
            button1.BeginInvoke((Action)(() => { button1.Enabled = false; }));
            button2.BeginInvoke((Action)(() => { button2.Enabled = false; }));
            button3.BeginInvoke((Action)(() => { button3.Enabled = false; }));
            button4.BeginInvoke((Action)(() => { button4.Enabled = false; }));
            button5.BeginInvoke((Action)(() => { button5.Enabled = false; }));
            button6.BeginInvoke((Action)(() => { button6.Enabled = false; }));
            button7.BeginInvoke((Action)(() => { button7.Enabled = false; }));
            button8.BeginInvoke((Action)(() => { button8.Enabled = false; }));
            button9.BeginInvoke((Action)(() => { button9.Enabled = false; }));

            /*button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
            button6.Enabled = false;
            button7.Enabled = false;
            button8.Enabled = false;
            button9.Enabled = false;*/
        }
        // Bỏ đóng băng bàn cờ
        private void unfreezeBoard()
        {
            // dùng Invoke để tránh lỗi cross-thread
            if (button1.BackgroundImage == null)
            {
                button1.BeginInvoke((Action)(() => { button1.Enabled = true; }));
                //button1.Enabled=true;
            }
            if (button2.BackgroundImage == null)
            {
                button2.BeginInvoke((Action)(() => { button2.Enabled = true; }));
                //button2.Enabled = true;
            }
            if (button3.BackgroundImage == null)
            {
                button3.BeginInvoke((Action)(() => { button3.Enabled = true; }));
                //button3.Enabled = true;
            }
            if (button4.BackgroundImage == null)
            {
                button4.BeginInvoke((Action)(() => { button4.Enabled = true; }));
                //button4.Enabled = true;
            }
            if (button5.BackgroundImage == null)
            {
                button5.BeginInvoke((Action)(() => { button5.Enabled = true; }));
                // button5.Enabled = true;
            }
            if (button6.BackgroundImage == null)
            {
                button6.BeginInvoke((Action)(() => { button6.Enabled = true; }));
                //button6.Enabled = true;
            }
            if (button7.BackgroundImage == null)
            {
                button7.BeginInvoke((Action)(() => { button7.Enabled = true; }));
               // button7.Enabled = true;
            }
            if (button8.BackgroundImage == null)
            {
                button8.BeginInvoke((Action)(() => { button8.Enabled = true; }));
              //  button8.Enabled = true;
            }
            if (button9.BackgroundImage == null)
            {
                button9.BeginInvoke((Action)(() => { button9.Enabled = true; }));
               // button9.Enabled = true;
            }
        }
        // Tạo paket
        private string createPacket(MessageType msgType, string sender, string receiver, string content=null, string anothercontent = null)
        {
            string headerAndpayload = msgType.ToString() + "@@" + sender + "@@" + receiver + "@@" + content + "@@" + anothercontent;

            return headerAndpayload;
        }
        // Đọc packet
        private string[] readPacket(string packet)
        {
            string[] result = packet.Split("@@");
            return result;
        }

        // Nếu thắng thì gọi hàm này
        private void sendWin(int index)
        {
            string headerAndpayload = createPacket(MessageType.Win,this.username, this.enemyName, index.ToString(),this.myroomName) ;
            this.sWriter.WriteLine(headerAndpayload);
        }
        // Nếu đi gọi hàm nàyz
        private void sendMove(int index)
        {
            string headerAndpayload = createPacket(MessageType.Move,this.username, this.enemyName, index.ToString()) ;
            this.sWriter.WriteLine(headerAndpayload);
        }
        private void sendDraw(int index)
        {
            string headerAndpayload = createPacket(MessageType.Draw, this.username, this.enemyName, index.ToString(), this.myroomName);
            this.sWriter.WriteLine(headerAndpayload);

        }
        // Nếu muốn gửi tin nhắn gọi hàm này
        private void sendMessage(string msg)
        {
            try
            {       
                this.sWriter.AutoFlush = true;
                this.sWriter.WriteLine(createPacket(MessageType.Text, this.username, this.enemyName, msg));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

            }            
            
        }
        // Hàm này dùng để update hình ảnh nước đi của người chơi kia
        private void updateEnemyShape(int index)
        {
            if (index == 1)
            {
                if (this.enemyShape == "X")
                    button1.BeginInvoke((Action)(() => { button1.BackgroundImage = Properties.Resources.x__1_; }));
                else
                    button1.BeginInvoke((Action)(() => { button1.BackgroundImage = Properties.Resources.o; }));
            }
            else if (index == 2)
            {
                if (this.enemyShape == "X")
                    button2.BeginInvoke((Action)(() => { button2.BackgroundImage = Properties.Resources.x__1_; }));
                else
                    button2.BeginInvoke((Action)(() => { button2.BackgroundImage = Properties.Resources.o; }));
            }
            else if (index == 3)
            {
                if (this.enemyShape == "X")
                    button3.BeginInvoke((Action)(() => { button3.BackgroundImage = Properties.Resources.x__1_; }));
                else
                    button3.BeginInvoke((Action)(() => { button3.BackgroundImage = Properties.Resources.o; }));
            }
            else if (index == 4)
            {
                if (this.enemyShape == "X")
                    button4.BeginInvoke((Action)(() => { button4.BackgroundImage = Properties.Resources.x__1_; }));
                else
                    button4.BeginInvoke((Action)(() => { button4.BackgroundImage = Properties.Resources.o; }));
            }
            else if (index == 5)
            {
                if (this.enemyShape == "X")
                    button5.BeginInvoke((Action)(() => { button5.BackgroundImage = Properties.Resources.x__1_; }));
                else
                    button5.BeginInvoke((Action)(() => { button5.BackgroundImage = Properties.Resources.o; }));
            }
            else if (index == 6)
            {
                if (this.enemyShape == "X")
                    button6.BeginInvoke((Action)(() => { button6.BackgroundImage = Properties.Resources.x__1_; }));
                else
                    button6.BeginInvoke((Action)(() => { button6.BackgroundImage = Properties.Resources.o; }));
            }
            else if (index == 7)
            {
                if (this.enemyShape == "X")
                    button7.BeginInvoke((Action)(() => { button7.BackgroundImage = Properties.Resources.x__1_; }));
                else
                    button7.BeginInvoke((Action)(() => { button7.BackgroundImage = Properties.Resources.o; }));
            }
            else if (index == 8)
            {
                if (this.enemyShape == "X")
                    button8.BeginInvoke((Action)(() => { button8.BackgroundImage = Properties.Resources.x__1_; }));
                else
                    button8.BeginInvoke((Action)(() => { button8.BackgroundImage = Properties.Resources.o; }));
            }
            else if (index == 9)
            {
                if (this.enemyShape == "X")
                    button9.BeginInvoke((Action)(() => { button9.BackgroundImage = Properties.Resources.x__1_; }));
                else
                    button9.BeginInvoke((Action)(() => { button9.BackgroundImage = Properties.Resources.o; }));
            }
        }
        // Hàm này dùng để start thread lắng nghe phản hồi từ server
        private void clientRecv(TcpClient tcpClient)
        {
            StreamWriter sWriter = new StreamWriter(tcpClient.GetStream());
            StreamReader sReader = new StreamReader(tcpClient.GetStream());
            sWriter.AutoFlush = true;
            try
            {
                while (tcpClient.Connected)
                {
                    Application.DoEvents();
                    string readMessage = sReader.ReadLine();
                    string[] message = readPacket(readMessage);
                    MessageType msgType = (MessageType)Enum.Parse(typeof(MessageType), message[0], true);
                    if (msgType==MessageType.Text)
                    {
                        UpdateChatHistoryThreadSafe(this.enemyName+": "+message[3]);
                    }   
                    else if (msgType==MessageType.Move)
                    {
                        updateEnemyShape(int.Parse(message[3]));
                        this.myTurn = true;
                        unfreezeBoard();
                        updateSafeTurnLabel("Tới lượt bạn");
                    }   
                    else if ( msgType==MessageType.Draw)
                    {
                        updateEnemyShape(int.Parse(message[3]));
                        MessageBox.Show("Hòa nhau!");
                        safeClearBoard();
                        ready_btn.BeginInvoke((Action)(() => { ready_btn.Enabled = true; }));
                        freezeBoard();
                        myTurn = false;
                    }    
                    else if ( msgType==MessageType.Win)
                    {
                        updateEnemyShape(int.Parse(message[3]));
                        MessageBox.Show("Bạn thua!");
                        ready_btn.BeginInvoke((Action)(() => { ready_btn.Enabled = true; }));
                        safeClearBoard();
                        freezeBoard();
                        myTurn = false;
                    }    
                    else if ( msgType==MessageType.noticeEnemyReady)
                    {
                        UpdateChatHistoryThreadSafe("Hệ thống: " + enemyName + " đã sẵn sàng! : ");
                    }    
                    else if(msgType==MessageType.noticeEnemyJoinRoom)
                    {
                        this.enemyName = message[3];
                        UpdateChatHistoryThreadSafe("Hệ thống: "+enemyName+ " đã vào phòng!");
                    }   
                    else if (msgType==MessageType.noticeEnemyLeaveRoom)
                    {
                        if(ready_btn.Enabled==false)
                        {
                            safeClearBoard();
                            myTurn = false;
                            MessageBox.Show("Bạn thắng!");
                            ready_btn.BeginInvoke((Action)(() => { ready_btn.Enabled = true; }));
                        }    
                        UpdateChatHistoryThreadSafe("Hệ thống: Đối thủ của bạn đã rời phòng!");
                        this.enemyName = "";
                    }    
                    else if(msgType==MessageType.startGame)
                    {
                        UpdateChatHistoryThreadSafe("Hệ Thống: Game bắt đầu!");
                        
                        this.myShape = message[3];
                        if (this.myShape == "X")
                            this.enemyShape = "O";
                        else
                            this.enemyShape = "X";
                        if (message[4] == "Your turn")
                        {
                            this.myTurn = true;
                            unfreezeBoard();
                            updateSafeTurnLabel("Tới lượt bạn");
                        }
                        else
                            updateSafeTurnLabel("Lượt của đối thủ");
                        //UpdateChatHistoryThreadSafe("Hình của bạn: " + this.myShape);
                        //UpdateChatHistoryThreadSafe("Hình của đối thủ: " + this.enemyShape);

                    }    
                }
            }
            catch (SocketException ex)
            {
                MessageBox.Show(ex.Message);
                tcpClient.Close();
            }
        }
        private delegate void SafeCallDelegate(string text);

        // Hàm để update chat
        private void UpdateChatHistoryThreadSafe(string text)
        {
            if (screen_chat_rtb.InvokeRequired)
            {
                var d = new SafeCallDelegate(UpdateChatHistoryThreadSafe);
                screen_chat_rtb.Invoke(d, new object[] { text });
            }
            else
            {
                screen_chat_rtb.Text += text+"\r\n";
            }
        }
        private void updateSafeTurnLabel(string text)
        {
            if (this.turn_label.InvokeRequired)
            {
                var d = new SafeCallDelegate(updateSafeTurnLabel);
                turn_label.Invoke(d, new object[] { text });
            }
            else
            {
                this.turn_label.Text = text;
            }
        }
        // Nut gửi message
        private void send_mes_btn_Click(object sender, EventArgs e)
        {
            sendMessage(type_chat_rtb.Text);
            UpdateChatHistoryThreadSafe("You: "+type_chat_rtb.Text);
            type_chat_rtb.Text = "";

        }
        private void button1_Click(object sender, EventArgs e)
        {
            // Nếu là lượt đi của mình
            if (myTurn)
            {
                // Update hình ảnh nước đi
                if (this.myShape == "X")
                    button1.BackgroundImage = Properties.Resources.x__1_;
   
                else
                    button1.BackgroundImage = Properties.Resources.o; 
                // Kiểm tra nước đi đó mình thắng hay chưa, nếu thắng gửi win cho server, clear bàn cờ về ban đầu
                if (this.checkWin())
                {
                    sendWin(1);
                    MessageBox.Show("Bạn thắng!");
                    safeClearBoard();
                    myTurn = false;
                    updateSafeTurnLabel("Xin vui lòng đợi");
                    ready_btn.BeginInvoke((Action)(() => { ready_btn.Enabled = true; }));
                }//Nếu hòa nhau
                else if (this.checkDraw())
                {
                    sendDraw(1);
                    MessageBox.Show("Hòa nhau!");
                    safeClearBoard();
                    freezeBoard();
                    myTurn = false;
                    updateSafeTurnLabel("Xin vui lòng đợi");
                    ready_btn.BeginInvoke((Action)(() => { ready_btn.Enabled = true; }));

                }
                // Nếu chưa thắng thì gửi move cho server
                else
                {
                    sendMove(1);
                    freezeBoard();
                    this.myTurn = false;
                    updateSafeTurnLabel("Lượt đối thủ");
                }
                
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (myTurn)
            {
                if (this.myShape == "X")
                    button2.BackgroundImage = Properties.Resources.x__1_;

                else
                    button2.BackgroundImage = Properties.Resources.o;
                if (this.checkWin())
                {
                    sendWin(2);
                    MessageBox.Show("Bạn thắng!");
                    safeClearBoard();
                    freezeBoard();
                    myTurn = false;
                    updateSafeTurnLabel("Xin vui lòng đợi");
                    ready_btn.BeginInvoke((Action)(() => { ready_btn.Enabled = true; }));
                }
                else if (this.checkDraw())
                {
                    sendDraw(2);
                    MessageBox.Show("Hòa nhau!");
                    safeClearBoard();
                    freezeBoard();
                    myTurn = false;
                    updateSafeTurnLabel("Xin vui lòng đợi");
                    ready_btn.BeginInvoke((Action)(() => { ready_btn.Enabled = true; }));

                }
                else
                {
                    sendMove(2);
                    freezeBoard();
                    this.myTurn = false;
                    updateSafeTurnLabel("Lượt đối thủ");
                }

            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (myTurn)
            {
                if (this.myShape == "X")
                    button3.BackgroundImage = Properties.Resources.x__1_;

                else
                    button3.BackgroundImage = Properties.Resources.o;
                if (this.checkWin())
                {
                    sendWin(3);
                    MessageBox.Show("Bạn thắng!");
                    safeClearBoard();
                    freezeBoard();
                    updateSafeTurnLabel("Xin vui lòng đợi");
                    ready_btn.BeginInvoke((Action)(() => { ready_btn.Enabled = true; }));
                }
                else if (this.checkDraw())
                {
                    sendDraw(3);
                    MessageBox.Show("Bạn thắng!");
                    safeClearBoard();
                    freezeBoard();
                    myTurn = false;
                    updateSafeTurnLabel("Xin vui lòng đợi");
                    ready_btn.BeginInvoke((Action)(() => { ready_btn.Enabled = true; }));

                }
                else
                {
                    sendMove(3);
                    freezeBoard();
                    this.myTurn = false;
                    updateSafeTurnLabel("Lượt đối thủ");
                }

            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (myTurn)
            {
                if (this.myShape == "X")
                    button4.BackgroundImage = Properties.Resources.x__1_;

                else
                    button4.BackgroundImage = Properties.Resources.o;
                if (this.checkWin())
                {
                    sendWin(4);
                    MessageBox.Show("Bạn thắng!");
                    safeClearBoard();
                    freezeBoard();
                    updateSafeTurnLabel("Xin vui lòng đợi");
                    ready_btn.BeginInvoke((Action)(() => { ready_btn.Enabled = true; }));
                }
                else if (this.checkDraw())
                {
                    sendDraw(4);
                    MessageBox.Show("Hòa nhau!");
                    safeClearBoard();
                    freezeBoard();
                    myTurn = false;
                    updateSafeTurnLabel("Xin vui lòng đợi");
                    ready_btn.BeginInvoke((Action)(() => { ready_btn.Enabled = true; }));

                }
                else
                {
                    sendMove(4);
                    freezeBoard();
                    this.myTurn = false;
                    updateSafeTurnLabel("Lượt đối thủ");
                }

            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (myTurn)
            {
                if (this.myShape == "X")
                    button5.BackgroundImage = Properties.Resources.x__1_;

                else
                    button5.BackgroundImage = Properties.Resources.o;
                if (this.checkWin())
                {
                    sendWin(5);
                    MessageBox.Show("Bạn thắng!");
                    safeClearBoard();
                    freezeBoard();
                    updateSafeTurnLabel("Xin vui lòng đợi");
                    ready_btn.BeginInvoke((Action)(() => { ready_btn.Enabled = true; }));
                }
                else if (this.checkDraw())
                {
                    sendDraw(5);
                    MessageBox.Show("Hòa nhau!");
                    safeClearBoard();
                    freezeBoard();
                    myTurn = false;
                    updateSafeTurnLabel("Xin vui lòng đợi");
                    ready_btn.BeginInvoke((Action)(() => { ready_btn.Enabled = true; }));

                }
                else
                {
                    sendMove(5);
                    freezeBoard();
                    this.myTurn = false;
                    updateSafeTurnLabel("Lượt đối thủ");
                }

            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (myTurn)
            {
                if (this.myShape == "X")
                    button6.BackgroundImage = Properties.Resources.x__1_;

                else
                    button6.BackgroundImage = Properties.Resources.o;
                if (this.checkWin())
                {
                    sendWin(6);
                    MessageBox.Show("Bạn thắng!");
                    safeClearBoard();
                    freezeBoard();
                    updateSafeTurnLabel("Xin vui lòng đợi");
                    ready_btn.BeginInvoke((Action)(() => { ready_btn.Enabled = true; }));
                }
                else if (this.checkDraw())
                {
                    sendDraw(6);
                    MessageBox.Show("Hòa nhau!");
                    safeClearBoard();
                    freezeBoard();
                    myTurn = false;
                    updateSafeTurnLabel("Xin vui lòng đợi");
                    ready_btn.BeginInvoke((Action)(() => { ready_btn.Enabled = true; }));
                }
                else
                {
                    sendMove(6);
                    freezeBoard();
                    this.myTurn = false;
                    updateSafeTurnLabel("Lượt đối thủ");
                }

            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (myTurn)
            {
                if (this.myShape == "X")
                    button7.BackgroundImage = Properties.Resources.x__1_;

                else
                    button7.BackgroundImage = Properties.Resources.o;
                if (this.checkWin())
                {
                    sendWin(7);
                    MessageBox.Show("Bạn thắng!");
                    safeClearBoard();
                    freezeBoard();
                    updateSafeTurnLabel("Xin vui lòng đợi");
                    ready_btn.BeginInvoke((Action)(() => { ready_btn.Enabled = true; }));
                }
                else if (this.checkDraw())
                {
                    sendDraw(7);
                    MessageBox.Show("Hòa nhau!");
                    safeClearBoard();
                    freezeBoard();
                    myTurn = false;
                    updateSafeTurnLabel("Xin vui lòng đợi");
                    ready_btn.BeginInvoke((Action)(() => { ready_btn.Enabled = true; }));

                }
                else
                {
                    sendMove(7);
                    freezeBoard();
                    this.myTurn = false;
                    updateSafeTurnLabel("Lượt đối thủ");
                }

            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (myTurn)
            {
                if (this.myShape == "X")
                    button8.BackgroundImage = Properties.Resources.x__1_;

                else
                    button8.BackgroundImage = Properties.Resources.o;
                if (this.checkWin())
                {
                    sendWin(8);
                    MessageBox.Show("Bạn thắng!");
                    safeClearBoard();
                    freezeBoard();
                    updateSafeTurnLabel("Xin vui lòng đợi");
                    ready_btn.BeginInvoke((Action)(() => { ready_btn.Enabled = true; }));
                }
                else if (this.checkDraw())
                {
                    sendDraw(8);
                    MessageBox.Show("Hòa nhau!");
                    safeClearBoard();
                    freezeBoard();
                    myTurn = false;
                    updateSafeTurnLabel("Xin vui lòng đợi");
                    ready_btn.BeginInvoke((Action)(() => { ready_btn.Enabled = true; }));

                }
                else
                {
                    sendMove(8);
                    freezeBoard();
                    this.myTurn = false;
                    updateSafeTurnLabel("Lượt đối thủ");
                }

            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (myTurn)
            {
                if (this.myShape == "X")
                    button9.BackgroundImage = Properties.Resources.x__1_;

                else
                    button9.BackgroundImage = Properties.Resources.o;
                if (this.checkWin())
                {
                    sendWin(9);
                    MessageBox.Show("Bạn thắng!");
                    safeClearBoard();
                    freezeBoard();
                    myTurn = false;
                    updateSafeTurnLabel("Xin vui lòng đợi");
                    ready_btn.BeginInvoke((Action)(() => { ready_btn.Enabled = true; }));
                }
                else if(this.checkDraw())
                {
                    sendDraw(9);
                    MessageBox.Show("Hòa nhau!");
                    safeClearBoard();
                    freezeBoard();
                    myTurn = false;
                    updateSafeTurnLabel("Xin vui lòng đợi");
                    ready_btn.BeginInvoke((Action)(() => { ready_btn.Enabled = true; }));

                }    
                else
                {
                    sendMove(9);
                    freezeBoard();
                    this.myTurn = false;
                    updateSafeTurnLabel("Lượt đối thủ");
                }

            }
        }

        private void ready_btn_Click(object sender, EventArgs e)
        {
            try
            {
                UpdateChatHistoryThreadSafe("Hệ thống: Bạn đã sẵn sàng!");
                sWriter.WriteLine(createPacket(MessageType.ready, this.username, this.enemyName,myroomName));
                ready_btn.BeginInvoke((Action)(() => { ready_btn.Enabled = false; }));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void leave_room_btn_Click(object sender, EventArgs e)
        {
            sWriter.WriteLine(createPacket(MessageType.leaveRoom,username,enemyName,myroomName));
            //load_room_form loadroom = new load_room_form(username,tcpClient);
            this.Close();
            sReader.Close();
            sWriter.Close();
            this.tcpClient.Dispose();
            this.clientThread.Interrupt();

            //load_room_form loadroom = new load_room_form(username, this.tcpClient);
            //loadroom.Show();
           /*TcpClient tcpclient = new TcpClient();
            tcpclient.Connect(new IPEndPoint(IPAddress.Parse("192.168.1.5"), 9999));
            StreamReader sr = new StreamReader(tcpclient.GetStream());
            StreamWriter sw = new StreamWriter(tcpclient.GetStream());
            sw.WriteLine(username);
            sr.ReadLine();
            load_room_form loadroom = new load_room_form(username, tcpClient);
            loadroom.Show();*/

        }
    }      
    
}
