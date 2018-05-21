
/////////////////////////////////////////////////////////////////////
// ChildProc - demonstrate creation of multiple .net processes     //
//                                                                 //
// Author: SHUBHAM JIWTODE                                         //
// Application: CSE681-Software Modeling and Analysis Demo         //
// Environment: C# console                                         //
// Source: Jim Fawcett                                             //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * ===================
 * Takes care of build request sent by mother process
 * gets testreq and related files from repository
 * 
 * 
 * Interfaces:
 * -----------
 *  static public void send(string id) // notifies Mother process about its availability
 *  static public void ack_R(string id, string reqName) //Acknowledges the repo about the connection
 *  static public void requestRfile(string id, string reqName)//sends Request to repo to send the xml and related .cs files
 * public static List<string[]> callParse(string path_of_xml)//parses mutli test build request
 * static public void sendDll_conn(string x, string y)//establish connection with test harness
 * static public void sendDll(string x, string y)//sends .dll to test harness
 * static public void sendlogs_conn(string x,string y)//establish connection with repo
 * static public void sendlogs_conn(string x,string y)//sends build logs to repo
 * static public void getTestReqInfo(string i,string j)//Gather file info to build test request
 * 
 * 
 * Required Files:
 * ---------------
 * Repo.cs
 * program.cs
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
using System.Diagnostics;
using MessagePassingComm;
using System.Threading;
using System.IO;
using System.Xml.Linq;
using Clienthelp;

namespace ChildProc
{
    class ChildProc
    {
      
        private static Receiver rc;
        private static XDocument docTR;
        private static XDocument doc=new XDocument();
        private static StreamWriter log;
        public static string author { get; set; } = "SHUBHAM JIWTODE";
        private static Sender sendRepo=new Sender("http://localhost", 6060);
        private static Sender sendTh = new Sender("http://localhost", 9090);
        public static object BuildProcess { get; private set; }
        private static List<string> DllFiles = new List<string>();
        private static String curdir;
        private static int ecode = -1;
        private static string outName;
        /////////////////////////////////////////////////////////////

        static void initListen(string i)
        {
            rc = new Receiver();
            string x = "808" + i;
            rc.start("http://localhost", Int32.Parse(x));
        }




        static void listen()
        {
            while (true)
            {
                CommMessage rcMsg;
                rcMsg = rc.getMessage();
                rcMsg.show();
                switch (rcMsg.command)
                {
                    case "sendtoChild":
                        {  
                            requestRfile(rcMsg.arguments[1], rcMsg.arguments[0]);
                            break;
                        }
                    case "SendR2CP_conn":
                        {
                            ack_R(rcMsg.arguments[1], rcMsg.arguments[0]);
                            break;
                        }
                    case "SendR2CP":
                        {   string i = rcMsg.arguments[1];
                            string j = rcMsg.arguments[0];
                            int k = Int32.Parse(i)-8080;
                            Thread.Sleep(2000);
                            SelectToolChain(k, j);
                            string filepath = "../../../BuilderFileStore/" + k;
                            string name = System.IO.Path.GetFileNameWithoutExtension(System.IO.Path.Combine(filepath, j));
                            sendlogs_conn(k.ToString(), name);
                            break;
                        }
                    case "ack_buildlog":
                        { 
                            sendlogs(rcMsg.arguments[0], rcMsg.arguments[1]);
                            sendDll_conn(rcMsg.arguments[0], rcMsg.arguments[1]);
                            break;
                        }
                    case "ack_sendDll":
                        { 
                            sendDll(rcMsg.arguments[0], rcMsg.arguments[1]);
                            break;
                        }
                }
            }
        }


       ///////////////////////////////////////////////////////////////// load the build request send by the repo

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

        //////////////////////////////////////////////////////////Parses the build req sent by the repo
        static public string parse(string propertyName, int no, string reqName)
        {
            string reqPath = "../../../BuilderFileStore/" + no + "/" + reqName;
            
            loadXml(reqPath);
           
            string parseStr = doc.Descendants(propertyName).First().Value;
            return parseStr;
        }



        ////////////////////////////////////////////////////////// Selecting tool chain depending on the parsed file from the xml request

