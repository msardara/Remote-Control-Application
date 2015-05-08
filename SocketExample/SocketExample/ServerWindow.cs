using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;
using System.Collections.Specialized;
using System.IO.Compression;
using System.Diagnostics;
using Microsoft.Win32;


namespace SocketExample
{
    #region Delegates

    /*
     * Delegate to edit the connection label. A secondary thread cannot modify directly the label of the main form.
     */
    public delegate void editLabel(Label l, String status);
    
    /*
     *  Delegates to set the clipboard from the main Window.
     */
    public delegate void fileSet(StringCollection sc);          
    public delegate void imgSet(Image img);
    public delegate void txtSet(String str);
    public delegate void audioSet(Stream audio);

    #endregion

    public partial class ServerWindow : Form
    {
        #region DllImport

        /*
         * Platform Invoke. Here we import extern windows api from "user32.dll".
         */

        /*
         * This function sends an input to the system queue. The input is totally described by the INPUT structure.
         */
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint numberOfInputs, [MarshalAs(UnmanagedType.LPArray, SizeConst = 1)] INPUT[] inputs, int sizeOfInputStructure);

        /*
         * This function sets the window in the clipboard viewer chain.
         */
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SetClipboardViewer(IntPtr hWndNewViewer);

        /*
         * Function that sends the message to the next window in the window chain
         */
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

        /*
         * Function to get the current clipboard owner, in order to avoid a double clipboard set.
         */
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetClipboardOwner();

        /*
         * Function that remove the current window from the clipboardViewer window chain.
         */
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

        #endregion

        #region Attributes

        private readonly Object lockObj1 = new Object();
        private TcpClient clientSocket = null;      /*Socket to communicate with client*/
        private TcpListener serverSocket = null;    /*Socket to accept the connection from client*/
        private Thread listener = null;             /*Server thread that waits for client commands*/
        private volatile bool connected = false;    /*
                                                     * Boolean that indicate if the connection is active
                                                     * (connected = true even if the server is listening)
                                                     * and it is used to stop server listening. Due to the use of
                                                     * 2 threads, this variable is volatile.
                                                     */

        private bool target = false;                /* Boolean value that indicates if this server is the Target of the client. */

        private TcpClient clipClient;               /* Tcp client that connects to the client clipboardUpdates listener. */
        private TcpListener clipServer;             /* TcpListener that listen to receive each clipboard update from the client. */
        private TcpClient clipReceiver;             /* TcpClient used with the TcpListener ClipServer */
        
        private Thread clipboardDataSender;         /* Thread that sends the new clipboard data toward  the client */
        private Thread clipboardDataReceiver;       /* Thread that receive the clipboard updates from the client. */

        /*
         * The volatile keyword alerts the compiler that multiple threads will access the _shouldStop data member, and therefore
         * it should not make any optimization
         * assumptions about the state of this member. For more information, see volatile (C# Reference).
         * The use of volatile with the _shouldStop data member allows us to safely access this member from multiple threads
         * without the use of formal thread synchronization techniques, but only because _shouldStop is a bool. This means that
         * only single, atomic operations are necessary to modify _shouldStop. If, however, this data member were a class, struct,
         * or array, accessing it from multiple threads would likely result in intermittent data corruption.
         * Consider a thread that changes the values in an array. Windows regularly interrupts threads in order to allow other
         * threads to execute, so this thread could be halted after assigning some array elements but before assigning others.
         * This means the array now has a state that the programmer never intended, and another thread reading this array may fail
         * as a result.
         */
        private volatile bool enableClipServer = true;  /* Boolean used to Stop the thread that receives the clipboard updates */
        private volatile bool enableClipSender = true;  /* Boolean used to Stop the thread that sends the clipboard updates */

        private AutoResetEvent send = new AutoResetEvent(false);    /* Condition varable used to unblock the sender thread, each time that a new clipboard update is captured */

        private string password = null;             /*The server require a password for the connection*/

