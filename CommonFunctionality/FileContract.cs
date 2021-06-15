using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace CommonFunctionality
{
    public class FileContract : IDisposable
    {
        public FileContract(string filename)
        {
            FilePath = filename;
            _mutex = new();
        }

        public void CreateFile()
        {
            if (!Exists)
            {
                File.Create(FilePath);
            }
        }
        public void Append(List<string> text)
        {
            _mutex.WaitOne();

            File.AppendAllLines(FilePath, text);

            _mutex.ReleaseMutex();
        }
        public void UpdateLine(string searchFor, string newLine)
        {
            var lines = File.ReadAllLines(FilePath);

            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                if (lines[lineIndex].Contains(searchFor))
                {
                    lines[lineIndex] = newLine;

                    _mutex.WaitOne();
                    File.WriteAllLines(FilePath, lines);
                    _mutex.ReleaseMutex();

                    return;
                }
            }

            _mutex.WaitOne();
            File.AppendAllLines(FilePath, new string[] { newLine });
            _mutex.ReleaseMutex();
        }
        public List<string> Read()
        {
            if (Exists)
            {
                return new(File.ReadAllLines(FilePath));
            }


            throw new FileNotFoundException();
        }

        public bool Exists => File.Exists(FilePath);
        public string FilePath { get; }
        public string FileName => Path.GetFileName(FilePath);
        public string Directory => Path.GetDirectoryName(FilePath);
        private readonly Mutex _mutex;
        private bool _disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _mutex.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
