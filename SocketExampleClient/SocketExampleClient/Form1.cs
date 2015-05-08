using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO.Compression;


namespace SocketExampleClient
{
    #region Delegates

    public delegate void addServer(String ip, TcpClient cl);
    public delegate void fileSet(StringCollection sc);
    public delegate void imgSet(Image img);
    public delegate void txtSet(String str);
    public delegate void audioSet(Stream audio);
    public delegate void updatePBarRec(float value);
    public delegate void updatePBarSent(float value);
    public delegate void makeVisible(ProgressBar pb);
    public delegate void makeInvisible(ProgressBar pb);

    public delegate int keyboardHookProc(int code, int wParam, ref keyboardHookStruct lParam);

    public delegate void restore_hook(object sender, EventArgs e);

    #endregion

    public partial class Form1 : Form
    {
        #region DllImport


        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SetClipboardViewer(IntPtr hWndNewViewer);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetClipboardOwner();

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);
        /*
         * Windows API that adds a application-defined hook procedure into the hook chain.
         * We have to deal low-level keyboard events.
         */
        [DllImport("user32.dll")]
        static extern IntPtr SetWindowsHookEx(int idHook, IntPtr callback, IntPtr hInstance, uint threadId);
        
        /*
         * This function removes the procedure-handle hInstance from the hook chain.
         */
        [DllImport("user32.dll")]
        static extern bool UnhookWindowsHookEx(IntPtr hInstance);

        /*
         * This function passes the hook information to the next hook procedure in the current hook chain.
         */
        [DllImport("user32.dll")]
        static extern int CallNextHookEx(IntPtr idHook, int nCode, int wParam, ref keyboardHookStruct lParam);

        /*
         * This Windows API loads the specified module into the address space of the calling process.
         */
        [DllImport("kernel32.dll")]
        static extern IntPtr LoadLibrary(string lpFileName);

        /*
         * This constants are used to process the keyboards events.
         */
        const int WH_KEYBOARD_LL = 13; /* With this constant, SetWindowsHookEx will install a hook procedure that monitors all low-level keyboard input events. */
        const int WM_KEYDOWN = 0x100;
        const int WM_KEYUP = 0x101;
        const int WM_SYSKEYDOWN = 0x104;
        const int WM_SYSKEYUP = 0x105;

        #endregion

        /*
         *  Map containing all hosts to control. Only if an host is reachable will be inserted in this map,
         *  otherwise the user will be advised runtime.
         */ 
        private Dictionary<String, TcpClient> remoteHosts;

        private TcpClient targetServer;            /*Socket connected to target server (server receiving keyboard-mouse inputs)*/

        private Thread cliplistener;        /*Thread listening for receive clipboard updates*/

        private Thread clipSender;          /*Thread that send clipboard data when required*/

        /*
         *  AutoResetEvent variable, used to fire the asynchronous send operation
         *  when the user perform a double click on a server in the server list.
         */
        private AutoResetEvent send = new AutoResetEvent(false);

        /*
         *  Handle to the hook procedure. In this case, we are monitoring low-level keyboard input events. 
         */
        private IntPtr _hhook = IntPtr.Zero;

        /*
         * Handle to the DLL containing the hook procedure pointed to by the lpfn parameter.
         */
        private IntPtr hInstance;
        private IntPtr handle;
        private IntPtr hWndNextWindow = IntPtr.Zero;

        //  Property linked to _hhook.
        public IntPtr hhook
        {
            get { return _hhook; }
            set { _hhook = value; }
        }

        /*
         *  Delegate for the hook procedure. Using this delegate it is possible to pass to the unmanaged code
         *  a function pointer, with Marshal library.
         */
        private keyboardHookProc del;
        
