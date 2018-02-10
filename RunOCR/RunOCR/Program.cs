using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace RunOCR
{
    class Program
    {


        // All of the files in the input parameters, the first parameter
        static List<string> dirs = new List<string>();

        static List<string> newDir = new List<string>();

        static void Main(string[] args)
        {
            //1. parse parameters
            //The first parameter - a folder that contains picture files
            //The second parameter - output path of analytic results, a folder
            //The third parameter - "-r" or "-R" indicate that will query each folder recursively inside if the first parameter is a folder

            // determine the number of the parameters
            if (args.Length == 2)
            {
                if (Directory.Exists(args[0]))
                {
                    dirs.AddRange(Directory.GetFiles(args[0]));
                }
                else
                {
                    Console.WriteLine("{0} is not a valid directory.", args[0]);

                    return;
                }
            }
            else if (args.Length == 3)
            {
                if (Directory.Exists(args[0]) & (args[2] == @"-r" || args[2] == @"-R"))
                {
                    dirs = ProcessDirectory(args[0]);
                }
                else
                {
                    Console.WriteLine("{0} is not a valid directory.", args[0]);

                    return;
                }
            }
            else
            {
                Console.WriteLine("Parameters are invalid, please check!");

                return;
            }

            if (!Directory.Exists(args[1]))
            {
                Console.WriteLine("{0} is not a valid directory.", args[1]);

                return;
            }


            //2. optimize and process noise points  --  Yao Di

            //3. will .jpg convert to .tif format
            RunPictureConverter(args[0], PicFormat.JPG, PicFormat.TIF);

            //4. retrieve picture files and call tesseract to generate box result files
            //   1) retrieve all picture(.tif) files according to the first parameter and the third parameter
            //   2) by checking config file to read the path of tesseract(no need, manually add the path of tesseract into environment variables), then call it 

            // retrieve the files of the target folder due to the second step changed the number of the files
            newDir = ProcessDirectory(args[0]);

            List<string> TifPicture = FilterPicture(newDir, PicFormat.TIF);


            RunTesseract(TifPicture);


            //5. process noise point according to initializing result files
            //   e.g. if  one of the data likes "0 50 23 81 81 0", then a = 0, b = 50, c = 23, d = 81, e = 81, f = 0
            //   1) if (d - b) * (e - c) < 65 or (d - b) > 300 or d == 988 or b == 0, remove this one of the data

            //6. sort data
            //   sort each of the data in the result file based on the size of the b column value

            //7. will result outputs to target path
            //   1) after sorting, extract the first column of each piece of data and form a string, 
            //   2) will this string write in a text file that the same name as picture name,
            //   3) put this text file in the path of the second paramter that user specified

            //8. remove all temp files we generated from the directory user given the first parameter.


        }

        // Process all files in the directory passed in, recurse on any directories 
        // that are found, and process the files they contain.
        public static List<string> ProcessDirectory(string targetDirectory)
        {
            List<string> files = new List<string>();
            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
                files.Add(fileName);

            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
                ProcessDirectory(subdirectory);

            return files;
        }

        public static void RunTesseract(List<string> dirs)
        {
            string cmdPath = "cmd.exe";

            try
            {
                Parallel.ForEach(dirs, (dir) =>
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
                        p.StandardInput.WriteLine(cmd + "&exit");

                        p.StandardInput.AutoFlush = true;

                        string output = p.StandardOutput.ReadToEnd();
                        Console.WriteLine(output);
                        p.WaitForExit();
                        p.Close();
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception");
            }
        }

        // filter out specified format files in the specified folder
        public static List<string> FilterPicture(List<string> dirs, PicFormat format) => dirs.FindAll(delegate (string s)
        {

            switch (format)
            {
                case PicFormat.TIF:
                    return s.ToLower().Contains(".tif");
                case PicFormat.JPG:
                    return s.ToLower().Contains(".jpg");
                default:
                    return false;
            }

        });

        public static void RunPictureConverter(string folder, PicFormat oriFormat, PicFormat desFormat)
        {
            string cmdPath = "cmd.exe";
            using (Process p = new Process())
            {
                p.StartInfo.FileName = cmdPath;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.CreateNoWindow = false;
                p.Start();

                string cmd = "PictureConverter.exe " + folder + " " + oriFormat + " " + desFormat;
                p.StandardInput.WriteLine(cmd + "&exit");

                p.StandardInput.AutoFlush = true;

                string output = p.StandardOutput.ReadToEnd();
                Console.WriteLine(output);
                p.WaitForExit();
                p.Close();
            }
        }

        public enum PicFormat
        {
            TIF,
            JPG
        }
    }
}
