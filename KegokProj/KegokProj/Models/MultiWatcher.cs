using KegokProj.DAL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Web;
using System.Web.Hosting;

namespace KegokProj.Models
{
    public class MultiWatcher : IDisposable
    {
        private List<string> filePaths;
        private ReaderWriterLockSlim rwlock;
        private System.Timers.Timer processTimer;
        private string watchedPath;
        private FileSystemWatcher watcher;
        private int readLinesCount = 0;

        public MultiWatcher(string watchedPath)
        {
            filePaths = new List<string>();

            rwlock = new ReaderWriterLockSlim();

            this.watchedPath = watchedPath;
            InitFileSystemWatcher();
        }

        private void InitFileSystemWatcher()
        {
            watcher = new FileSystemWatcher();
            watcher.Filter = "*.txt";
            watcher.Created += Watcher_FileCreated;
            watcher.Changed += Watcher_FileChanged;
            watcher.Error += Watcher_Error;
            watcher.Path = watchedPath;
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;
        }

        private void Watcher_Error(object sender, ErrorEventArgs e)
        {
            // Watcher crashed. Re-init.
            InitFileSystemWatcher();
        }

        private void Watcher_FileCreated(object sender, FileSystemEventArgs e)
        {
            try
            {
                rwlock.EnterWriteLock();
                filePaths.Add(e.FullPath);
                
                if (processTimer == null)
                {
                    // First file, start timer.
                    processTimer = new System.Timers.Timer(2000);
                    processTimer.Elapsed += ProcessQueue;
                    processTimer.Start();
                }
                else
                {
                    // Subsequent file, reset timer.
                    processTimer.Stop();
                    processTimer.Start();
                }

            }
            finally
            {
                rwlock.ExitWriteLock();
            }
        }

        private void Watcher_FileChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                rwlock.EnterWriteLock();
                filePaths.Add(e.Name);
                
                if (processTimer == null)
                {
                    // First file, start timer.
                    processTimer = new System.Timers.Timer(3000);
                    processTimer.Elapsed += ProcessQueue;
                    processTimer.Start();
                }
                else
                {
                    // Subsequent file, reset timer.
                    processTimer.Stop();
                    processTimer.Start();
                }
            }
            finally
            {
                rwlock.ExitWriteLock();
            }
        }

        private void ProcessQueue(object sender, ElapsedEventArgs args)
        {
            try
            {
                Console.WriteLine("Processing queue, " + filePaths.Count + " files created:");
                rwlock.EnterReadLock();
                foreach (string filePath in filePaths)
                {
                    Console.WriteLine(filePath);
                }
                filePaths.Clear();
            }
            finally
            {
                if (processTimer != null)
                {
                    processTimer.Stop();
                    processTimer.Dispose();
                    processTimer = null;
                }
                rwlock.ExitReadLock();
            }
        }

        private void RecordEntry(string fileName)
        {
            if (fileName != null)
            {
                Process proc = new Process();
                proc.StartInfo.FileName = HostingEnvironment.MapPath("~/Content/" + fileName);

                using (StreamReader reader = new StreamReader(HostingEnvironment.MapPath("/Content/Sensors/" + fileName)))
                {
                    Data_access da = new Data_access();
                    int totallinesCount = File.ReadAllLines(HostingEnvironment.MapPath("/Content/Sensors/" + fileName)).Length;
                    int newLinesCount = totallinesCount - readLinesCount;

                    //var fileByLine = File.ReadLines(HostingEnvironment.MapPath("/Content/101_Sensors.txt")).Last();
                    var fileByLine = File.ReadLines(HostingEnvironment.MapPath("/Content/Sensors/" + fileName)).Skip(readLinesCount).Take(newLinesCount);
                    var each = fileByLine.Select(l => l.Split('_'));
                    string[] parsedFileContent = new string[fileByLine.Count()];
                    foreach (var item in fileByLine)
                    {
                        parsedFileContent = item.Split('_');
                        da.AddParams(parsedFileContent, totallinesCount, "/Content/Sensors/" + fileName);
                    }

                    readLinesCount = totallinesCount;
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (rwlock != null)
                {
                    rwlock.Dispose();
                    rwlock = null;
                }
                if (watcher != null)
                {
                    watcher.EnableRaisingEvents = false;
                    watcher.Dispose();
                    watcher = null;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}