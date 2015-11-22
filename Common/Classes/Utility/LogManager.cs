using System;
using System.IO;
using System.Text;

namespace Common.Classes.Utility
{
    public class LogManager : IDisposable
    {
        private bool STATUS = false;
        private string PREFIX = string.Empty;
        private FileStream Base_FileStream;
        private StringBuilder LogBuilder = new StringBuilder();

        public LogManager(string Path, string Prefix)
        {
            if(Directory.Exists(Path))
            {
                Base_FileStream = new FileStream(string.Format("{0}-{1}.log", Prefix, DateTime.Now.ToString("MM-dd-yyyy-HH")), FileMode.Create, FileAccess.ReadWrite, FileShare.None);
                STATUS = true;
            }
        }

        public bool GetStatus()
        {
            return STATUS;
        }

        public void Write(string Log)
        {
            if (STATUS)
            {
                LogBuilder.AppendLine(Log);
            }
        }

        public void Commit()
        {
            if (Base_FileStream != null)
            {
                Base_FileStream.Write(Encoding.Default.GetBytes(LogBuilder.ToString()), 0, LogBuilder.Length);
            }
        }

        public void Dispose()
        {
            if (Base_FileStream != null)
            {
                Base_FileStream.Close();
            }

            STATUS = false;
            LogBuilder = null;

            GC.SuppressFinalize(this);
        }
    }
}
