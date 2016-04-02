using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
namespace OVRSender
{
    public partial class MainWindow : Form
    {
        Thread ListenThread;
        // Incoming data from the client.
        static SocketData Data;
        static Socket listener;
        static bool bContinueListening = true;

        public MainWindow()
        {
            Data = new SocketData();
            InitializeComponent();
            ListenThread = new Thread(StartListening);
            ListenThread.Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Data.bSpawn = true;
            Data.SpawnPositionIndex = StringToInt(textBox1.Text);

            Data.zrotForce = StringToInt(textBox2.Text);
            Data.MaxRot = StringToInt(textBox3.Text);
            Data.MinRot = StringToInt(textBox4.Text);
            Data.rotupForce = StringToInt(textBox5.Text);
            Data.speed = StringToFloat(textBox6.Text);
            Data.speedincrease = StringToFloat(textBox7.Text);
            Data.speeddecrease = StringToFloat(textBox8.Text);
            Data.Maxspeed = StringToInt(textBox9.Text);
            Data.Minspeed = StringToInt(textBox10.Text);
            Data.takeoffspeed = StringToInt(textBox11.Text);
            Data.lift = StringToInt(textBox12.Text);
            Data.minlift = StringToInt(textBox13.Text);
            Data.Elevation = StringToFloat(textBox14.Text);
        }

        private void MainWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            bContinueListening = false;
            listener.Close();
        }

        public static void StartListening()
        {
            // Data buffer for incoming data.
            byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket.
            // Dns.GetHostName returns the name of the 
            // host running the application.
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());

            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 12345);

            // Create a TCP/IP socket.
            listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and 
            // listen for incoming connections.
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);

                // Start listening for connections.
                while (bContinueListening)
                {
                    Console.WriteLine("Waiting for a connection...");
                    // Program is suspended while waiting for an incoming connection.
                    Socket handler = listener.Accept();
                    string DataString = null;

                    // An incoming connection needs to be processed.
                    while (true)
                    {
                        bytes = new byte[1024];
                        int bytesRec = handler.Receive(bytes);
                        DataString += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                        if (DataString.IndexOf("<EOF>") > -1)
                        {
                            break;
                        }
                    }

                    // Show the data on the console.
                    Console.WriteLine("Text received : {0}", DataString);

                    // Echo the data back to the client.

                    DataString = SerializeObject(Data);

                    // Clear the spawn command
                    Data.bSpawn = false;

                    byte[] msg = Encoding.ASCII.GetBytes(DataString);

                    handler.Send(msg);
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

        }


        #region helperFunctions
        public static string SerializeObject(SocketData toSerialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(toSerialize.GetType());

            using (MemoryStream ms = new MemoryStream())
            {
                xmlSerializer.Serialize(ms, toSerialize);
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        static int StringToInt(string IN)
        {
            int j;
            if (Int32.TryParse(IN, out j))
            {
                return j;
            }
            return 0;
        }

        static float StringToFloat(string IN)
        {
            float o;
            if (float.TryParse(IN, out o))
            {
                return o;
            }
            return 0;

        }
        #endregion
    }


    [Serializable]
    public class SocketData
    {
        public bool bSpawn;
        // Data To Send
        public int zrotForce = 4;
        public int MaxRot = 90;
        public int MinRot = -90;
        public int rotupForce = 1;
        public float speed = 50;
        public float speedincrease = 4;
        public float speeddecrease = 1;
        public int Maxspeed = 100;
        public int Minspeed = 0;
        public int takeoffspeed = 20;
        public int lift = 3;
        public int minlift = 0;
        public bool hit = false;
        public int SpawnPositionIndex = 0;
        public float Elevation = 170;

    }

}