        /*
         * Main window Constructor.
         */
        public Form1()
        {
            InitializeComponent();
            
            this.remoteHosts = new Dictionary<string, TcpClient>(); /* Instantiation of servers map*/
            SocketCommunication.UpdatePBSent += this.updateSendbar; /* Add to delegate "UpdatePBSent" a function that updates the SendBar. */
            SocketCommunication.UpdatePBRec += this.updateRecbar;   /* Add to delegate "UpdatePBRec" a function that updates the RecBar. */

            del += LowLevelKeyboardProc;                            /* Initialization of keyboardHookProc (delegate) with this procedure. */
            hInstance = LoadLibrary("user32.dll");                  /* hInstance is initialized with the handle to the user32 module. */

            Clipboard.SetText(" ");
            handle = GetClipboardOwner();

            hWndNextWindow = (IntPtr)SetClipboardViewer(this.Handle);
            
            this.cliplistener = new Thread(clipListFunction);       /* Background thread that listens for server connections linked to clipboard updates.*/
            this.cliplistener.IsBackground = true;                  /* This Thread works in background! */
                                                                     
            this.cliplistener.SetApartmentState(ApartmentState.STA);/* Clipboard access require thread be of
                                                                     * type Single Thread Apartment, because of
                                                                     * clipboard isn't thread safe.
                                                                     */

            this.cliplistener.Start();                               /* The cliboard listener thread starts! */

            this.clipSender = new Thread(clipSenderFunction);        /* Background thread that wait until a new Server is selected, and when this happens
                                                                      * sends the clipboard content toward the selected server.
                                                                      */
            this.clipSender.IsBackground = true;                     /* This thread works in background! */
            this.clipSender.SetApartmentState(ApartmentState.STA);   /* Clipboard access require thread be of
                                                                      * type Single Thread Apartment, because of
                                                                      * clipboard isn't thread safe.
                                                                      */
            this.clipSender.Start();                                 /* The clipboard sender thread Starts! */
        }

        /*
         * Function called by the delegate addS in the Window ServerParameters. It adds a new Server in
         * the Server List (List Box1).
         */
        public void addServer(String ip, TcpClient cl)
        {
            remoteHosts.Add(ip, cl);                                    /* Here we add a Server in the internal "remoteHosts" Dictionary */
            ServerList.Items.Add(cl.Client.RemoteEndPoint.ToString());  /* Here we add the server "IPAddress:Port" in the external ServerList */
            ServerList.Refresh();                                       /* We have to refresh the server list in order to see quickly the addition */
        }

        #region Mouse Events

        /*
         * Listener for mouse movements in the "MouseArea box". Here we capture each mouse movement and we send this event (mouse coordinates update)
         * toward the server actually connected.
         */
        private void MouseArea_MouseMove(object sender, MouseEventArgs e)
        {
            message msg = new message(); /* Message struct to send to the Server */

            try
            { 
                if (targetServer != null && targetServer.Connected) /* If no server is actually connected, we have to sent nothing */
                {                   
                    msg.act.eventype = EVENTYPE.MOUSE_MOVEMENT; /* Type of event: mouse movement */

                    /*
                     * Mouse coordinates in the MouseArea box, that maps the server screen. The mouse coordinates ti send with SendInput are mapped
                     * in the value range 0 - 65536.
                     */
                    msg.act.x = (Cursor.Position.X - MouseArea.Location.X) * 65536 / MouseArea.Width;
                    msg.act.y = (Cursor.Position.Y - MouseArea.Location.Y) * 65536 / MouseArea.Height;

                    msg.messType = messageType.ACTION; /* Type of message: mouse or keyboard action, that must be sent via SendInput by the Server. */

                    /*
                     * Here we send all data to the server via Socket.
                     */
                    byte[] buffer = SocketCommunication.rawSerialize(msg); /*Raw Serialize: Struct Serialization*/
                    SocketCommunication.sendBufferToSocket(buffer, targetServer); /*Buffer sending*/
                }
            }
            catch (Exception ex) /*If some exception occurs, we will show a message with the Error Description*/
            {
                MessageBox.Show(ex.Message);
            }
        }

        /*
         * Listener for mouse wheel in the "MouseArea box". Here we capture each mouse wheel in MouseArea box and we will send this event (mouse wheel)
         * toward the server actually connected.
         */
        private void MouseArea_MouseWheel(object sender, MouseEventArgs e)
        {
            message msg = new message();

            try
            {
                if (targetServer != null && targetServer.Connected) /* If no server is actually connected, we have to sent nothing */
                {
                    msg.act.eventype = EVENTYPE.MOUSE_WHEEL; /* Event type: Mouse Wheel! */
                    msg.act.wheelMovement = e.Delta;        /* e.Delta contains the number of wheel rotations. */

                    msg.messType = messageType.ACTION;      /* Type of message: mouse or keyboard action, that must be sent via SendInput by the Server. */

                    /*
                     * Here we send all data to the server via Socket.
                     */
                    byte[] buffer = SocketCommunication.rawSerialize(msg); /*Raw Serialize: Struct Serialization*/
                    SocketCommunication.sendBufferToSocket(buffer, targetServer); /*Buffer sending*/
                }
            }
            catch (Exception ex) /*If some exception occurs, we will show a message with the Error Description*/
            {
                MessageBox.Show(ex.Message);
            }
        }

