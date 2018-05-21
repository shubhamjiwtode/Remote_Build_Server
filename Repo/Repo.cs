/////////////////////////////////////////////////////////////////////
// Repo.cs - Handles the testreq and related files                 //
//                                                                 //
// Author: SHUBHAM JIWTODE                                         //
// Application: CSE681-Software Modeling and Analysis Demo         //
// Environment: C# console                                         //
// Source: Jim Fawcett                                             //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * ===================
 * Save the TestRequest and respective files from Client
 * Sends buildreq and realted Files to Childbuild when requested
 * 
 * Interfaces:
 * -----------
 * static public void parse() // Parses the xml for both testfiles and testdriver
 * static public void SendR2CP(string i, string j, MessagePassingComm.Sender[] x)// Sends buildrequest and respective .cs files
 * static public void SendR2CP_conn(string i,string j,MessagePassingComm.Sender[] x)//Makes connection with childprocess
 * static public List<string> parseList(string propertyName)//Parses the property from the Loaded Xml files
 * static public bool loadXml(string reqPath)// Parsed xml is loaded to doc 
 * static public List<string> parseList(string propertyName)  // Parses the provided xml file
 * static public void KillRepo() //Kills the repo(Itself)
 *  public static List<string> callParse(string path_of_xml)//Parses the xml to get file names
 *  static public void ack_testlog(string i)//acknowledges test harness about the connection
 *  static public void ack_buildlog(string i)// acknowledges child proc about the connection
 * public static void getbuildlogsHandler()//sends list of build logs to client
 * public static void gettestlogsHandler()//sends list of test logs to client
 * public static void getDriverfilesHandler()//sends list of driver files to client
 * public static void getTestfilesHandler()//sends list of test files to client
 * 
 * Required Files:
 * ---------------
 * Client.cs
 * Repo.cs
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
using System.Xml.Linq;
using System.Diagnostics;

namespace Repo
{
    class Repo
    {
        private static Receiver rc;
        private static XDocument doc;
        private static List<string> testedFiles { get; set; } = new List<string>();
        private static List<string> testDriver { get; set; } = new List<string>();
        private static  List<string> sendList { get; set; } = new List<string>();
        /////////////////////////////////////////////////////////////

        static void initListen()
        {
            rc = new Receiver();

            rc.start("http://localhost", 6060);
        }

        static void listen(string x)
        {
            Sender[] send = new Sender[Int32.Parse(x)];
            for (int i = 0; i < Int32.Parse(x); i++)
            {
                send[i] = new Sender("http://localhost", 8081 + i);
            }
            while (true)
            {
                CommMessage rcMsg;
                rcMsg = rc.getMessage();
                rcMsg.show();
                switch (rcMsg.command)
                {
                    case "requestRfile":
                        {
                            SendR2CP_conn(rcMsg.arguments[0], rcMsg.arguments[1], send);
                            break;
                        }
                    case "ack_R":
                        {
                            SendR2CP(rcMsg.arguments[0], rcMsg.arguments[1], send);
                            break;
                        }
                    case "sendlogs_conn":
                        {
                            ack_buildlog(rcMsg.arguments[0], rcMsg.arguments[1]);
                            break;
                        }
                    case "sendtestlogs_conn":
                        {
                            ack_testlog(rcMsg.arguments[0]);
                            break;
                        }
                    case "KillRepo":
                        {
                            KillRepo();
                            break;
                        }
                    case "getfiles":
                        {
                            sendFiletoClient(rcMsg.arguments[0]);
                            break;
                        }
                }
            }
        }

        static public void KillRepo() {
            Process.GetCurrentProcess().Kill();
        }

        static public void sendFiletoClient(string args)
        {
            if (args == "gettestDriver")
            {
                getDriverFilesHandler();
            }
            if (args == "gettestFiles")
            {
                gettestfileHandler();
            }
            if (args == "getbuildLogs")
            {
                gettestlogsHandler();
            }
            if (args == "gettestFiles")
            {
                getbuildlogsHandler();
            }
        }

