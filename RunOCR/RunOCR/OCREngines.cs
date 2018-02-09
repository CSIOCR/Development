using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace RunOCR
{
    class OCREngines
    {
        // Filter out the pictures with the extension .Tif 
        public static List<string> FilterPicture(List<string> dirs)
        {
            return dirs.FindAll(delegate (string s) {
                return s.ToLower().Contains(".tif");
            });
        }

        public static void RunTesseract(List<string> dirs)
        {
            string cmdPath = "cmd.exe";

            foreach (string dir in dirs)
            {
                using (Process p = new Process())
                {
                    p.StartInfo.FileName = cmdPath;
                    p.StartInfo.UseShellExecute = false;        
                    p.StartInfo.RedirectStandardInput = true;   
                    p.StartInfo.RedirectStandardOutput = true;  
                    p.StartInfo.RedirectStandardError = true;   
                    p.StartInfo.CreateNoWindow = true;          
                    p.Start();

                    string picName = dir.Substring(0, dir.Length - 4);
                    string cmd = "tesseract " + dir + " " + picName + " -l num_All10000_Review+num_25+num_All10000_Updated batch.nochop makebox";
                    p.StandardInput.WriteLine(cmd);
                    p.StandardInput.WriteLine("exit");
                    p.StandardInput.AutoFlush = true;

                    //output = p.StandardOutput.ReadToEnd();
                    p.WaitForExit();
                    p.Close();
                    //Console.WriteLine(output);
                }
            }

        }

    }
}