        /*
         * Listener for "mouse downs" in the "MouseArea box". Here we capture each mouse down event in MouseArea box
         * and we will send this event (mouse left/right down) toward the server actually connected.
         */
        private void MouseArea_MouseDown(object sender, MouseEventArgs e)
        {
            message msg = new message();

            if (targetServer != null && targetServer.Connected) /* If no server is actually connected, we have to sent nothing */
            {
                try
                {
                    /*
                     * Here we have to Identify which mouse button has been pressed. MouseEventArgs contains this kind of information. 
                     */
                    if (e.Button == MouseButtons.Left) 
                    {
                        msg.act.eventype = EVENTYPE.LEFT_DOWN; /*Eventype struct contains both left_down and right_down events.*/
                    }
                    else if (e.Button == MouseButtons.Right)
                    {
                        msg.act.eventype = EVENTYPE.RIGHT_DOWN;
                    }
                    else if (e.Button == MouseButtons.Middle)
                    {
                        msg.act.eventype = EVENTYPE.MIDDLE_DOWN;
                    }

                    msg.messType = messageType.ACTION;          /* Type of message: mouse or keyboard action, that must be sent via SendInput by the Server. */

                    /*
                     * Here we send all data to the server via Socket.
                     */
                    byte[] buffer = SocketCommunication.rawSerialize(msg); /*Raw Serialize: Struct Serialization*/
                    SocketCommunication.sendBufferToSocket(buffer, targetServer); /*Buffer sending*/
                }
                catch (Exception ex) /*If some exception occurs, we will show a message with the Error Description*/
                {
                    MessageBox.Show(ex.Message.ToString());
                }
            }
        }