        ///////////////////////////////////////////////////////////////////////////////// Sending buildlog list to client
        public static void getbuildlogsHandler()
        {
            Console.WriteLine("\n------------sending build log list to client");
            Sender send = new Sender("http://localhost", 7070);
            CommMessage sndMsg = new CommMessage(CommMessage.MessageType.request);
            sndMsg.to = "http://localhost:7070/IPluggableComm";
            sndMsg.from = "http://localhost:6060/IPluggableComm";
            sndMsg.command = "updateBuildLogList";
            sndMsg.author = "SHUBHAM JIWTODE";
            string driverFilesPath = ClientEnvironment.fileStorage + "/" + "Buildlogs/";
            List<string> ls = new List<string>();
            string[] testDriver = System.IO.Directory.GetFiles(driverFilesPath);
            foreach (string file in testDriver)
            {
                ls.Add(System.IO.Path.GetFileName(file));
            }
            sndMsg.arguments = ls;
            send.postMessage(sndMsg);
        }


        ///////////////////////////////////////////////////////////////////////////////// Sending testlogs list to client

        public static void gettestlogsHandler()
        {
            Console.WriteLine("\n------------sending test logs list to client");
            Sender send = new Sender("http://localhost", 7070);
            CommMessage sndMsg = new CommMessage(CommMessage.MessageType.request);
            sndMsg.to = "http://localhost:7070/IPluggableComm";
            sndMsg.from = "http://localhost:6060/IPluggableComm";
            sndMsg.command = "updateTestLogList";
            sndMsg.author = "SHUBHAM JIWTODE";
            string driverFilesPath = ClientEnvironment.fileStorage + "/" + "Testlogs/";
            List<string> ls = new List<string>();
            string[] testDriver = System.IO.Directory.GetFiles(driverFilesPath);
            foreach (string file in testDriver)
            {
                ls.Add(System.IO.Path.GetFileName(file));
            }
            sndMsg.arguments = ls;
            send.postMessage(sndMsg);
        }





        ///////////////////////////////////////////////////////////////////////////////// Sending testDriver list to client
        public static void getDriverFilesHandler()
        {
            Sender send = new Sender("http://localhost", 7070);
            CommMessage sndMsg = new CommMessage(CommMessage.MessageType.request);
            sndMsg.to = "http://localhost:7070/IPluggableComm";
            sndMsg.from = "http://localhost:6060/IPluggableComm";
            sndMsg.command = "updateTestDriverList";
            sndMsg.author = "SHUBHAM JIWTODE";
            string driverFilesPath = ClientEnvironment.fileStorage + "/" + "TestDrivers/";
            List<string> ls = new List<string>();
            string[] testDriver = System.IO.Directory.GetFiles(driverFilesPath);
            foreach (string file in testDriver)
            {
                ls.Add(System.IO.Path.GetFileName(file));
            }
            sndMsg.arguments = ls;
            send.postMessage(sndMsg);
        }

        ///////////////////////////////////////////////////////////////////////////////// Sending testfiles list to client
        public static void gettestfileHandler()
        {
            Sender send = new Sender("http://localhost", 7070);
            CommMessage sndMsg = new CommMessage(CommMessage.MessageType.request);
            sndMsg.to = "http://localhost:7070/IPluggableComm";
            sndMsg.from = "http://localhost:6060/IPluggableComm";
            sndMsg.command = "updateTestFileList";
            sndMsg.author = "SHUBHAM JIWTODE";
            string driverFilesPath = ClientEnvironment.fileStorage + "/" + "TestFiles/";
            List<string> ls = new List<string>();
            string[] testDriver = System.IO.Directory.GetFiles(driverFilesPath);
            foreach (string file in testDriver)
            {
                ls.Add(System.IO.Path.GetFileName(file));
            }
            sndMsg.arguments = ls;
            send.postMessage(sndMsg);
        }

        ///////////////////////////////////////////////////////////////////////////////// Parsed XML is loaded to doc

        static public bool loadXml(string reqPath)
        {
            
            try
            {
                doc = XDocument.Load(reqPath);
                Console.WriteLine("\n" + reqPath);
                return true;
            }
            catch (Exception ex)
            {
                Console.Write("\n--{0}--\n", ex.Message);
                return false;
            }
        }

        /////////////////////////////////////////////////////////////////////////////// parses the xml to get the file names

