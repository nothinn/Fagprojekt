using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Fagprojekt
{
    class SocketClient
    {

        private static TcpClient tcpClient;
        private static NetworkStream networkStream;


        byte[] buffer = new byte[1024];

        bool running;

        public SocketClient(string ip, int port)
        {
            Console.WriteLine("Starting");

            try
            {
                tcpClient = new TcpClient(ip, port);
                networkStream = tcpClient.GetStream();

                running =  true;
                
                byte[] temp = Encoding.Default.GetBytes("start");


                networkStream.Write(temp, 0, temp.Length);
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex);
                running =  false;
            }

            Console.WriteLine("Done");
        }


        private static Queue<string> stringQueue = new Queue<string>(10);

        
        public int lines()
        {
            return stringQueue.Count;
        }

        public string readLine()
        {
            lock (stringQueue)
            {
                bool linenotready = true;

                string lineLast = "";
                while (linenotready)
                {
                    while (stringQueue.Count < 1)
                    {
                        string line;
                        byte[] bytesToRead = new byte[tcpClient.ReceiveBufferSize];

                        int bytesRead = networkStream.Read(bytesToRead, 0, tcpClient.ReceiveBufferSize);

                        line = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);

                        string[] array = line.Split('\r');

                        foreach (string text in array)
                        {
                            if (!String.IsNullOrWhiteSpace(text))
                            {
                                if (text.StartsWith(" "))
                                {
                                    if (text.Length < 4 || text.Length > 69 || !text.Contains('[') || !text.Contains(']') || !text.Contains("0x"))
                                    {
                                        new Exception();
                                    }
                                    stringQueue.Enqueue(text.Substring(1, text.Length -1 ));
                                }else
                                {
                                    if (text.Length < 4 || text.Length > 69 || !text.Contains('[') || !text.Contains(']') || !text.Contains("0x"))
                                    {
                                        new Exception();
                                    }
                                    stringQueue.Enqueue(text);
                                }



                            }
                        }
                    }
                    lineLast = stringQueue.Dequeue();
                    if (lineLast != "" && lineLast != null)
                    {
                        linenotready = false;
                    }
                }

                if (lineLast.Length < 4 || lineLast.Length > 69 || !lineLast.Contains('[') || !lineLast.Contains(']') || !lineLast.Contains("0x"))
                {
                    new Exception();
                }

                return lineLast;
            }
        }

        public bool isRunning()
        {
            return running;
        }
    }
}