        /*
         * Listener for "mouse ups" in the "MouseArea box". Here we capture each mouse up event in MouseArea box
         * and we will send this event (mouse left/right up) toward the server actually connected.
         */
        private void MouseArea_MouseUp(object sender, MouseEventArgs e)
        {
            message msg = new message();

            try
            {
                if (targetServer != null && targetServer.Connected) /* If no server is actually connected, we have to sent nothing */
                {
                    /*
                     * Here we have to Identify which mouse button has been released. MouseEventArgs contains this kind of information. 
                     */
                    if (e.Button == MouseButtons.Left)
                    {
                        msg.act.eventype = EVENTYPE.LEFT_UP;
                    }
                    else if (e.Button == MouseButtons.Right)
                    {
                        msg.act.eventype = EVENTYPE.RIGHT_UP;
                    }
                    else if (e.Button == MouseButtons.Middle)
                    {
                        msg.act.eventype = EVENTYPE.MIDDLE_UP;
                    }

                    msg.messType = messageType.ACTION; /* Type of message: mouse or keyboard action, that must be sent via SendInput by the Server. */

                    /*
                     * Here we send all data to the server via Socket.
                     */
                    byte[] buffer = SocketCommunication.rawSerialize(msg); /*Raw Serialize: Struct Serialization*/
                    SocketCommunication.sendBufferToSocket(buffer, targetServer); /*Buffer sending*/
                }
            }
            catch (Exception ex) /*If some exception occurs, we will show a message with the Error Description*/
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        #endregion
        /*
         * The system calls this function every time a new keyboard input event is about to be posted into this thread input queue.
         * In fact, this function is the keyboard listener, that send keyboards Events toward the current targete server.
         */
        public int LowLevelKeyboardProc(int code, int wParam, ref keyboardHookStruct lParam)
        {
            message msg = new message();
            byte[] buffer;

            try
            {
                /* A code the hook procedure uses to determine how to process the message. If nCode is less than zero,
                 * the hook procedure must pass the message to the CallNextHookEx function without further processing and
                 * should return the value returned by CallNextHookEx. Obviously, to send the message tcpcl has to be connected and not null :)
                 */
                if (code >= 0 && targetServer != null && targetServer.Connected) 
                {
                        if ((wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN)) /* keydown case. We send toward the server both normal keys and system keys. */
                        {
                            msg.act.eventype = EVENTYPE.KEY_DOWN;
                        }
                        else if ((wParam == WM_KEYUP || wParam == WM_SYSKEYUP))/* Keyup case. */
                        {
                            msg.act.eventype = EVENTYPE.KEY_UP;
                        }

                        msg.act.keypress = lParam.vkCode; /* vkCode of the kay that has been pressed. */
                        msg.messType = messageType.ACTION;

                        buffer = SocketCommunication.rawSerialize(msg);
                        SocketCommunication.sendBufferToSocket(buffer, targetServer);

                        return 1;
                }
                else
                {
                    /*
                     * We do not perform any operation and we call the next function in the HookProcChain.
                     */
                    return CallNextHookEx(hhook, code, wParam, ref lParam);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return CallNextHookEx(hhook, code, wParam, ref lParam); /*If an exception*/
            }
        }

        /*
         * Event Listener for clicks on menu to add a new Server.
         */
        private void addServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ServerParameters sp = new ServerParameters(); /*Here we will create a new Form that allow user to insert all the Server Parameters*/
            sp.AddS += this.addServer;                    /* When a new Server is added, ih has to update the server list in the main window calling addServer function using a delegate. */
            sp.rh += Form1_Resize;                        /* Calling Form1_Resize in sp, we restore the Keyboard Hook, that has to be deactivate now in order
                                                           * to allow user to type the Server Info.
                                                           */
            /*
             * Here we remove this window from the hook chain.
             */
            if (hhook != IntPtr.Zero)
                if (UnhookWindowsHookEx(hhook))
                    hhook = IntPtr.Zero;

            sp.Show(); /* Last and more important thing, the new window has to be shown to the user! */
        }

        /*
         * Performing a double click in a server in the Server List, we will select this server as Target, and we will update
         * the server clipboard with the clipboard data actually present in the client.
         */
        private void ServerList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            message msg = new message();

            if (ServerList.SelectedItem == null) return; /* If we perform a double click where there is no item, return here. */

            TcpClient cl = remoteHosts[ServerList.SelectedItem.ToString()]; /*The new Server is the current selected server.*/
            
            byte[] buffer;

            try
            {
                if (this.targetServer == null || !this.targetServer.Equals(cl)) /* If we are selecting the same target server, we do not have to send TARGET_CHANGED and TARGET messages */
                {
                    if (this.targetServer != null && this.targetServer.Connected) /*Here we are informing the old server that it is not longer the client target*/
                    {
                        msg.messType = messageType.TARGET_CHANGED; /* Type of message: Target_Changed! */

                        buffer = SocketCommunication.rawSerialize(msg);
                        SocketCommunication.sendBufferToSocket(buffer, targetServer);
                    }
                    /*
                     * Here we change the Server and we will inform the new server that it is the new client target!
                     */

                    this.targetServer = cl; /*The new tcpcl is the selected Server cl*/

                    msg.messType = messageType.TARGET; /* Type of message: Target! */

                    buffer = SocketCommunication.rawSerialize(msg);
                    SocketCommunication.sendBufferToSocket(buffer, targetServer);
                }
                /*
                 * Performing this operation we will unblock the sender thread, that Will analyze the clipboard and will send clipboard
                 * data toward the new Server. Note that if we perform a double click on the current target server, this operation will be anyway performed.
                 */
                send.Set();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /*
         * This Event handler make visible the contextMenuStrip, clicking with the mouse right button on a Server.
         */
        private void ServerList_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right) /* We have to perform a right click to show the strip menu ;) */
            {
                ServerList.SelectedIndex = ServerList.IndexFromPoint(e.Location); /* Here we select the server at the mouse location. If at the mouse location there is
                                                                                   * no servers, ServerList.IndexFromPoint(e.Location) will return -1.*/
                if (ServerList.SelectedIndex != -1)
                {
                    contextMenuStrip1.Show(ServerList, e.Location); /* Show the context menu at the current mouse position. */
                }
            }
        }

        /*
         * Event handler for clicks on the "Disconnect" in the contextMenuStrip
         */
        private void disconnectServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TcpClient toRemove = this.remoteHosts[ServerList.SelectedItem.ToString()]; /* The client to remove is the client current selected */


