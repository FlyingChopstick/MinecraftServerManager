using System;
using System.Collections.Generic;
using System.IO;

namespace CommonFunctionality
{
    public class FileContract
    {
        public FileContract(string filename)
        {
            FilePath = filename;
        }

        public void Append(List<string> text)
        {
            lock (_lock)
            {
                try
                {
                    File.AppendAllLines(FilePath, text);
                }
                catch (Exception)
                {

                }
            }
        }

        public bool Exists => File.Exists(FilePath);
        public string FilePath { get; }
        public string FileName => Path.GetFileName(FilePath);
        private readonly object _lock = new();
    }
}
