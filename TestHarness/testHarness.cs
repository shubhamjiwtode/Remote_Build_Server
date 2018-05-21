/////////////////////////////////////////////////////////////////////
// testHarness.cs acepts test logs and loads the loader               //
//                                                                 //
// Author: SHUBHAM JIWTODE                                         //
// Application: CSE681-Software Modeling and Analysis Demo         //
// Environment: C# console                                         //
// Source: Jim Fawcett                                             //
/////////////////////////////////////////////////////////////////////



/*
 * Package Operations:
 * ===================
 * Receiving the test request
 * Loading the loader and executing test
 * sending logs to repository
 * 
 * Interfaces
 * -----------
 *public static void sendtestlogs(string i)//sends test logs to repo
 *  public static void sendtestlogs_conn(string i) //establish connection with repo
 *  public static void ack_sendDll(string x, string y)//acknowledges the connection with child proc
 *  public static void load(string j)//initiates loader to load the .dll files
 *  static public bool loadXml(string reqPath)//loads the test request file sent by child proc
 *  static public void parse(string propertyName, string tstrpath)// Parses the test request to get .dll files name
 *  
 * Required Files:
 * ---------------
 * Client.cs
 * program.cs
 * Repo.cs
 * MainWindows.xaml.cs
 * MPCommService.cs
 * IMPCommService.cs
 * 
 * Maintenance History:
 * --------------------
 * ver 2.0      12/05/2017 
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagePassingComm;
using System.Threading;
using DllLoaderDemo;
using System.Xml.Linq;
using System.Diagnostics;

namespace TestHarness
{


    class testHarness
    {
        private static DllLoaderExec dllLoader = new DllLoaderExec();
        private static Receiver rc;
        private static XDocument doc;
        private static List<string> values;
        private static Sender sendRepo = new Sender("http://localhost", 6060);

        static void initListen()
        {
            rc = new Receiver();

            rc.start("http://localhost", 9090);
        }

        static void listen()
        {
            while (true)
            {
                CommMessage rcMsg;
                rcMsg = rc.getMessage();
                switch (rcMsg.command)
                {
                    case "sendDll_conn":
                        {
                            rcMsg.show();
                            string i = rcMsg.arguments[0];
                            string j = rcMsg.arguments[1];
                            ack_sendDll(i, j);
                            break;
                        }
                    case "sendDll":
                        {
                            rcMsg.show();
                            string i = rcMsg.arguments[0];
                            string j = rcMsg.arguments[1];
                            load(j);
                            break;
                        }
                    case "ack_testlog":
                        {
                            rcMsg.show();
                            string i = rcMsg.arguments[0];

                            sendtestlogs(i,"x");
                            break;
                        }
                    case "KillTH": {
                            Killth();
                            break;
                        }
                }
            }
        }

///////////////////////////////////////////////////////////////////////// send test logs to the repo

        public static void sendtestlogs(string i,string x)
        {
            Console.WriteLine("\n------------sending test logs to repo");
            
            CommMessage sendMsg = new CommMessage(CommMessage.MessageType.request);
            sendMsg.from = "9090";
            sendMsg.to = "http://localhost:" + "6060" + "/IpluggableComm";
            sendMsg.command = "sendtestlogs";
            List<string> ls = new List<string>();
            ls.Add(i);
            sendMsg.author = "SHUBHAM RAMESH JIWTODE";
            sendMsg.arguments = ls;
            sendRepo.postMessage(sendMsg);
            string name = i + ".txt";
            System.IO.File.Delete(System.IO.Path.Combine(ServiceEnvironment.fileStorage,i+"TR.xml"));
            sendRepo.postFile(name, "Testlogs");
        }
        
        /// ///////////////////////////////////////////////////////////////kills test harness
       
        static public void Killth()
        {
            Process.GetCurrentProcess().Kill();
        }

        ///////////////////////////////////////////////////////////////////////// establish connection with repo

        public static void sendtestlogs_conn(string i) {
            Console.WriteLine("\n------------Establishing Connection with repo");
            CommMessage sendMsg = new CommMessage(CommMessage.MessageType.request);
            sendMsg.from = "9090";
            sendMsg.to = "http://localhost:" + "6060"+ "/IpluggableComm";
            sendMsg.command = "sendtestlogs_conn";
            List<string> ls = new List<string>();
            ls.Add(i);

            sendMsg.author = "SHUBHAM RAMESH JIWTODE";
            sendMsg.arguments = ls;
            sendRepo.postMessage(sendMsg);
        }



        ///////////////////////////////////////////////////////////////////////// acknowledges child proc for connection

        public static void ack_sendDll(string x, string y)
        {
            string port = "808" + x;
            Sender send = new Sender("http://localhost", Int32.Parse(port));
            CommMessage sendMsg = new CommMessage(CommMessage.MessageType.request);
            sendMsg.from = "808" + x;
            sendMsg.to = "http://localhost:" + "808" + x + "/IpluggableComm";
            sendMsg.command = "ack_sendDll";
            List<string> ls = new List<string>();
            ls.Add(x);
            ls.Add(y);
            sendMsg.author = "SHUBHAM RAMESH JIWTODE";
            sendMsg.arguments = ls;
            send.postMessage(sendMsg);
        }

        public static void load(string j)
        {
            Thread.Sleep(2000);
            string testpath = "../../../ThFileStore" + "/" + j + "TR.xml";
            Console.WriteLine(testpath);
            try
            {
                parse("lib", testpath);
            }
            catch {
                Console.WriteLine("invalid test request or no lib");
                    }
            string fileStorage = "../../../ThFileStore";
            DllLoaderExec.UpdateDllLoader(fileStorage, j,values.ToArray());
            DllLoaderExec.InitiateTest();
            sendtestlogs_conn(j);
        }

        ///////////////////////////////////////////////////////////////////////// loads the test request to Xdocument

        static public bool loadXml(string reqPath)
        {

            try
            {
                doc = XDocument.Load(reqPath);
                return true;
            }
            catch (Exception ex)
            {
                Console.Write("\n--{0}--\n", ex.Message);
                return false;
            }
        }

        ///////////////////////////////////////////////////////////////////////// Parses the test request sent by child proc

        static public void parse(string propertyName, string tstrpath)
        {
            string reqPath = tstrpath;
            values = new List<string>();
            loadXml(tstrpath);
            IEnumerable<XElement> parseElems = doc.Descendants(propertyName);
            if (parseElems.Count() > 0)
            {
                switch (propertyName)
                {
                    case "lib":
                        foreach (XElement elem in parseElems)
                        {
                            values.Add(elem.Value);

                        }
                        break;
                }
            }
        } 

        static void Main(string[] args)
        {
            ClientEnvironment.fileStorage = "../../../ThFileStore";
            ServiceEnvironment.fileStorage = "../../../ThFileStore";
            initListen();
            Thread rcMsg = new Thread(new ThreadStart(listen));
            rcMsg.Start();
        }
    }
}