            if (this.targetServer != null && this.targetServer.Equals(toRemove)) /*If we are removing the actual target server we have to assign null to tcpcl in order to avoid*/
                  this.targetServer = null;                               /*transfers toward a disconnected server.*/


            try
            {
                if (toRemove != null && toRemove.Connected) /* Standards checks */
                {
                    message msg = new message();
                    msg.messType = messageType.DISCONNECT; /* Message Type: Disconnect! */

                    byte[] buffer = SocketCommunication.rawSerialize(msg);
                    SocketCommunication.sendBufferToSocket(buffer, toRemove);
                }

                toRemove.Close(); /*Here we are closing the socket connected with the server to remove from ServerList*/
                this.remoteHosts.Remove(ServerList.SelectedItem.ToString()); /* We have to remove the server from tme remoteHosts dictionary */
                this.ServerList.Items.Remove(ServerList.SelectedItem.ToString()); /* And from the extern list! */

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }  

        /*
         * Button to close the application
         */
        private void X_Click(object sender, EventArgs e)
        {
            /*
             * First thing: sending a DISCONNECT message to all connected servers.
             */
            if(remoteHosts != null)
                foreach (TcpClient tc in this.remoteHosts.Values)
                {
                    if (tc.Connected) /*If tc is still an active server*/
                    {
                        message msg = new message();
                        msg.messType = messageType.DISCONNECT;

                        byte[] buffer = SocketCommunication.rawSerialize(msg);
                        SocketCommunication.sendBufferToSocket(buffer, tc);

                        tc.Close(); /* Socket close! */
                    }
                }
            /*
             * Here we close the application.
             */
            Application.Exit();
        }

