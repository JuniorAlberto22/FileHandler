using System.Collections;
using System.IO;

namespace FileReadLAzy
{
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

        public byte GetNext()
        {
            byte toReturn =  GetByte(fileCursor);
            fileCursor++;
            return toReturn;
        }

        private byte GetByte(long index)
        {
            if(index > this.end)
            {
                Next();
            }
            return buffer[index - start];
        }

        public bool HasNext()
        {
            return fileCursor < fileLength;
        }


        private void initi()
        {
            this.start = 0;
            this.end = bufferLength - 1;
            this.reader.BaseStream.Seek(start, SeekOrigin.Begin);
            this.reader.Read(buffer, 0, buffer.Length);
            totalRead += bufferLength;
        }

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

        public void Close()
        {
            reader.Close();
            file.Close();
        }

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