        static public void SelectToolChain(int no, string reqName)
        {
            Console.WriteLine("...........................");
            string testDrive = parse("testDriver", no,  reqName);
            Console.WriteLine(testDrive);
            string testDrivepath = System.IO.Path.Combine("../../../BuilderFileStore/"+no, testDrive);
            string ext = System.IO.Path.GetExtension(testDrivepath);
            Console.WriteLine("\n-------------Tool chain selected is-- " + ext + "\n");
            if (ext == ".cs")
            {
                Csbuild(no, reqName);
            }
            else 
            {
                Console.WriteLine("functionality not available");
            }
            
        }

        ////////////////////////////////////////////////////////// Starts build process for .cs files

        static public void procCSB(Process process, string i,string[] test,string outName) {
            process.StartInfo.CreateNoWindow = false;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.FileName = "cmd.exe";
            curdir = Directory.GetCurrentDirectory();
            System.Text.StringBuilder testfiles=new StringBuilder();
            foreach (string x in test) {
                string y = x + " ";
                testfiles.Append(y); }
            Directory.SetCurrentDirectory(i);
            
            string args = "/Ccsc /target:library /OUT:"+outName+".dll /nologo "+ testfiles;
            Console.WriteLine(args);
            process.StartInfo.Arguments = args;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            
            process.Start();
            
            ecode = -1;
            process.WaitForExit();
            ecode = process.ExitCode;
           
        }

