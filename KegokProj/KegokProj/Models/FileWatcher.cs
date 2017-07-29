using KegokProj.DAL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Hosting;

namespace KegokProj.Models
{
    public class FileWatcher
    {
        private FileSystemWatcher _watcher;
        private bool _enabled = true;
        private int readLinesCount = 0;

        public FileWatcher()
        {
            _watcher = new FileSystemWatcher();
            _watcher.Path = HostingEnvironment.MapPath("~/Content");
            _watcher.Filter = "*.txt";
            _watcher.Changed += Watcher_Changed;
            _watcher.Created += Watcher_Created;
        }

        public void Start()
        {
            _watcher.EnableRaisingEvents = true;
        }

        public void Stop()
        {
            _watcher.EnableRaisingEvents = false;
            _enabled = false;
        }

        
        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            try
            {
                //RecordEntry();
                string filePath = e.FullPath;
                
                RecordEntry(e.Name);
                _watcher.EnableRaisingEvents = false; //отключаем слежение
            }
            finally
            {
                _watcher.EnableRaisingEvents = true; //переподключаем слежение
            }
        }

        private void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            try
            {
                //RecordEntry();
                _watcher.EnableRaisingEvents = false; //отключаем слежение
            }
            finally
            {
                _watcher.EnableRaisingEvents = true; //переподключаем слежение
            }
        }

        public string ReadLine(string FilePath, int LineNumber)
        {
            string result = "";
            try
            {
                if (File.Exists(FilePath))
                {
                    using (StreamReader _StreamReader = new StreamReader(FilePath))
                    {
                        for (int a = 0; a < LineNumber; a++)
                        {
                            result = _StreamReader.ReadLine();
                        }
                    }
                }
            }
            catch { }
            return result;
        }

        //private void RecordEntry()
        //{
        //    Process proc = new Process();
        //    proc.StartInfo.FileName = HostingEnvironment.MapPath("~/Content/azaza.txt");
            
        //    using (StreamReader reader = new StreamReader(HostingEnvironment.MapPath("/Content/101_Sensors.txt")))
        //    {
        //        Data_access da = new Data_access();
        //        int totallinesCount = File.ReadAllLines(HostingEnvironment.MapPath("/Content/101_Sensors.txt")).Length;
        //        int newLinesCount = totallinesCount - readLinesCount;

        //        //var fileByLine = File.ReadLines(HostingEnvironment.MapPath("/Content/101_Sensors.txt")).Last();
        //        var fileByLine = File.ReadLines(HostingEnvironment.MapPath("/Content/101_Sensors.txt")).Skip(readLinesCount).Take(newLinesCount);
        //        var each = fileByLine.Select(l => l.Split('_'));
        //        string[] parsedFileContent = new string[fileByLine.Count()];
        //        foreach (var item in fileByLine)
        //        {
        //            parsedFileContent = item.Split('_');
        //            da.AddParams(parsedFileContent, totallinesCount, "/Content/101_Sensors.txt");
        //        }

        //        readLinesCount = totallinesCount;
        //    }
        //}

        private void RecordEntry(string filePath)
        {
            if (filePath != null)
            {
                Process proc = new Process();
                proc.StartInfo.FileName = HostingEnvironment.MapPath("~/Content/"+filePath);

                using (StreamReader reader = new StreamReader(HostingEnvironment.MapPath("/Content/"+filePath)))
                {
                    Data_access da = new Data_access();
                    int totallinesCount = File.ReadAllLines(HostingEnvironment.MapPath("/Content/"+filePath)).Length;
                    int newLinesCount = totallinesCount - readLinesCount;

                    //var fileByLine = File.ReadLines(HostingEnvironment.MapPath("/Content/101_Sensors.txt")).Last();
                    var fileByLine = File.ReadLines(HostingEnvironment.MapPath("/Content/"+filePath)).Skip(readLinesCount).Take(newLinesCount);
                    var each = fileByLine.Select(l => l.Split('_'));
                    string[] parsedFileContent = new string[fileByLine.Count()];
                    foreach (var item in fileByLine)
                    {
                        parsedFileContent = item.Split('_');
                        da.AddParams(parsedFileContent, totallinesCount, "/Content/"+filePath);
                    }

                    readLinesCount = totallinesCount;
                }
            }
        }
    }
}