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


namespace Client
{
    public partial class login_form : Form
    {
        public login_form()
        {
            InitializeComponent();
            label1.BackColor = Color.Transparent;
           
        }
        private string userName;
        private TcpClient tcpClient;
        private IPAddress ipServer = IPAddress.Parse("192.168.1.5");
        StreamReader sReader;
        StreamWriter sWriter;
        private const int port = 9999;
        private void connect_btn_Click(object sender, EventArgs e)
        {
            this.tcpClient = new TcpClient();
            this.tcpClient.Connect(new IPEndPoint(ipServer, port)); 
            this.sWriter = new StreamWriter(tcpClient.GetStream());
            sWriter.AutoFlush = true;
            this.sReader = new StreamReader(tcpClient.GetStream());
            sWriter.WriteLine(username_rtb.Text);
            string response =sReader.ReadLine();
            if (response == "OK")
            {
                
                this.userName = username_rtb.Text;
                load_room_form loadroom = new load_room_form(userName,tcpClient);
                this.Hide();
                if (!loadroom.IsDisposed)
                    loadroom.Show();
            }
            else 
            {
                MessageBox.Show(response);
                username_rtb.Text = "";
            }    

        }
    }
}