        //////////////////////////////////////////////////////////Tool chain for .cs-Starts buid process and writes log after completion of build process
        static public void Csbuild(int no, string reqName)
        {
            
            try
            {
                string solutionFile = "../../../BuilderFileStore/"+no;
                string exePath = System.IO.Path.Combine("C:\\Windows\\Microsoft.NET\\Framework\\v4.0.30319", "csc.exe");
               
                Process process = new Process();
                List<string[]> list = new List<string[]>();
                list =callParse(System.IO.Path.Combine(solutionFile, reqName));
                int cnt = 1;
               
                foreach (String[] test in list)
                {
                    
                    outName = cnt++.ToString();
                    try
                    {
                        procCSB(process, solutionFile, test, outName);
                    }
                    catch {
                        Console.WriteLine("");
                    }
                    Thread.Sleep(2000);
                    List<string> logOutput = new List<string>();
                    string Childpath = "../../BuilderFileStore/" + no;
                    string[] buildFiles = System.IO.Directory.GetFiles(Childpath, "*.cs*");  
                    string name = System.IO.Path.GetFileNameWithoutExtension(System.IO.Path.Combine(Childpath, reqName));
                    
                    Cslogs(buildFiles, Childpath, name);
                    Directory.SetCurrentDirectory(curdir);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
        }

        ////////////////////////////////////////////////////////// Writes log for build of .cs

        static public void Cslogs(string[] Buildfile,string path,string logname) {
            using (log = new System.IO.StreamWriter(System.IO.Path.Combine(path + "/" + logname + ".txt"), true))
            {
                log.Write("Build Started at: " + DateTime.Now.ToString() + Environment.NewLine);
                foreach (System.Collections.DictionaryEntry env in Environment.GetEnvironmentVariables())
                {
                    log.WriteLine((string)env.Key + "=" + (string)env.Value);
                }
                log.WriteLine(Environment.NewLine + "Compiling:" + Environment.NewLine);
                foreach (string file in Buildfile)
                {
                    log.WriteLine(System.IO.Path.GetFileName(file) + Environment.NewLine);
                }

                log.Write("Build Started at: " + DateTime.Now.ToString());
                if (ecode == 0)
                {
                    Console.Write("\nBuild Succeeded.");
                    log.WriteLine("\nBuild Succeeded.");

                }
                else
                {
                    Console.Write("\nBuild Failed.");
                    log.WriteLine("\n\nBuild Failed.");
                    Console.Write("\nCheck log file generatead at {0} for more details", System.IO.Path.GetFullPath((System.IO.Path.Combine(path + "/" + logname + ".txt"))));

                }


            }
        }
        
       
        

        //////////////////////////////////////////////////////////Gather information for creating Test request

        static public void getTestReqInfo(string i,string j)
        {
            string builderPath = "../../../BuilderFileStore/" + i;
            Console.WriteLine("\n------------Test Request Created"+builderPath);
            
            Console.WriteLine(System.IO.Path.GetFullPath(builderPath) );
            string[] test_Files = System.IO.Directory.GetFiles(builderPath);
            foreach (string file in test_Files)
            {
                String path = System.IO.Path.Combine(builderPath, file);
                if (System.IO.Path.GetExtension(path) == ".dll")
                {
                    DllFiles.Add(System.IO.Path.GetFileName(file));
                }

            }
            makeRequest(builderPath,j);
        }

        ////////////////////////////////////////////////////////// Creates test request to be sent to test harness
        static public void makeRequest(string i,string j)
        {
            docTR = new XDocument();
            XElement testRequestElem = new XElement("testRequest");
            docTR.Add(testRequestElem);

            XElement authorElem = new XElement("author");
            authorElem.Add(author);
            testRequestElem.Add(authorElem);

            XElement dateTimeElem = new XElement("dateTime");
            dateTimeElem.Add(DateTime.Now.ToString());
            testRequestElem.Add(dateTimeElem);

            
            foreach (string file in DllFiles)
            {
                XElement driverElem = new XElement("lib");
                driverElem.Add(file);
                testRequestElem.Add(driverElem);
                Console.WriteLine(file);
            }

          
            saveXml(i,j);

        }
        ////////////////////////////////////////////////////////// Save the created test request in BuilderFileStore
        static public bool saveXml(string path,string name)
        {
            try
            {
                name = name + "TR.xml";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                docTR.Save(System.IO.Path.Combine(path, name));

                return true;
            }
            catch (Exception ex)
            {
                Console.Write("\n--{0}--\n", ex.Message);
                return false;
            }

        }

        ////////////////////////////////////////////////////////// To establish a channel to tranfer .dll files

        static public void sendDll_conn(string x, string y)
        {

            Console.WriteLine("\n------------Establishing Connection with Test Harness");
            getTestReqInfo(x, y);
            CommMessage sendMsg = new CommMessage(CommMessage.MessageType.request);
            sendMsg.from = "808" + x;
            sendMsg.to = "http://localhost:9090/IpluggableComm";
            sendMsg.command = "sendDll_conn";
            List<string> ls = new List<string>();
            ls.Add(x);
            ls.Add(y);
            
            sendMsg.author = "SHUBHAM RAMESH JIWTODE";
            sendMsg.arguments = ls;
            sendTh.postMessage(sendMsg);

        }

        ////////////////////////////////////////////////////////// To send .dll files over an established channel

        public static void sendDll(string x, string y)
        {

            Console.WriteLine("\n------------sending dll to Test Harness");
            CommMessage sendMsg = new CommMessage(CommMessage.MessageType.request);
            sendMsg.from = "808" + x;
            sendMsg.to = "http://localhost:9090/IpluggableComm";
            sendMsg.command = "sendDll";
            List<string> ls = new List<string>();
            ls.Add(x);
            ls.Add(y);
            sendMsg.author = "SHUBHAM RAMESH JIWTODE";
            sendMsg.arguments = ls;
            sendTh.postMessage(sendMsg);
            string name = y + "TR.xml";
            sendTh.postFile(name,"");
            foreach (string file in DllFiles) {
                sendTh.postFile(file,"");
            }

            sendReady(x);
        }


        /////////////////////////////////////////////////////////////////To establish channel between chilc proc an repo to send logs

        static public void sendlogs_conn(string x,string y)
        {
            Console.WriteLine("\n------------Establishing Connection with Repo");

            CommMessage sendMsg = new CommMessage(CommMessage.MessageType.request);
            sendMsg.from = "808"+x;
            sendMsg.to = "http://localhost:6060/IpluggableComm";
            sendMsg.command = "sendlogs_conn";
            List<string> ls = new List<string>();
            ls.Add(x);
            ls.Add(y);
            sendMsg.author = "SHUBHAM RAMESH JIWTODE";
            sendMsg.arguments = ls;
            sendRepo.postMessage(sendMsg);
        }

        ////////////////////////////////////////////////////////// Sending buil log to repo
        public static void sendlogs(string x,string y)
        {
            Console.WriteLine("\n------------sending logs to repo");
            CommMessage sendMsg = new CommMessage(CommMessage.MessageType.request);
            sendMsg.from = "808" + x;
            sendMsg.to = "http://localhost:6060/IpluggableComm";
            sendMsg.command = "sendlogs";
            List<string> ls = new List<string>();
            string name = y + ".txt";
            ls.Add(x);
            ls.Add(name);
            sendMsg.author = "SHUBHAM RAMESH JIWTODE";
            sendMsg.arguments = ls;
            sendRepo.postMessage(sendMsg);
            
            sendRepo.postFile(name,"Buildlogs");
           
        }

        /////////////////////////////////////////////////////////////// Acknowledges the repo about the connection
        static public void ack_R(string id, string reqName) {
            Console.WriteLine("\n------------Established Connection with Repo");
            Sender send = new Sender("http://localhost", 6060);
            CommMessage sendMsg = new CommMessage(CommMessage.MessageType.request);
            sendMsg.from = id;
            sendMsg.to = "http://localhost:6060/IpluggableComm";
            sendMsg.command = "ack_R";
            List<string> ls = new List<string>();
            ls.Add(reqName);
            ls.Add(id);
            sendMsg.author = "SHUBHAM RAMESH JIWTODE";
            sendMsg.arguments = ls;
            send.postMessage(sendMsg);
            

        }


        ////////////////////////////////////////////////////////////sends Request to repo to send the xml and related .cs files

        static public void requestRfile(string id, string reqName)
        {
            Console.WriteLine("\n------------Requesting build files to Repo");
            Sender send = new Sender("http://localhost", 6060);
            CommMessage sendMsg = new CommMessage(CommMessage.MessageType.request);
            sendMsg.from = id; 
            sendMsg.to = "http://localhost:6060/IpluggableComm";
            sendMsg.command = "requestRfile";
            List<string> ls = new List<string>();
            ls.Add(reqName);
            ls.Add(id);
            sendMsg.arguments = ls;
            sendMsg.author = "SHUBHAM RAMESH JIWTODE";
            
            if (Directory.Exists(ClientEnvironment.fileStorage))
            {
                System.IO.DirectoryInfo dir = new DirectoryInfo(ClientEnvironment.fileStorage);
                foreach (FileInfo file in dir.GetFiles()) { file.Delete(); }
            }
            send.postMessage(sendMsg);
        }
        //////////////////////////////////////////////////////////// notifies Mother process about its availability
        static public void sendReady(string id)
        {
            Console.WriteLine("\n------------requeuing child{0}",id);
            string x = "808" + id;
            Sender send = new Sender("http://localhost",8080);
            CommMessage sendMsg = new CommMessage(CommMessage.MessageType.request);
            
            sendMsg.from = x;
            sendMsg.to = "http://localhost:8080/IpluggableComm";
            sendMsg.command = "m_ready";
            sendMsg.author = "SHUBHAM RAMESH JIWTODE";
            send.postMessage(sendMsg);
            
            
        }


        ////////////////////////////////////////////////////////// parse multi test build request
        public static List<string[]> callParse(string path_of_xml)
        {
            loadXml(path_of_xml);
            var items = doc.Descendants("test");
            List<string[]> finallist = new List<string[]>();
            List<string> arr = new List<string>();
            string[] one;
            foreach (var item in items)
            {
                if (item.Descendants("testDriver").First().Value != null)
                {
                    try
                    {
                        var childs = item.Descendants("testDriver").First().Value; //skip <name> element
                        arr.Add(childs);
                        IEnumerable<XElement> parseElems = item.Descendants("tested");
                        if (parseElems.Count() > 0)
                        {
                            foreach (XElement elem in parseElems)
                            {
                                arr.Add(elem.Value);
                            }
                        }
                        one = arr.ToArray();
                        foreach (var x in one)
                        {
                            Console.WriteLine("one elemnts");
                            Console.WriteLine(x);
                        }
                        finallist.Add(one);
                        arr.Clear();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Invalid Test Request", e.Message);
                    }
                }
            }
            return finallist;
        }
        ////////////////////////////////////////////////////////////
        static void Main(string[] args)
        {
           
            initListen(args[0]);
           
            Console.Write("\n  Hello from child #{0}\n\n", args[0]);
            sendReady(args[0]);
            Thread rcMsg = new Thread(new ThreadStart(listen));
            rcMsg.Start();
            ClientEnvironment.fileStorage = "../../../BuilderFileStore/"+args[0];
            ServiceEnvironment.fileStorage = "../../../BuilderFileStore";
            Console.Write("\n  Press key to exit");
            Console.ReadKey();
            Console.Write("\n  ");
        }
    }
}