        protected override void WndProc(ref Message m)
        {
            switch ((WM)m.Msg)
            {
                case WM.DRAWCLIPBOARD:
                    if (targetServer != null && targetServer.Connected && handle != GetClipboardOwner())
                    {                      
                        send.Set();
                    }

                    SendMessage(hWndNextWindow, m.Msg, m.WParam, m.LParam);
                    break;

                case WM.CHANGECBCHAIN:
                    if (m.WParam == hWndNextWindow)
                        hWndNextWindow = m.LParam;
                    else
                        SendMessage(hWndNextWindow, m.Msg, m.WParam, m.LParam);
                    break;

                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        /*
         * Function of ClipListener thread. This threda waits for actual target server connections to receive clipboard data.
         */
        public void clipListFunction()
        {
            TcpClient clipClient;       /*Socket connected with server client to receive data*/
            TcpListener clipServer;     /*Socket listening for clipboard updates*/

            try
            {
                clipServer = new TcpListener(IPAddress.Any, 9999); /* TcpListener listening on port 9999. */
                clipServer.Start(); /* clipServer starts listening for incoming connection requests*/

                while (true) /*This thread runs while the application exits*/
                {
                    try
                    {
                        Invoke(new makeInvisible(makeInvisible), remoteCopyProgressBar); /* This function make invisible the progress bar, if it is visible */
                        using (clipClient = clipServer.AcceptTcpClient()) /* Here we accept a pending connection request. */
                        {
                            if (targetServer != null && targetServer.Connected) /*If tcpcl is not connected we must not accept connections from other servers that are not the actual target*/
                            {
                                IPEndPoint remoteClipIpEndPoint = clipClient.Client.RemoteEndPoint as IPEndPoint; /* Ip address of server that is sending the clipboard update */
                                IPEndPoint remoteIpEndPoint = targetServer.Client.RemoteEndPoint as IPEndPoint;          /* Ip address of server that actually is the target*/

                                if (remoteClipIpEndPoint.Address.Equals(remoteIpEndPoint.Address)) /* The server that is sending the clipboard update must be the target server */
                                {
                                    Invoke(new makeVisible(makeVisible), remoteCopyProgressBar);   /* We have to starting the transfer. This progress bar shows the transfer progress. */
                                    remoteCopy(clipClient);                                        /* Function that receives clipboard data */
                                }
                            }

                            clipClient.Close();
                        }
                    }
                    catch (SocketException) /* IO and Socket exceptions do not stop this thread, that continue to listen on the clipServer socket. If a server crashes, the client must continue to listen for other servers connections*/
                    {    
                        continue;
                    }
                    catch (IOException)
                    {
                        continue;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /*
         * Function that receives the clipboard data and sets the client clipboard with the received data.
         */
        public void remoteCopy(TcpClient clipClient)
        {
            message msg;

            /*
             * Here we receive the struct that contains the data type that is going to arrive.
             */
            byte[] data = SocketCommunication.receiveBufferFromSocket(clipClient);
            msg = SocketCommunication.rawDeserialize<message>(data);

            /*
             * If we do not receive a messageType == remote_copy, there is an error! 
             */
            if (msg.messType != messageType.REMOTE_COPY)
                throw new Exception("Formato messaggio copia errato.");

            switch (msg.cinf.ct)
            {
                case clipboardType.TEXT: /*The clipboard data is text*/

                    data = SocketCommunication.receiveChunks(clipClient, msg.cinf.size); /*Function that receives data, saving chuncks of 1024 Bytes*/

                    /* We have to call this functions with an invoke in order to set the main thread as clipboard owner */
                    Invoke(new txtSet(Clipboard.SetText), Encoding.Unicode.GetString(data)); /* Here we set the clipboard with the text received, encoding the received bytes with unicode format. */
                    
                    break;

                case clipboardType.IMAGE: /* The clipboard data is an Image */

                    data = SocketCommunication.receiveChunks(clipClient, msg.cinf.size); /*Function that receives data, saving chuncks of 1024 Bytes*/

                    Image i = (Image)SocketCommunication.deserialize(data); /* Received bytes deserialization. The received object is of type Image*/

                    Invoke(new imgSet(Clipboard.SetImage), i); /* Here We set the clipboard with the received Image */

                    break;

                case clipboardType.FILES: /* FILES case. To sent multiple files, we have to compress all files in a zip Archive. Therefore, now we must unzip the received archive */

                    String path = SocketCommunication.receiveFile(clipClient, msg.cinf.size); /* This function returns the path of the received archive */
                    String dirName = Path.Combine(path, "ReceiveTemp");                       /* Here we create the path to the folder that will contain all files */

                    if (!Directory.Exists(dirName)) /*If this folder doesn't exist...*/
                    {
                        Directory.CreateDirectory(dirName); /* we have to create it! */
                    }
                    else
                    {
                        Directory.Delete(dirName, true); /* The  directory already exists, and probably contains files...*/
                        Directory.CreateDirectory(dirName); /* so we have to delete the directory and to recreate it, in order to save memory. */
                    }

                    ZipFile.ExtractToDirectory(Path.Combine(path, "receivedArchive.zip"), dirName); /* We unzip the archive in the folder "ReceiveTemp" */
                    File.Delete(Path.Combine(path, "receivedArchive.zip"));                         /*Then we delete the received zip, in order to save memory.*/

                    string[] sc = Directory.GetFileSystemEntries(dirName);  /*This functions returns a string array with the filenames of each element in the new folder*/
                    StringCollection strColl = new StringCollection();      /* Clipboard.SetFileDropList requires a StringCollection object */

                    strColl.AddRange(sc);                                   /* Here we add to strColl all strings contained in sc */                       

                    Invoke(new fileSet(Clipboard.SetFileDropList), strColl);/* We have to call this functions with an invoke in order to set the main thread as clipboard owner */
                    
                    break;

                case clipboardType.AUDIO: /* The clipboard data is an audio stream. */

                    data = SocketCommunication.receiveChunks(clipClient, msg.cinf.size); /*Function that receives data, saving chuncks of 1024 Bytes*/
                    Stream audio = (Stream)SocketCommunication.deserialize(data);        /* Received bytes deserialization. The received object is of type Stream*/
                    Invoke(new audioSet(Clipboard.SetAudio), audio);                     /* Here We set the clipboard with the received Audio stream */

                    break;
                    
                case clipboardType.NO_VALID_DATA:                                       /*If the clipboard doesn't contain a valid data format, we receive this message.*/

                    data = SocketCommunication.receiveChunks(clipClient, msg.cinf.size);

                    break;
            }

        }

        /*
         * Function of clipSender thread. This thread will stay blocked waiting for a condition variable set, performed by the main thread
         * when the user makes a double click on a server.
         */
        private void clipSenderFunction()
        {
            byte[] data;

            while (true) /*This function runs until the Application is active.*/
            {                
                try
                {
                    Invoke(new makeInvisible(makeInvisible), remotePasteProgressBar); /* If a remote paste progress bar is visible, we have to make it invisible */

                    send.WaitOne();  /*Wait for a notification from the main thread*/
                        
                    Invoke(new makeVisible(makeVisible), remotePasteProgressBar); /* Because we have to send data toward the targetServer, we show the progressBar to inform the user on the transfer */  

                    IPEndPoint remoteServerIpEndPoint = targetServer.Client.RemoteEndPoint as IPEndPoint; /*Get ip address of the server actually connected*/

                    message msg = new message();

                    msg.messType = messageType.REMOTE_PASTE; /* The operation to perform is a remote paste! */

                    if (Clipboard.ContainsText()) /* The clipboard contains text! */
                    {
                        String text = Clipboard.GetText(); /* Here we get the text copied in the clipboard */

                        data = Encoding.Unicode.GetBytes(text); /* We have to get the byte array of the text, in order to send this through a socket. */

                        msg.cinf.ct = clipboardType.TEXT; /* We are sending text! */
                        msg.cinf.size = data.Length;      /* We have to sent even the amount of text bytes in order to receive the whole array */
                    }
                    else if (Clipboard.ContainsImage()) /*The clipboard contains an Image!*/
                    {
                        Image img = Clipboard.GetImage(); /*We have to get the image object..*/

                        data = SocketCommunication.serialize(img); /* and to serialize it! */

                        msg.cinf.ct = clipboardType.IMAGE;          /*The data is an Image*/
                        msg.cinf.size = data.Length;
                    }
                    else if (Clipboard.ContainsAudio()) /* The clipboard contains an audio stream! */
                    {
                        Stream audio = Clipboard.GetAudioStream();  /*We have to get the stream object..*/

                        data = SocketCommunication.serialize(audio); /* and to serialize it! */

                        msg.cinf.ct = clipboardType.AUDIO;          /* Tha clipboard data is an audio stream! */
                        msg.cinf.size = data.Length;                                                                       
                    }
                    else if (Clipboard.ContainsFileDropList()) /* The clipboard contains files! */
                    {
                        string archPath = SocketCommunication.createArchive(); /*This function creates an archive with all files in the clipboard*/

                        FileInfo fi = new FileInfo(archPath);                   /* Object containing info about the archive */

                        if (fi.Length > 10 * 1024 * 1024)       /* If the zip size is more than 10 MB */
                        {
                            DialogResult confirmResult = MessageBox.Show("Sei davvero sicuro di voler trasferire più di 10 MB di dati?",
                                                            "Conferma Trasferimento",
                                                            MessageBoxButtons.YesNo); /* We have to show a confirm button, in order to avoid network overhead. */

                            if (confirmResult == DialogResult.No) /* If the user doesn't want to transfer the file.. */
                            {
                                continue;                         /* We have to continue */
                            }
                        }

                        msg.cinf.ct = clipboardType.FILES;                      /* The clipboard data is a FileDropList! */
                        msg.cinf.size = (int)fi.Length;                         /* Size of the archive that is going to be sent */

                        using (TcpClient clientSendData = new TcpClient())      /*Socket used to send clipboard data to server */
                        {
                            clientSendData.Connect(remoteServerIpEndPoint.Address, 9998); /* We have to connect to the target server, port 9998 */

                            SocketCommunication.sendBufferToSocket(SocketCommunication.rawSerialize(msg), clientSendData);
                            SocketCommunication.sendFileToSocket(clientSendData, archPath);

                            clientSendData.Close();
                        }

                        continue;
                    }
                    else
                    {
                        msg.cinf.ct = clipboardType.NO_VALID_DATA;
                        data = new byte[4];
                    }

                    if (data.Length > 10 * 1024 * 1024) /* If the clipboard data is bigger than 10 MB */
                    {
                        DialogResult confirmResult = MessageBox.Show("Sei davvero sicuro di voler trasferire più di 10 MB di dati?",
                                                        "Conferma Trasferimento",
                                                        MessageBoxButtons.YesNo); /* We have to show a confirm button, in order to avoid network overhead. */

                        if (confirmResult == DialogResult.No) /* If the user doesn't want to transfer the file.. */
                        {
                            continue;                         /* We have to continue */
                        }
                    }

                    using (TcpClient clientSendData = new TcpClient())                  /*Socket used to send clipboard data to server */
                    {
                        clientSendData.Connect(remoteServerIpEndPoint.Address, 9998);   /* We have to connect to the target server, port 9998 */

                        SocketCommunication.sendBufferToSocket(SocketCommunication.rawSerialize(msg), clientSendData); /* Struct Sending */
                        SocketCommunication.sendChunks(clientSendData, data);           /* Data sending */

                        clientSendData.Close();
                    }
                }
                catch (SocketException) /* The exceptions that occurs will be catched here and the thread will not terminate! This thread has to remain active while the application ends */
                {  
                    continue;
                }
                catch (IOException)
                {
                    continue;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }
        }

        /*
         * Event handler for clicks on the minimize button.
         */
        private void MinimizeButton_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized; /*The form obviously will be minimized!*/

            if (hhook != IntPtr.Zero) /*If the keyboard hook is active*/
            {
                if (UnhookWindowsHookEx(hhook)) /*It is necessary unhook the hookProcedure, in order to allow user to write when the client is minimized!*/
                    hhook = IntPtr.Zero;        /* Here we will make hhook pointer null */
                else
                    MessageBox.Show("Error occurred!");
            }
        }

        /*
         * This function is called by a SocketCommunication delegate. 
         */
        private void updateRecbar(float value)
        {
            Invoke(new updatePBarRec(updatePBarRec), value); /* Because normally is not the main thread that perform the receive operation this function will do an invoke operation. */
        }

        /*
         * Function that update the state of the remoteCopyProgressbar.
         */
        private void updatePBarRec(float value)
        {
            remoteCopyProgressBar.Value = (int)value; /* Here we update the value ..*/
            remoteCopyProgressBar.Refresh();          /* then we refresh the progressBar to show to the user the transfer state */
        }

        private void updateSendbar(float value)
        {
            Invoke(new updatePBarSent(updatePBarSent), value); /* Because normally is not the main thread that perform the receive operation this function will do an invoke operation. */
        }

        /*
         * Function that update the state of the remotePasteProgressbar.
         */
        private void updatePBarSent(float value)
        {
            remotePasteProgressBar.Value = (int)value; /* Here we update the value ..*/
            remotePasteProgressBar.Refresh();          /* then we refresh the progressBar to show to the user the transfer state */
        }

        /*
         * This function makes visible the progress bar pb 
         */
        private void makeVisible(ProgressBar pb)
        {
            if (pb.Name.Equals("remotePasteProgressBar")) /* If the progress bar is the "remotePasteProgresBar.." */
                RemotePaste.Visible = true;               /* We have to show the label "RemotePaste" */
            else
                RemoteCpy.Visible = true;                 /* Else we have to show the label RemoteCopy */

            pb.Visible = true;                            /* Obviously we have to show the progress bar too! */
        }

        /*
         * This function makes invisible the progress bar pb 
         */
        private void makeInvisible(ProgressBar pb)
        {
            if (pb.Name.Equals("remotePasteProgressBar")) /* If the progress bar is the "remotePasteProgresBar.." */
                RemotePaste.Visible = false;              /* We have to hide the label "RemotePaste" */
            else
                RemoteCpy.Visible = false;                /* Else we have to hide the label RemoteCopy */

            pb.Visible = false;                           /* Obviously we have to hide the progress bar too... */
            pb.Value = 0;                                 /*..and to reset the Value property */
        }

        /*
         * When we resize the form (if the WindowState is "maximized") we have to set the keyboard hook to capture all keyboards events, even the syskeys.
         */
        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Maximized && hhook == IntPtr.Zero) /*If the windowState is maximized.. */
            {
                hhook = SetWindowsHookEx(WH_KEYBOARD_LL, Marshal.GetFunctionPointerForDelegate(del), hInstance, 0); /* We have to set the hook procedure in order to capture all keyboard events */
            }
        }

        private void MouseArea_MouseEnter(object sender, EventArgs e)
        {
            Cursor.Hide();
        }

        private void MouseArea_MouseLeave(object sender, EventArgs e)
        {
            Cursor.Show();
        }

    }
}
