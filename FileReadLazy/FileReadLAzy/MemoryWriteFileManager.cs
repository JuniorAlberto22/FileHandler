using System.Collections.Generic;
using System.IO;

namespace FileReadLAzy
{
    class MemoryWriteFileManager
    {
        private readonly string TEMP_ALIAS;
        private BinaryWriter writer;
        private FileStream file;
        private List<TreatByteArray> treatActions;

        public delegate byte[] TreatByteArray(MemoryReaderFileManager reader, byte[] filePart, int start, int end);

        private string filePath;
        private int bufferLength;
        private string fileTemp;

        public MemoryWriteFileManager(List<TreatByteArray> treatActions, string filePath, int bufferLength, string tempFileName)
        {
            this.TEMP_ALIAS = tempFileName;
            this.bufferLength = bufferLength;
            this.filePath = filePath;
            this.treatActions = treatActions;

            this.fileTemp = string.Concat(GetFilePath(filePath), TEMP_ALIAS);

            file = new FileStream(fileTemp, FileMode.Append);
            writer = new BinaryWriter(file);
            GetFilePath(filePath);
        }

        private string GetFilePath(string path)
        {
            return path.Replace(Path.GetFileName(path), "");
        }

        public void Process()
        {
            TreatFile();
        }

        private void TreatFile()
        {
            MemoryReaderFileManager readerManager = new MemoryReaderFileManager(this.bufferLength, filePath);
            long fileSize = readerManager.fileLength;

            int start, end;
            SetStartEnd(readerManager, out start, out end);
            byte[] pointer = Treat(readerManager.buffer, readerManager, start, end);
            Persist(this.writer, pointer, start, end);
            while (!readerManager.finished)
            {
                readerManager.Next();
                SetStartEnd(readerManager, out start, out end);
                pointer = Treat(readerManager.buffer, readerManager, start, end);
                Persist(this.writer, pointer, start, end);
            }
            readerManager.Close();
            this.Close();
        }

        private void SetStartEnd(MemoryReaderFileManager readerManager, out int start, out int end)
        {
            if (readerManager.finished)
            {
                start = 0;
                end = readerManager.LeftOver;
            }
            else
            {
                start = 0;
                end = readerManager.buffer.Length ;
            }
        }

        private void Persist(BinaryWriter writer, byte[] filePart, int start, int end)
        {
            writer.Write(filePart, start, end);
        }

        private byte[] Treat(byte[] filePart, MemoryReaderFileManager readerManager, int start, int end)
        {
            byte[] currentState = filePart;
            foreach(var act in treatActions)
            {
                currentState = act(readerManager, filePart, start, end);
            }
            return currentState;
        }

        public void Close()
        {
            this.file.Close();
            this.writer.Close();
        }
    }
}