        public static List<string> callParse(string path_of_xml)
        {
            Console.WriteLine("in callparse method");
            loadXml(path_of_xml);
            var items = doc.Descendants("test");
            List<string> finallist = new List<string>();
            foreach (var item in items)
            {
                if (item.Descendants("testDriver").First().Value != null)
                {
                    try
                    {
                        var childs = item.Descendants("testDriver").First().Value; //skip <name> element
                        sendList.Add(childs);
                       
                        IEnumerable<XElement> parseElems = item.Descendants("tested");
                        if (parseElems.Count() > 0)
                        {
                            foreach (XElement elem in parseElems)
                            {
                               
                                sendList.Add(elem.Value);
                            }
                        } 
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Invalid Test Request", e.Message);
                    }
                }
            }

            return sendList;
        }

        /////////////////////////////////////////////////////////////////////////////// acknoledge test harness about the connection
        static public void ack_testlog(string i)
        {
            
            Sender send = new Sender("http://localhost ",9090);
            CommMessage sendMsg = new CommMessage(CommMessage.MessageType.request);
            sendMsg.from = "6060";
            sendMsg.to = "http://localhost:" + "9090" + "/IpluggableComm";
            sendMsg.command = "ack_testlog";
            List<string> ls = new List<string>();
            ls.Add(i);
           
            sendMsg.author = "SHUBHAM RAMESH JIWTODE";
            sendMsg.arguments = ls;
            send.postMessage(sendMsg);


        }

        /////////////////////////////////////////////////////////////////////////////// acknoledge child proc about the connection
        static public void ack_buildlog(string i, string j)
        {
            int x = 8080 + Int32.Parse(i);
            Sender send = new Sender("http://localhost", x);
            CommMessage sendMsg = new CommMessage(CommMessage.MessageType.request);
            sendMsg.from = "6060";
            sendMsg.to = "http://localhost:"+x+"/IpluggableComm";
            sendMsg.command = "ack_buildlog";
            List<string> ls = new List<string>();
            ls.Add(i);
            ls.Add(j);
            sendMsg.author = "SHUBHAM RAMESH JIWTODE";
            sendMsg.arguments = ls;
            send.postMessage(sendMsg);


        }




        /////////////////////////////////////////////////////////////// Makes connection to ChildProcess
        static public void SendR2CP_conn(string i,string j,MessagePassingComm.Sender[] x)
        {
            Console.WriteLine("\n------------Establishing Connection with Child proc");
            int add = Int32.Parse(j) - 8081;
            CommMessage sendMsg = new CommMessage(CommMessage.MessageType.request);
            sendMsg.from = "http://localhost:6060/IpluggableComm";
            sendMsg.to = "http://localhost:" + j + "/IpluggableComm";
            List<string> ls = new List<string>();
            ls.Add(i);
            ls.Add(j);
            sendMsg.arguments = ls;
            sendMsg.author = "SHUBHAM RAMESH JIWTODE";
            sendMsg.command = "SendR2CP_conn";
            x[add].postMessage(sendMsg);
           
        }

        /////////////////////////////////////////////////////////////// Sends buildrequest and respective .cs files
        static public void SendR2CP(string i, string j, MessagePassingComm.Sender[] x)
        {
            Console.WriteLine("\n------------sending files to child proc");
            sendList = new List<string>();
            int add = Int32.Parse(j) - 8081;
            int pid = add + 1;
            CommMessage sendMsg = new CommMessage(CommMessage.MessageType.reply);
            sendMsg.from = "http://localhost:6060/IpluggableComm";
            sendMsg.to = "http://localhost:" + j + "/IpluggableComm";
            List<string> ls = new List<string>();
            ls.Add(i);
            ls.Add(j);
            sendMsg.arguments = ls; 
            sendMsg.command = "SendR2CP";
            sendMsg.author = "SHUBHAM RAMESH JIWTODE";
            x[add].postMessage(sendMsg);
            sendList.Add(i);
            
           
            callParse(System.IO.Path.Combine("../../../RepoFileStore", i));
            
            foreach (string file in sendList) {
               
                Console.WriteLine(file);
                
                x[add].postFile(file, pid.ToString());
            }
            
        }
        

        public Repo() {
           
        }
        static void Main(string[] args)
        {
            ServiceEnvironment.fileStorage = "../../../RepoFileStore";
            ClientEnvironment.fileStorage = "../../../RepoFileStore";
            initListen();
            if (Int32.Parse(args[0]) != 0)
            {
                listen(args[0]);
            }
           
            
            Console.Write("\n  Press key to exit");
            
        }
    }
}