        private IntPtr hWndNextWindow = IntPtr.Zero;    /* Handle of the next window in the window chain */

        private IntPtr handle;                          /* Handle of this window in the window chain. */

        private NotifyIcon notifyIcon1;                 /* NotifyIcon in the windows tray area */

        #endregion

        /*
         * Form1 Constructor.
         */
        public ServerWindow(NotifyIcon icon)
        {
            InitializeComponent();
            Clipboard.SetText(" ");         /* Here we are setting the clipboard in order to get the handle of this application in the clipboard owner info. */
            handle = GetClipboardOwner();   /* And we get the handle of the clipboard owner ( the handle of this window, that is not this.Handle. ) */

            notifyIcon1 = icon;             /* Here we receive the notifyIcon object, that has been already create in the main method. */

            notifyIcon1.BalloonTipTitle = "Remote Control Notification";    /* Here we are setting the title pf the BalloonTip */
            
            notifyIcon1.ContextMenu = new ContextMenu(new MenuItem[] /* We are setting the context menu of the notifyIcon with basic commands, (Open the windows/ Exit) */
            {
                new MenuItem("Open", (s, e) => { this.Show(); this.WindowState = FormWindowState.Normal; notifyIcon1.Visible = false; } ), /* Lambda functions with the operations to perform after the click on the MenuItem */
                new MenuItem("Exit", (s, e) => { Application.Exit(); } ),
            });

            notifyIcon1.DoubleClick += notifyIcon1_DoubleClick;     /* This instruction adds to the event listener "DoubleClick" the procedure "notifyIcon1_DoubleClick" */   
            
            /*
             * Here we are setting this application in the list of applications that runs when Windows starts.
             */
            using (RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true)) /* Here we are opening the register key that contain the list of applications that starts automatically */
            {
                if (rk.GetValue(System.AppDomain.CurrentDomain.FriendlyName) == null) /* If the key linked to this application doesn't exist */
                    rk.SetValue(System.AppDomain.CurrentDomain.FriendlyName,"\"" + Application.ExecutablePath.ToString() + "\""); /* We have to set this registry key with the path of 
                                                                                                                                   * the application in order to open it when windows bootstraps */
            }

        }

        
        /*
         * The button1 click initialize server and starts the command listener, the clipboard listener and the clipboard sender threads.
         */
        private void MakeListeningTcpSocket_Click(object sender, EventArgs e)   
        {  
            try
            {
                UInt16 port = Convert.ToUInt16(portTextBox.Text.ToString());       /*Convert the port string to int.
                                                                                 *If the field text doesn't contain a
                                                                                 *number, will be thrown an Exception*/

                hWndNextWindow = (IntPtr)SetClipboardViewer(this.Handle);       /* Set this window in the ClipboardViewersChain. */

                if (passwordTextBox.Text == "")
                    throw new Exception("Invalid Password");                    /*Password can't be null!*/
                
                this.password = passwordTextBox.Text;                                  /*Set the password*/
                
                this.serverSocket = new TcpListener(IPAddress.Any, port);       /*Listener socket allocation*/
                this.listener = new Thread(listener_function);                  /*Listener thread*/
                this.connected = true;                                          /*Now we are connected!*/
                this.listener.IsBackground = true;                              /* This thread works in background! */
                this.listener.SetApartmentState(ApartmentState.STA);            /*Clipboard access require thread be of
                                                                                 *type Single Thread Apartment, because of
                                                                                 *clipboard isn't thread safe.*/
                this.listener.Start();                                          /*The server thread will start here!*/
                this.StopServerButton.Enabled = true;                                    /*Enabling button to stop server*/
                this.MakeListeningTcpSocket.Enabled = false;                    /*Disable button to start server*/
                this.portTextBox.Enabled = false;                                  /*Disable port field, the connection is going to be performed.*/

                this.clipboardDataSender = new Thread(clipSenderFunction);      /* Creation of the clipboard sender thread. */
                this.clipboardDataSender.SetApartmentState(ApartmentState.STA); /*Clipboard access require thread be of
                                                                                 *type Single Thread Apartment, because of
                                                                                 *clipboard isn't thread safe.*/
                this.enableClipSender = true;                                   /* Boolean used to stop this thread if the user press the button "Stop Server" */
                this.clipboardDataSender.IsBackground = true;                   /* This thread works in background! */
                this.clipboardDataSender.Start();                               /* The thread starts to work here! */

                this.clipboardDataReceiver = new Thread(clipReceiverFunction);  /* Creation of the clipboard data receiver thread. */
                this.clipboardDataReceiver.SetApartmentState(ApartmentState.STA); /*Clipboard access require thread be of
                                                                                 *type Single Thread Apartment, because of
                                                                                 *clipboard isn't thread safe.*/
                this.clipboardDataReceiver.IsBackground = true;                 /* Even this thread works in background! */
                this.enableClipServer = true;                                   /* Boolean used to stop this thread when we press "Stop Server" */
                this.clipboardDataReceiver.Start();                             /* The clipboard receiver starts to work here! */
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());                         /*Message box with exception message!*/
            }
        }

        /*
         * Server function. While main thread is waiting for user commands,
         * this thread is listening for client connection/commands and eventually will inject events in system queue.
         */
        private void listener_function()
        {
            message msg = new message();
            byte[] data;                    /*Byte array containing received data*/
            bool breaker;                   /*Boolean variable to break out of the inner loop*/
            
            serverSocket.Start();           /*Initialization of server Socket*/
            MessageBox.Show("Server Is Listening!");

            while (connected)
            {
                try /* This try/catch block is in the while loop in order to avoid the listener thread termination due to some exception */
                {
                    clientSocket = serverSocket.AcceptTcpClient();  /*The server is waiting for a client connection*/

                    data = SocketCommunication.receiveBufferFromSocket(clientSocket);   /* First thing, the client sends to the server the password */

                    string psw = Encoding.Unicode.GetString(data);                      /* Get the password string from the byte array received */

                    if (psw.CompareTo(password) == 0)                   /* If the password is correct */
                    {
                        msg.messType = messageType.CONNECTION_ACCEPTED;/* The server reply with a "ConnectionAccepted message" */
                    }
                    else /* If the password is wrong */
                    {
                        msg.messType = messageType.CONNECTION_REFUSED;/* The server reply with a "Connection Refused Message" */
                    }
 
                    SocketCommunication.sendBufferToSocket(SocketCommunication.rawSerialize(msg), clientSocket);

                    if (msg.messType == messageType.CONNECTION_REFUSED) /* If the password was wrong, we must not accept client messages. */
                        throw new Exception("Wrong Password");          /* Then we have to listen for a new connection, showing to the user the message "Wrong Password" */
                    
                    /*
                     * Here we are showing to the user info about the client actually connected 
                     */
                    Invoke(new editLabel(this.editLabel), new object[] { statusInfo, "CONNECTED WITH " + clientSocket.Client.RemoteEndPoint.ToString() });

                    this.target = true;

                    breaker = true; /* While the client is still active, we have to receive client messages. Breaker = false only if the client send to the server a "Disconnect" message */

                    while (breaker)
                    {
                        data = SocketCommunication.receiveBufferFromSocket(clientSocket);   /* Wait for a client message */
                        msg = SocketCommunication.rawDeserialize<message>(data);            /* Deserialization of the received byte array */

                        switch (msg.messType)   /* We have to check which message has been received */
                        {
                            case messageType.ACTION:    /* We received a message whith an input which has to be sent to the system queue */

                                inputSend(msg.act);     /* Function that inject the input in the system */

                                break;

                            case messageType.DISCONNECT:    /* We received a "Disconnect" message */
                                this.clientSocket.Close();  /* We have to close the TcpClient on which we are receiving the messages */
                                this.target = false;        /* Obviously this server isn't longer the target! */

                                Invoke(new editLabel(this.editLabel), new object[] { statusInfo, "DISCONNECTED" }); /* The server status is now "DISCONNECTED" */
                                Invoke(new editLabel(this.editLabel), new object[] { targetInfo, "FALSE" });        /* And this server is not longer the target */

                                breaker = false;    /* We have to break out from the inner loop in order to accept another client connection. */

                                break;

                            case messageType.TARGET_CHANGED:    /* This server is not longer the target, but the client is still connected */
                                this.target = false;

                                Invoke(new editLabel(this.editLabel), new object[] { targetInfo, "FALSE" });    /* This server is not longer the target */

                                break;

                            case messageType.TARGET:            /* This server is now the target! */
                                this.target = true;

                                Invoke(new editLabel(this.editLabel), new object[] { targetInfo, "TRUE" });

                                break;

                        }
                    }
                }
                catch (SocketException se)
                {
                    if (se.ErrorCode == 123456789) /* If the error code is 123456789, the other side of the connection has probably chashed. Then we have to close the actual connection and to wait for a new client connection */
                    {
                        this.clientSocket.Close(); /* We have to close the socket (due to the crashing of the other connection side) */
                        this.target = false;       /* And obviously this server is not longer the target. */

                        /*
                         * We have also to update the info to show to the user this server is not longer connected.
                         */
                        Invoke(new editLabel(this.editLabel), new object[] { statusInfo, "DISCONNECTED" });
                        Invoke(new editLabel(this.editLabel), new object[] { targetInfo, "FALSE" });

                        breaker = false;
                    }
                    continue;
                }
                catch (IOException)
                {
                    continue;
                }
                catch (ObjectDisposedException)
                {
                    continue;
                }
                catch (Exception except)
                {
                    MessageBox.Show(except.Message.ToString());
                }
            }
        }

        /*
         * Function that edit the Label l with the string text, and,
         * if the notify icon is visible, shows a BallonTip with the updated info.
         */
        public void editLabel(Label l, string text)
        {
            l.Text = text; /* Set the label l with the string text. */

            if (notifyIcon1.Visible == true)
            {
                if (text.Equals("TRUE") || text.Equals("FALSE")) /* If the text is true of false, we are updating the info about the target, do we have to add the string "Target = " */
                    text = "Target = " + text;
                
                notifyIcon1.BalloonTipText = text;  /* Set the BalloonTip text */
                notifyIcon1.ShowBalloonTip(5000);   /* And show it for 5 seconds */
            }
        } 

        /*
         * Function to handle the clipboard updates. This function override a function in the basic class.
         * This function receives a Message struct (pointer) containing info about the message. We have to process only
         * 2 messages, for the others we perform the classic operations in the basic class.
         */
        protected override void WndProc(ref Message m)
        {
            switch ((WM)m.Msg) /* Check the message type */
            {
                case WM.DRAWCLIPBOARD : /* The message is linked to a clipboard update, so we have to sent toward the client the new clipboard data.. */   
                    if (clientSocket != null && clientSocket.Connected && target && handle != GetClipboardOwner()) /* ..only if the client is connected and the clipboard was not set by this application! */
                    {
                        send.Set();     /* We are notifying to the clipboard sender (that is blocked by a send.WaitOne()) that a new clipboard data is present and it has to send it toward the client. */
                    }
                    
                    SendMessage(hWndNextWindow, m.Msg, m.WParam, m.LParam); /* Obviously the message has to be send to the next window in the clipboard window chain */
                    break;

                case WM.CHANGECBCHAIN:  /* This message indicates that the window in WParam must be removed from the chain. Then, if the next
                                         * window in the chain is the window that has to be removed, we have to set the next window
                                         * with the LParam, that is the handle of the window after the window to be deleted.
                                         */
                    if (m.WParam == hWndNextWindow)
                        hWndNextWindow = m.LParam;
                    else                /* The next window most not be removed. We have to sent the message to the next window. */
                        SendMessage(hWndNextWindow, m.Msg, m.WParam, m.LParam); 
                    break;

                default: /* Default Case: we have to call the basic procedure, in order to process all other messages */
                    base.WndProc(ref m);
                    break;
            }
        }

        /*
         * Event Handler for clicks on the Sto Server Button.
         */
        private void StopServerButton_Click(object sender, EventArgs e)
        {
            if (this.connected) /* If we are connected with a client */
            {
                /*
                 * Message box to confirm the choice.
                 */
                DialogResult confirmResult = MessageBox.Show("Sei davvero sicuro di voler interrompere la connessione?\nEventuali trasferimenti saranno interrotti.",
                                                            "Chiusura Connessione",
                                                            MessageBoxButtons.YesNo);

                if (confirmResult == DialogResult.Yes) /* If we really want to End the connection.. */
                {
                    endConnection();            /* Function that close the threads and modify the labels. */
                    ChangeClipboardChain(this.Handle, this.hWndNextWindow); /* We have to remove this window from the clipboard listeners chain. */
                    this.hWndNextWindow = IntPtr.Zero;                      /* And to nullify the next window handle */
                    connected = false;                                      /* We are not longer connected */
                }
            }
        }

        /*
         * Before the form closing we have to perform these operations
         */
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            ChangeClipboardChain(this.Handle, this.hWndNextWindow); /* This window has to be removed from the Clipboard Viewers Chain */
            Application.Exit();                                     /* After this we can end the application */
        }

        /*
         * Function executed by the clip receiver thread.
         */
        private void clipReceiverFunction()
        {
            message msg;
            clipServer = new TcpListener(IPAddress.Any, 9998); /* Socket listening for clients connections. */
            clipServer.Start();                                /* The clip server starts */
            byte[] data;

            while (enableClipServer) /* This boolean is modified by the main window when the user perform a click on the StopServerButton */                          
            {
                try
                {
                    using (clipReceiver = clipServer.AcceptTcpClient())
                    {
                        /* Here we are receiving the message with the clipboard data type that we are going to receive */
                        data = SocketCommunication.receiveBufferFromSocket(clipReceiver);
                        msg = SocketCommunication.rawDeserialize<message>(data);

                        /* If the message is not a remote paste message, there is an error, and we must throw an exception. */
                        if (msg.messType != messageType.REMOTE_PASTE)
                            throw new Exception("Wrong message format!");

                        switch (msg.cinf.ct)
                        {
                            case clipboardType.TEXT: /* We are going to receive text */

                                data = SocketCommunication.receiveChunks(clipReceiver, msg.cinf.size); /* Here we set the clipboard with the text received, encoding the received bytes with unicode format. */

                                Invoke(new txtSet(Clipboard.SetText), Encoding.Unicode.GetString(data)); /* Here we set the clipboard with the received text */

                                break;

                            case clipboardType.IMAGE: /* We are going to receive an Image */

                                data = SocketCommunication.receiveChunks(clipReceiver, msg.cinf.size); /*Function that receives data, saving chuncks of 1024 Bytes*/
                                
                                Image i = (Image)SocketCommunication.deserialize(data);                 /* Received bytes deserialization. The received object is of type Image*/
                                Invoke(new imgSet(Clipboard.SetImage), i);                              /* Here We set the clipboard with the received Image */

                                break;

                            case clipboardType.FILES: /* FILES case. To sent multiple files, we have to compress all files in a zip Archive. Therefore, now we must unzip the received archive */

                                String path = SocketCommunication.receiveFile(clipReceiver, msg.cinf.size); /* This function returns the path of the received archive */
                                String dirName = Path.Combine(path, "temp");                                /* Here we create the path to the folder that will contain all files */

                                if (!Directory.Exists(dirName)) /*If this folder doesn't exist...*/
                                {
                                    Directory.CreateDirectory(Path.Combine(dirName));   /* we have to create it! */
                                }
                                else
                                {
                                    Directory.Delete(dirName, true);                    /* The  directory already exists, and probably contains files...*/
                                    Directory.CreateDirectory(dirName);                 /* so we have to delete the directory and to recreate it, in order to save memory. */
                                }

                                ZipFile.ExtractToDirectory(Path.Combine(path, "receivedArchive.zip"), dirName); /* We unzip the archive in the folder "ReceiveTemp" */

                                string[] sc = Directory.GetFileSystemEntries(dirName);      /*This functions returns a string array with the filenames of each element in the new folder*/
                                StringCollection strColl = new StringCollection();          /* Clipboard.SetFileDropList requires a StringCollection object */

                                strColl.AddRange(sc);                                       /* Here we add to strColl all strings contained in sc */

                                Invoke(new fileSet(Clipboard.SetFileDropList), strColl);    /* We have to call this functions with invoke in order to set the main thread as clipboard owner */

                                break;

                            case clipboardType.AUDIO:

                                data = SocketCommunication.receiveChunks(clipReceiver, msg.cinf.size);  /*Function that receives data, saving chuncks of 1024 Bytes*/
                                Stream audio = (Stream)SocketCommunication.deserialize(data);           /* Received bytes deserialization. The received object is of type Stream*/
                                Invoke(new audioSet(Clipboard.SetAudio), audio);                        /* Here We set the clipboard with the received Audio stream */

                                break;
                        }
                    }
                }
                catch (SocketException)
                {
                    continue;
                }
                catch (IOException)
                {
                    continue;
                }
                catch (ObjectDisposedException)
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
         * Function of clipSender thread. This thread will stay blocked waiting for a condition variable set, performed by the main thread
         * when the user perform a copy/cut operation.
         */
        private void clipSenderFunction()
        {
            message msg = new message();
            byte[] data = null;

                while (enableClipSender)    /*This function runs until the Application is active or the user makes a click on StopServerButton.*/
                {
                    try
                    {
                        send.WaitOne();     /* Wait for a notification from the main thread */

                        if (!enableClipSender)  /* If the main thread has set this boolean to false, this thread will terminate here. */
                            break;

                        IPEndPoint remoteIpEndPoint = clientSocket.Client.RemoteEndPoint as IPEndPoint; /* IP address of the client actually connected */

                        msg.messType = messageType.REMOTE_COPY; /* The operation to perform is a RemoteCopy! */

                        if (Clipboard.ContainsText())   /* The clipboard contains text! */
                        {
                            String text = Clipboard.GetText();      /* Here we get the text copied in the clipboard */

                            data = Encoding.Unicode.GetBytes(text); /* We have to get the byte array of the text, in order to send this through a socket. */

                            msg.cinf.ct = clipboardType.TEXT;       /* We are sending text! */

                        }
                        else if (Clipboard.ContainsImage())         /*The clipboard contains an Image!*/
                        {
                            Image img = Clipboard.GetImage();       /*We have to get the image object..*/

                            data = SocketCommunication.serialize(img);  /* and to serialize it! */

                            msg.cinf.ct = clipboardType.IMAGE;      /*The data is an Image*/
                        }
                        else if (Clipboard.ContainsAudio())         /* The clipboard contains an audio stream! */
                        {
                            Stream audio = Clipboard.GetAudioStream();  /*We have to get the stream object..*/

                            data = SocketCommunication.serialize(audio);    /* and to serialize it! */

                            msg.cinf.ct = clipboardType.AUDIO;          /* The clipboard data is an audio stream! */
                        }
                        else if (Clipboard.ContainsFileDropList())      /* The clipboard contains files! */
                        {
                            string archPath = SocketCommunication.createArchive();  /*This function creates an archive with all files in the clipboard*/

                            FileInfo fi = new FileInfo(archPath);       /* Object containing info about the archive */

                            if (fi.Length > 10 * 1024 * 1024)           /* If the zip size is more than 10 MB */
                            {
                                DialogResult confirmResult = MessageBox.Show("Sei davvero sicuro di voler trasferire più di 10 MB di dati?",
                                                                "Conferma Trasferimento",
                                                                MessageBoxButtons.YesNo); /* We have to show a confirm button, in order to avoid network overhead. */

                                if (confirmResult == DialogResult.No)   /* If the user doesn't want to transfer the file.. */
                                {
                                    continue;                           /* We have to continue */
                                }
                            }

                            msg.cinf.ct = clipboardType.FILES;          /* The clipboard data is a FileDropList! */

                            msg.cinf.size = (int)fi.Length;             /* Size of the archive that is going to be sent */

                            using (clipClient = new TcpClient())        /* Socket used to send clipboard data to server */
                            {
                                clipClient.Connect(remoteIpEndPoint.Address, 9999); /* We have to connect to the target server, port 9999 */

                                SocketCommunication.sendBufferToSocket(SocketCommunication.rawSerialize(msg), clipClient);

                                SocketCommunication.sendFileToSocket(clipClient, archPath);
                            }

                            File.Delete(archPath);

                            continue;
                        }
                        else
                        {
                            msg.cinf.ct = clipboardType.NO_VALID_DATA;

                            data = new byte[4];
                        }

                        if (data.Length > 10 * 1024 * 1024)         /* If the data size is more than 10 MB */
                        {
                            DialogResult confirmResult = MessageBox.Show(   "Sei davvero sicuro di voler trasferire più di 10 MB di dati?",
                                                                            "Conferma Trasferimento",
                                                                            MessageBoxButtons.YesNo); /* We have to show a confirm button, in order to avoid network overhead. */

                            if (confirmResult == DialogResult.No)   /* If the user doesn't want to transfer the file.. */
                            {
                                continue;                           /* We have to continue */
                            }
                        }

                        msg.cinf.size = data.Length;

                        using (clipClient = new TcpClient())
                        {
                            clipClient.Connect(remoteIpEndPoint.Address, 9999);

                            SocketCommunication.sendBufferToSocket(SocketCommunication.rawSerialize(msg), clipClient);  /*Struct Sending*/
                            SocketCommunication.sendChunks(clipClient, data);                                           /* Data sending */
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
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
        }

        private void endConnection()
        {
            this.connected = false;     /*Volatile variable to terminate infinite loop in the server thread*/
            this.serverSocket.Server.Close();   /* We have to stop the serverSocket, so although it is blocked in an accept call we can unblock the thread */

            if (clientSocket != null && clientSocket.Connected) /* If the client socket is actually connected, we have to close it */
                this.clientSocket.Client.Close();



            this.portTextBox.Enabled = true;                    /* We must reactivate the portTextBox in order to insert the new port */
            this.statusInfo.Text = "DISCONNECTED";              /* The status in now DISCONNECTED */

            this.enableClipSender = false;                      /* Volatile boolean variable used to terminate the clipSender thread */
            if(clipClient != null && clipClient.Connected)      /* If the clipSender is sending data, we must close the socket that is sending */
                clipClient.Client.Close();
            send.Set();                                         /* If the clipSender is blocked, we must unblock it! */
            send.Reset();                                       /* We have to be sure this autoreset event will be reset! */

            this.enableClipServer = false;                      /* Volatile bool variable used to terminate the clipReceiver thread */
            this.clipServer.Server.Close();                             /* We have to stop the serverSocket, so although it is blocked in an accept call we can unblock the thread */
            if (clipReceiver != null && clipReceiver.Connected) /* If the clipReceiver is actually connected, we have to close the socket where it is receiving */
                clipReceiver.Client.Close();

            this.listener.Join();                               /* We must wait the termination of all threads */
            this.clipboardDataReceiver.Join();
            this.clipboardDataSender.Join();

            this.StopServerButton.Enabled = false;              /* We have to disable the stop server button */
            this.MakeListeningTcpSocket.Enabled = true;         /* And to enable the other button. */
            

            this.listener = null;
            this.clipboardDataReceiver = null;
            this.clipboardDataSender = null;
        }

        /*
         * Function that send the inputs into the system queue.
         */
        private void inputSend(action act)
        { 
            INPUT[] inputs = new INPUT[5];  /*Vector of windows input structure*/

            switch (act.eventype) /* We must distinguish all input events */
            {
                case EVENTYPE.MOUSE_MOVEMENT:
                    inputs[0] = InputSimulation.mouseMovement(act.x, act.y); /* inputs[0] will contain a mouse movement event, with the x,y coordinates */

                    if (SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT))) == 0) /* SendInput will send this input in the system queue. */
                        throw new Exception("Input non valido!");

                    break;

                case EVENTYPE.LEFT_DOWN:
                    inputs[0] = InputSimulation.mouseLeftDown(); /* inputs[0] will contain a mouse left button down event. */

                    if (SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT))) == 0) /* SendInput will send this input in the system queue. */
                        throw new Exception("Input non valido!");

                    break;

                case EVENTYPE.LEFT_UP:
                    inputs[0] = InputSimulation.mouseLeftUp(); /* inputs[0] will contain a mouse left button up event. */

                    if (SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT))) == 0) /* SendInput will send this input in the system queue. */
                        throw new Exception("Input non valido!");

                    break;

                case EVENTYPE.MOUSE_WHEEL:
                    inputs[0] = InputSimulation.mouseWheel(act.wheelMovement);  /* inputs[0] will contain a mouse wheel movement event. */

                    if (SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT))) == 0) /* SendInput will send this input in the system queue. */
                        throw new Exception("Input non valido!");

                    break;


                case EVENTYPE.RIGHT_DOWN:
                    inputs[0] = InputSimulation.mouseRightDown();   /* inputs[0] will contain a mouse right button down event. */

                    if (SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT))) == 0) /* SendInput will send this input in the system queue. */
                        throw new Exception("Input non valido!");

                    break;

                case EVENTYPE.RIGHT_UP:
                    inputs[0] = InputSimulation.mouseRightUp(); /* inputs[0] will contain a mouse right button up event. */

                    if (SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT))) == 0) /* SendInput will send this input in the system queue. */
                        throw new Exception("Input non valido!");

                    break;

                case EVENTYPE.MIDDLE_DOWN:
                    inputs[0] = InputSimulation.middleDown();   /* inputs[0] will contain a mouse middle button down event. */

                    if (SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT))) == 0)   /* SendInput will send this input in the system queue. */
                        throw new Exception("Input non valido!");
                    
                    break;

                case EVENTYPE.MIDDLE_UP:
                    inputs[0] = InputSimulation.middleUp(); /* inputs[0] will contain a mouse middle button up event. */

                    if (SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT))) == 0) /* SendInput will send this input in the system queue. */
                        throw new Exception("Input non valido!");

                    break;

                case EVENTYPE.KEY_DOWN:
                    inputs[0] = InputSimulation.keyboardPress((short)act.keypress);     /* inputs[0] will contain a keyboard press event. */

                    if (SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT))) == 0)       /* SendInput will send this input in the system queue. */
                        throw new Exception("Input non valido!");

                    break;

                case EVENTYPE.KEY_UP:
                    inputs[0] = InputSimulation.keyboardRelease((short)act.keypress);   /* inputs[0] will contain a keyboard release event. */


                    if (SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT))) == 0)       /* SendInput will send this input in the system queue. */
                        throw new Exception("Input non valido!");

                    break;

                default:
                    MessageBox.Show("Evento ricevuto sconosciuto!");

                    break;
            }
                            
        }

        /*
         * Window resize event handler
         */
        private void ServerWindow_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized) /* If we are minimizing the window, we have to hide the window and to show the notify icon */
            {
                notifyIcon1.Visible = true; /* We have to make visible the notify icon */
                this.Hide();                /* And to hide this window */
            }
        }

        /*
         * NotifyIcon double click event handler.
         */
        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            this.Show();                                /* We have to show the window */
            this.WindowState = FormWindowState.Normal;  /* To change the windowState from minimized to normal */
            notifyIcon1.Visible = false;                /* And to hide the notifyIcon. */
        }

    }
}