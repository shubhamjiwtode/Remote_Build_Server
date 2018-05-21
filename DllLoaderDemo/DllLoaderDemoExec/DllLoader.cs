

/////////////////////////////////////////////////////////////////////
// DllLoader.cs acepts test logs and loads the loader               //
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
 * static public void UpdateDllLoader(string buildPath, string y,string[] dllFiles)//updates the path,name of log file, provide names of .dll to be built
 *  public static void InitiateTest()//initiates test over .dll
 * string loadAndExerciseTesters()//loads libraries and start tests
 *  
 * Required Files:
 * ---------------
 * testHarness.cs
 * 
 * Maintenance History:
 * --------------------
 * 
 * ver 2.0      12/05/2017 
 */

using System;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DllLoaderDemo
{
    public class DllLoaderExec
    {
        private static string testersLocation = "";
        private static string logFileN = "";
        private static System.IO.StreamWriter log;
        private static string LogFilePath = "../../../ThFileStore";
        private static string[] files;

        /*----< library binding error event handler >------------------*/
        /*
         *  This function is an event handler for binding errors when
         *  loading libraries.  These occur when a loaded library has
         *  dependent libraries that are not located in the directory
         *  where the Executable is running.
         */
        static Assembly LoadFromComponentLibFolder(object sender, ResolveEventArgs args)
        {
            Console.Write("\n  called binding error event handler");
            string folderPath = testersLocation;
            string assemblyPath = Path.Combine(folderPath, new AssemblyName(args.Name).Name + ".dll");
            if (!File.Exists(assemblyPath)) return null;
            Assembly assembly = Assembly.LoadFrom(assemblyPath);
            return assembly;
        }
        //----< load assemblies from testersLocation and run their tests >-----
        /////////////////////////////////////////////////////////////////////////loads libraries and start tests
        string loadAndExerciseTesters()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += new ResolveEventHandler(LoadFromComponentLibFolder);

            try
            {
                DllLoaderExec loader = new DllLoaderExec();
                
                files  = Directory.GetFiles(testersLocation, "*.dll");
               

                Console.Write("\n\n============================================================================================");
                foreach (string file in files)

                {
                    Console.Write("\nLoaded file is {0}", file);
                    Console.Write("\n\n============================================================================================");
                    Assembly asm = Assembly.LoadFile(file);
                    string fileName = Path.GetFileName(file);
                    Console.Write("\n  loaded {0}", fileName);
                    Type[] types = asm.GetTypes();
                    foreach (Type t in types)
                    {
                        if (t.GetInterface("DllLoaderDemo.ITest", true) != null)
                            if (!loader.runSimulatedTest(t, asm, logFileN))
                            {
                                Console.Write("\n  test {0} failed to run", t.ToString());

                                using (log = new System.IO.StreamWriter(System.IO.Path.Combine(LogFilePath, logFileN + ".txt"), true))
                                {
                                    log.WriteLine("\n  test {0} failed to run", t.ToString());
                                }
                            }
                            else
                            {
                                Console.Write("\n  test {0} succeeded", t.ToString());
                                using (log = new System.IO.StreamWriter(System.IO.Path.Combine(LogFilePath, logFileN + ".txt"), true))
                                {
                                    log.WriteLine("\n  test {0} succeeded", t.ToString());
                                }
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "Simulated Testing completed";
        }

        //----< run tester t from assembly asm >-------------------------------

        bool runSimulatedTest(Type t, Assembly asm, string LogFile)
        {
            try
            {
                Console.Write(
                  "\n  attempting to create instance of {0}", t.ToString()
                  );
                object obj = asm.CreateInstance(t.ToString());
                MethodInfo method = t.GetMethod("say");
                if (method != null)
                    method.Invoke(obj, new object[0]);
                bool status = false;
                method = t.GetMethod("test");
                if (method != null)
                    status = (bool)method.Invoke(obj, new object[0]);
                
                Func<bool, string> act = (bool pass) =>
                {
                    if (pass)
                    {
                        using (log = new System.IO.StreamWriter(System.IO.Path.Combine(LogFilePath, LogFile + ".txt"), true))
                        {
                            log.WriteLine(Environment.NewLine + "passed");
                            
                        }
                        return "passed";
                    }
                    else
                    {
                        using (log = new System.IO.StreamWriter(System.IO.Path.Combine(LogFilePath, LogFile + ".txt"), true))
                        {
                            log.WriteLine(Environment.NewLine + "failed");
                        }

                        return "failed";
                    }
                };
                Console.Write("\n  test {0}", act(status));
            }
            catch (Exception ex)
            {
                Console.Write("\n  test failed with message \"{0}\"", ex.Message);
                return false;
            }
            return true;
        }

        //----< extract name of current directory without its parents ---------

        string GuessTestersParentDir()
        {
            string dir = Directory.GetCurrentDirectory();
            int pos = dir.LastIndexOf(Path.DirectorySeparatorChar);
            string name = dir.Remove(0, pos + 1).ToLower();
            if (name == "debug")
                return "../..";
            else
                return ".";
        }
        //----< run demonstration >--------------------------------------------
        /////////////////////////////////////////////////////////////////////Initiates the test
        public static void InitiateTest()
        {
            System.IO.Directory.CreateDirectory("../../../ThFileStore");
            using (log = new System.IO.StreamWriter(System.IO.Path.Combine(LogFilePath, logFileN + ".txt")))
            {
                log.Write("Test Started at: " + DateTime.Now.ToString() + Environment.NewLine);
                foreach (System.Collections.DictionaryEntry env in Environment.GetEnvironmentVariables())
                {
                    log.WriteLine((string)env.Key + "=" + (string)env.Value);
                }
                log.WriteLine(Environment.NewLine + "loading" + Environment.NewLine);
               
                
            }
                Console.Write("\n  Demonstrating Robust Test Loader");
            DllLoaderExec loader = new DllLoaderExec();
            DllLoaderExec.testersLocation = Path.GetFullPath(DllLoaderExec.testersLocation);
            Console.Write("\n  Loading Test Modules from:\n    {0}\n", DllLoaderExec.testersLocation);
            string result = loader.loadAndExerciseTesters();

          
            Console.WriteLine(result);
            using (log = new System.IO.StreamWriter(System.IO.Path.Combine(LogFilePath, logFileN + ".txt"),true))
            {
                log.WriteLine(DateTime.Now.ToString());
            }
            using (log = new System.IO.StreamWriter(System.IO.Path.Combine(LogFilePath, logFileN + ".txt"),true))
            {
                log.WriteLine(result);
                Console.Write(result);
            }
            Console.WriteLine("\nTest logs generated at {0} ", System.IO.Path.GetFullPath(System.IO.Path.Combine("../../../ThFileStore")));
        }


        /////////////////////////////////////////////////////////////////////updates the path,name of log file, provide names of .dll to be built

        static public void UpdateDllLoader(string buildPath, string y,string[] dllFiles)
        {
            testersLocation = buildPath;
            logFileN = y;
            files = dllFiles;
        }
    }
}

