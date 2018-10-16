using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;


namespace RemoteControlMaster
{

    public partial class ServerParameters : Form
    {
        private addServer _addS;    /* Delegate to add the New Server in the main window Server List. These delegates art set in the main window. */
        private restore_hook _rh;   /* Delegate to restore the keyboard hook after the parameters insertion. */

        /*
         * Properties to get/set delegates from the main window.
         */
        public addServer AddS
        {
            get { return _addS; }
            set { _addS = value; }
        }

        public restore_hook rh
        {
            get { return _rh; }
            set { _rh = value; }
        }

        /*
         * Server parameters constructor
         */
        public ServerParameters()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TcpClient tcpcl;    /*tcp Client that will connect to the new server*/
            message msg;

            try
            {
                string ip_addr = textBox1.Text.ToString();  /* Get the IP address inserted by the user */
                string password = textBox3.Text.ToString(); /* Get the password inserted by the user */

                UInt16 port = Convert.ToUInt16(textBox2.Text.ToString());   /* Convert the port string to UInt16 type */
                IPAddress ip = IPAddress.Parse(ip_addr);                    /* Convert the string IP to IPAddress type */

                tcpcl = new TcpClient();
                tcpcl.Connect(ip, port);    /* Here we try to connect with the new Server. */

                byte[] data = Encoding.Unicode.GetBytes(password);  /* Here we are converting the password to a byte array */
                SocketCommunication.sendBufferToSocket(data, tcpcl);/* Then we send the byte array with the password to the server. */

                data = SocketCommunication.receiveBufferFromSocket(tcpcl);  /* Here we're receiving the server reply, that shows if the password is correct. */

                msg = SocketCommunication.rawDeserialize<message>(data);    /* Here we deserialize the received buffer. */

                if (msg.messType == messageType.CONNECTION_REFUSED ) /* If the message contains "Connection Refused", the password is probably wrong. */
                    throw new Exception("Wrong Password");

                if (msg.messType != messageType.CONNECTION_ACCEPTED) /* The server has to reply with a "CONNECTION_ACCEPTED message" */
                    throw new Exception("Error!");

                AddS(tcpcl.Client.RemoteEndPoint.ToString(), tcpcl);    /* Here we're calling the delegate in order to add the new server to the main window server list. */

                this.Close();   /* After the server connection, the window will close "automatically" */
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /*
         * Form closing event Handler.
         */
        private void ServerParameters_FormClosing(object sender, FormClosingEventArgs e)
        {
            rh(this, new EventArgs()); /* When the form is closing, we have to restore the hook procedure in order to catch keyboard events. */
        }
    }
}
