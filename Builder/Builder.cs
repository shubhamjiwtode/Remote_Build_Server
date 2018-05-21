/////////////////////////////////////////////////////////////////////
// program.cs - Spawn and handle child process                     //
//                                                                 //
// Author: SHUBHAM JIWTODE                                         //
// Application: CSE681-Software Modeling and Analysis Demo         //
// Environment: C# console                                         //
// Source: Jim Fawcett                                             //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * ===================
 * Spawns child processes 
 * Maintain child process operation using ready Q and Build req Q
 * 
 * Interfaces:
 * -------------
 * static bool createProcess(int i) // Takes child process id and creates that particular child process.
 *  static void sendtoChild()// If ready Q and Build request Q is not empty , it sends build request to childprocess
 *  static void listen()//Receiver listning for any incoming file.
 *  static void initListen() //Initiates the receiver
 *  public static void killMother()// First it kills the child process and then kills Mother builder(itself)
 * 
 * Required Files:
 * ---------------
 * 
 * Client.cs
 * ChildProc.cs
 * MPCommService.cs
 * IMPCommService.cs
 * 
 * Maintenance History:
 * --------------------
 * ver 1.0 :27 Sep 2017
 * - first release
 * 
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagePassingComm;
using System.Threading;

namespace Project3
{
    class Builder
    {
        public static SWTools.BlockingQueue<int> readyQ { get; set; } = new SWTools.BlockingQueue<int>();
        public static SWTools.BlockingQueue<string> brQ { get; set; } = new SWTools.BlockingQueue<string>();
        private static CommMessage rcMsg;
        private static Receiver rc;
        private static int motherPort = 8080;


        /////////////////////////////////////////////////////////////// Takes child process id and creates that particular child process.
        static bool createProcess(int i)
        {
            Process proc = new Process();
            
            string fileName = "..\\..\\..\\ChildProc\\bin\\debug\\ChildProc.exe";
            string absFileSpec = Path.GetFullPath(fileName);
      
            Console.Write("\n  attempting to start {0}", absFileSpec);
            string commandline = i.ToString();
            try
            {
                Process.Start(fileName, commandline);

            }
            catch (Exception ex)
            {
                Console.Write("\n  {0}", ex.Message);
                return false;
            }
            return true;
        }

        /////////////////////////////////////////////////////////////// If ready Q and Build request Q is not empty ,
        /////////////////////////////////////////////////////////////// it sends build request to childprocess
        static void sendtoChild()
        {
            while (true)
            {
                if (readyQ != null && brQ != null)
                {
                    int  avail_CP= readyQ.deQ();
                    string avail_Req = brQ.deQ();
                    Sender send = new Sender("http://localhost", avail_CP);
                    CommMessage sendMsg = new CommMessage(CommMessage.MessageType.request);
                    sendMsg.from = "http://localhost:" + 8080 + "/IpluggableComm";
                    sendMsg.to = "http://localhost:" + avail_CP + "/IpluggableComm";
                    List<string> ls = new List<string>();
                    ls.Add(avail_Req);
                    ls.Add(avail_CP.ToString());
                    sendMsg.command = "sendtoChild";
                    sendMsg.arguments = ls;
                    sendMsg.author = "SHUBHAM RAMESH JIWTODE";
                    send.postMessage(sendMsg);
                    Console.WriteLine("-------------------sending {0} to Child Process {1}", avail_Req,avail_CP);
                }
            }
        }


        static void initListen() {
            rc = new Receiver();
            rc.start("http://localhost",motherPort);
        }
       
        ///////////////////// Receiver listning for any incoming file
        
        static void listen() {
            
            while (true)
            {
                rcMsg = rc.getMessage();
                switch (rcMsg.command)
                {
                    case "m_ready":
                        {
                            rcMsg.show();
                            readyQ.enQ(Int32.Parse(rcMsg.from));
                            break;

                        }

                    case "sendR2MP":
                        {
                            rcMsg.show();
                            brQ.enQ(rcMsg.arguments[0]);
                            break;

                        }
                    case "KillMP":
                        {
                            rcMsg.show();
                            killMother();
                            break;

                        }

                    default:
                        break;
                }





            }
        }
        /////////////////////////////////////////////////////////////// First it kills the child process and then kills Mother builder(itself)
        public static void killMother() {
            try
            {
                foreach (var process in Process.GetProcessesByName("ChildProc"))
                {
                    process.Kill();
                }
                Thread.Sleep(1000);
                Process[] Proc = Process.GetProcessesByName("Builder");
                if (Proc.Length > 0)
                    Proc[0].Kill();
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0}", ex.Message);
            }
        }


        static void Main(string[] args)
        {           
            initListen();  
            Thread rcMsg = new Thread(new ThreadStart(listen));
            rcMsg.Start();
            Thread sendReq = new Thread(new ThreadStart(sendtoChild));
            sendReq.Start();
            Console.Title = "SpawnProc";
               
                Console.Write("\n  Mother Process");
                Console.Write("\n =====================");

                if (Int32.Parse(args[0]) != 0)
                {
                  
                    for (int i = 1; i <= Int32.Parse(args[0]); ++i)
                    {
                        if (createProcess(i))
                        {
                            Console.Write(" - succeeded");
                        }
                        else
                        {
                            Console.Write(" - failed");
                        }
                    }
                }
               
          

            Console.Read();
                Console.Write("\n  ");
            

        }
    }
}
