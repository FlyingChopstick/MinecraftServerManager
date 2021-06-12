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

        public void CreateFile()
        {
            if (!Exists)
            {
                File.Create(FilePath);
            }
        }
        public void Append(List<string> text)
        {
            lock (_lock)
            {
                File.AppendAllLines(FilePath, text);
            }
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
        private readonly object _lock = new();
    }
}
