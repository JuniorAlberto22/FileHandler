using System.Collections;
using System.IO;

namespace FileReadLazy
{
    /// <summary>
    /// Reade file from local storage from one limited buffer. When arrive in one point where the content is not in the memory,
    /// the required content will load in the memory
    /// </summary>
    class MemoryReaderFileManager : IEnumerable
    {
        private string fileName;
        private long totalRead;
        private int bufferLength;
        private long fileCursor;
        private long start, end;

        private FileStream file;
        private BinaryReader reader;

        public byte[] buffer { get; private set; }
        public bool finished { get; private set; }
        public long fileLength;
        public int LeftOver {get; set;}

        public MemoryReaderFileManager(int bufferLength, string fileName)
        {
            this.finished = false;
            this.fileName = fileName;
            this.totalRead = 0;
            this.fileCursor = 0;
            this.bufferLength = bufferLength;
            this.buffer = new byte[bufferLength];

            this.file = new FileStream(fileName, FileMode.Open);
            this.reader = new BinaryReader(this.file);
            this.fileLength = this.file.Length;
            this.LeftOver = (int)(this.fileLength % this.bufferLength);
            initi();
        }

        /// <summary>
        /// Return the next byte from file
        /// </summary>
        /// <returns></returns>
        public byte GetNext()
        {
            byte toReturn =  GetByte(fileCursor);
            fileCursor++;
            return toReturn;
        }

        /// <summary>
        /// Return byte contained in the index 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private byte GetByte(long index)
        {
            if(index > this.end)
            {
                Next();
            }
            return buffer[index - start];
        }

        /// <summary>
        /// Return true if exist one more byte to read
        /// </summary>
        /// <returns></returns>
        public bool HasNext()
        {
            return fileCursor < fileLength;
        }

        /// <summary>
        /// Instance the initial values
        /// </summary>
        private void initi()
        {
            this.start = 0;
            this.end = bufferLength - 1;
            this.reader.BaseStream.Seek(start, SeekOrigin.Begin);
            this.reader.Read(buffer, 0, buffer.Length);
            totalRead += bufferLength;
        }

        /// <summary>
        /// Reload the buffer with the new bytes loaded from file
        /// </summary>
        public void Next()
        {
            if (!finished)
            {
                this.start = this.end + 1;
                this.end += this.buffer.Length;
                this.reader.BaseStream.Seek(start, SeekOrigin.Begin);
                this.reader.Read(buffer, 0, buffer.Length);
                totalRead += bufferLength;
            }
            if(totalRead >= fileLength)
            {
                finished = true;
            }
        }

        /// <summary>
        /// Close file channels, that is, BinaryReader and FileStreamer classes
        /// </summary>
        public void Close()
        {
            reader.Close();
            file.Close();
        }

        /// <summary>
        /// Return one Enumerator for class utilization in foreach loops
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            MyEnumarator ienum = new MyEnumarator(this);
            return ienum.GetEnumerator();
        }

        private class MyEnumarator : IEnumerator
        {
            MemoryReaderFileManager manager;
            public MyEnumarator(MemoryReaderFileManager manager)
            {
                this.manager = manager;
            }

            public IEnumerator GetEnumerator()
            {
                return this;
            }

            public object Current => manager.GetNext();

            public bool MoveNext()
            {
                bool toReturn = manager.HasNext();
                if (!toReturn)
                {
                    Reset();
                }
                return toReturn;
            }

            public void Reset()
            {
                manager.finished = false;
                manager.totalRead = 0;
                manager.fileCursor = 0;
                manager.start = 0;
                manager.end = manager.bufferLength - 1;
                manager.Next();
            }
        }
    }
}
