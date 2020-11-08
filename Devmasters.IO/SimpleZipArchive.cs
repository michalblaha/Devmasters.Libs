using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ICSharpCode.SharpZipLib.Zip;

namespace Devmasters.IO
{
    public class SimpleZipArchive : IDisposable
    {

        public string ArchiveFileName { get; set; }
        private string tmpFile { get { return this.ArchiveFileName + ".tmp"; } }


        FileStream zipFile = null;
        ZipOutputStream archive = null;
        ZipEntry entry = null;
        //TextWriter writer = null;
        //TextWriter swriter = null;
        public SimpleZipArchive(string archive, string packedFileName, bool overwrite = false)
        {
            this.ArchiveFileName = archive;
            var fi = new FileInfo(this.ArchiveFileName);
            if (overwrite == false && fi.Exists)
                throw new System.IO.IOException("File " + archive + " already exists.");

            if (fi.Directory.Exists == false)
                fi.Directory.Create();

            this.zipFile = new FileStream(this.tmpFile, FileMode.Create);
            this.archive = new ZipOutputStream(zipFile);
            this.archive.SetLevel(9);

            this.entry = new ZipEntry(packedFileName);//this.archive.CreateEntry(packedFileName, CompressionLevel.Optimal);

            this.archive.PutNextEntry(this.entry);
            //this.writer = new StreamWriter(this.entry.Open());
            //this.swriter = StreamWriter.Synchronized(this.writer);

        }
        public void Flush()
        {
            this.archive.Flush();
        }

        public void Write(string text)
        {
            byte[] data = System.Text.UTF8Encoding.UTF8.GetBytes(text);
            this.archive.Write(data,0,data.Length);
        }
        public void WriteLine(string text = null)
        {
            if (string.IsNullOrEmpty(text))
                this.Write("\n");
            else
                this.Write(text+"\n");
        }

        private void Finish()
        {
            this.Flush();
            System.Threading.Thread.Sleep(50);
            CloseAll();
            System.Threading.Thread.Sleep(50);
            System.IO.File.Delete(this.ArchiveFileName);
            System.IO.File.Move(this.tmpFile, this.ArchiveFileName);

        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        private void CloseAll()
        {
            if (this.entry != null)
                this.archive.CloseEntry();

            if (this.archive != null)
                this.archive.Dispose();
            if (this.zipFile != null)
                this.zipFile.Dispose();

        }

        object lockDisp = new object();
        protected virtual void Dispose(bool disposing)
        {
            lock (lockDisp)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        // TODO: dispose managed state (managed objects).

                    }

                    // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                    // TODO: set large fields to null.

                    disposedValue = true;
                    Finish();
                }
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ZipArchive()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
