using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fagprojekt
{
    public struct Datapoint
    {
        public int key;
        public byte[] data;
        public DateTime time;
    }
    public class CanHandler
    {

        private bool running; //For worker thread

        //private Dictionary<int, List<Datapoint>> dictArray; //Ændre dict til at bruge denne som et array, måske list i stedet.


        private Queue<Datapoint> input;
        private Dictionary<int, Datapoint> dict;

        public Dictionary<int, Datapoint> Dict
        {
            get { return dict; }
        }

        private int[] IDs = new int[0];
        public List<int> ShowID = new List<int>();
        private SocketClient socket;
        FileStream logStream;
        StreamWriter logWriter;


        bool log = false;

        public CanHandler(string ip, int port)
        {
            dict = new Dictionary<int, Datapoint>();
            input = new Queue<Datapoint>();
            running = true;


            socket = new SocketClient(ip, port);


        }

        ~CanHandler()
        {
            running = false;
        }
        public void vis(String s)
        {
            try
            {
               

                ShowID.Add(int.Parse(s, System.Globalization.NumberStyles.HexNumber));
               
            }
            catch (Exception e)
            {

            }
        }
        public void nyShowID()
        {
            ShowID.Clear();
        }
        public override string ToString()
        {

            StringBuilder text = new StringBuilder("");



            foreach (int id in IDs)
            {
               
                    Datapoint entry = dict[id];

                    text.AppendFormat("ID: {0:X3}, Data: ", entry.key);
                if (!ShowID.Contains(id)) //tilføjet af witting
                {
                    foreach (byte data in entry.data)
                    {
                        text.AppendFormat("{0:X2}, ", data);
                    }
                }
                text.AppendLine();
            }
            text.AppendFormat("Datapoints: {0}\n", IDs.Length);

            return text.ToString();
        }



        public Datapoint getDataID(int ID)
        {
            if (dict.ContainsKey(ID))
            {
                return dict[ID];
            }

            return new Datapoint();
        }

        public int getKeyValue(int ID)
        {
            int sum = 0;
            foreach (byte value in dict[ID].data)
            {
                sum += value;
            }

            return sum;
        }


        public Datapoint[] GetDatapoints()
        {


            Datapoint[] array = new Datapoint[IDs.Length];
            int i = 0;
            foreach(int key in IDs) {
                array[i] = dict[IDs[i]];
                i++;
            }



            return array;
        }


        public void start()
        {
            Thread newThread = new Thread(readData);

            newThread.Start();
            logEnable();
        }

        public void stop()
        {
            running = false;

            logWriter.Close();
            logStream.Close();
        }
        

        public void logEnable()
        {

            logStream = new FileStream("test.log",FileMode.Create);
            
            logWriter = new StreamWriter(logStream);

            logWriter.AutoFlush = true;

            
            log = true;
        }

        private void saveLine(string line)
        {
            try
            {
                logWriter.WriteLine(DateTime.Now.Minute.ToString() + ":" + DateTime.Now.Second.ToString() + ":" + DateTime.Now.Millisecond.ToString() + " Data: "+  line);
            }
            catch (Exception)
            {
                
            }


        }

        private void readData()
        {
            while (running)
            {
                string line;

                     line = socket.readLine();
                lock (line)
                {
                    if (line.Length < 4 || line.Length > 69 || !line.Contains('[') || !line.Contains(']') || !line.Contains("0x"))
                    {
                        int tempnotknow = 0;
                    }

                    if (log)
                    {
                        saveLine(line);
                    }

                    processLine(line);
                }

            }
        }

        public void processLine(string line)
        {
            if (line.Length < 4 || line.Length > 69 || !line.Contains('[') || !line.Contains(']') || !line.Contains("0x"))
            {
                int tempnotknow = 0;
            }
            else
            {
                int ID = 0;
            int firstocc = line.IndexOf(' ');

            string temp;

            if (firstocc == 0)
            {
                int stopping = 2;
            }
            temp = line.Substring(0, firstocc);
            try
            {
                ID = Int32.Parse(temp, System.Globalization.NumberStyles.HexNumber);
            }
            catch (Exception e)
            {
                Console.WriteLine(line);
                Console.WriteLine(e.ToString());
            }

                temp = line.Substring(line.IndexOf('[') + 1, line.IndexOf(']') - line.IndexOf('[') - 1);

                string[] array = temp.Split(',');


                byte[] data = new byte[array.Length];

                int i = 0;

                foreach (string text in array)
                {
                    try
                    {
                        string temp2 = text.Substring(text.LastIndexOf("0x") + 2, 2);

                        data[i] = Byte.Parse(temp2, System.Globalization.NumberStyles.HexNumber);
                        i++;
                    }
                    catch (Exception e)
                    {

                    }

                }

                addQueue(ID, data);
            }
        }

        private void addQueue(int key, byte[] data)
        {
            Datapoint temp = new Datapoint();

            temp.data = data;
            temp.key = key;
            temp.time = DateTime.UtcNow;

            lock (input)
            {
                input.Enqueue(temp);
            }

        }


        public bool updateDict()
        {
            bool test = false;
            while (input.Count > 0)
            {
                test = true;

                Datapoint data;
                lock (input)
                {
                   data = input.Dequeue();
                }
                

                if (dict.ContainsKey(data.key))
                {
                    if (IsNewValue(dict[data.key], data)){
                        ValueChangedEventArgs args = new ValueChangedEventArgs();
                        args.Data = data;
                        OnValueChanged(args);
                        dict[data.key] = data;
                    }

                }
                else
                {
                    dict.Add(data.key, data);

                    ValueChangedEventArgs args = new ValueChangedEventArgs();
                    args.Data = data;
                    OnValueAdded(args);

                    int[] temp = new int[IDs.Length + 1];

                    int j = 0;
                    foreach (int i in IDs)
                    {

                        temp[j] = i;
                        j++;
                    }

                    temp[temp.Length - 1] = data.key;

                    Array.Sort(temp);
                   
                    IDs = temp;

                }
            }
            return test;
        }


        private bool IsNewValue(Datapoint data1, Datapoint data2)
        {
            if (data1.data.Length != data2.data.Length) return true;

            int i = 0;

            foreach(byte point in data1.data)
            {
                if (point != data2.data[i]) return true;
                i++;
            }
            return false;
        }

        protected virtual void OnValueChanged(ValueChangedEventArgs e)
        {
            EventHandler<ValueChangedEventArgs> handler = ValueChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler<ValueChangedEventArgs> ValueChanged;

        protected virtual void OnValueAdded(ValueChangedEventArgs e)
        {
            EventHandler<ValueChangedEventArgs> handler = ValueAdded;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler<ValueChangedEventArgs> ValueAdded;
    }

    public class ValueChangedEventArgs : EventArgs
    {
        public Datapoint Data { get; set; }
    }
}
