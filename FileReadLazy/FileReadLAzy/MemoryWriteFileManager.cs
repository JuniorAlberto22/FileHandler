using System.Collections.Generic;
using System.IO;

namespace FileReadLazy
{
    /// <summary>
    /// Process one file from some delegates functions, where use the MemoryReaderFileManager internal to load file parts in buffer,
    /// and processes each part to write the result in a new file
    /// </summary>
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

        /// <summary>
        /// Return the path where is the file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string GetFilePath(string path)
        {
            return path.Replace(Path.GetFileName(path), "");
        }

        public void Process()
        {
            TreatFile();
        }

        /// <summary>
        /// Read part files and processes them, then write result in a new file.
        /// </summary>
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

        /// <summary>
        /// Return the usable files in buffer, that is, if the content file is minor then sizer buffer, the
        /// end is limited to it
        /// </summary>
        /// <param name="readerManager"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
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

        /// <summary>
        /// Persist the altered bytes in a new file
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="filePart"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        private void Persist(BinaryWriter writer, byte[] filePart, int start, int end)
        {
            writer.Write(filePart, start, end);
        }

        /// <summary>
        /// Run the subscribed functions to treat the bytes and return them
        /// </summary>
        /// <param name="filePart"></param>
        /// <param name="readerManager"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private byte[] Treat(byte[] filePart, MemoryReaderFileManager readerManager, int start, int end)
        {
            byte[] currentState = filePart;
            foreach(var act in treatActions)
            {
                currentState = act(readerManager, filePart, start, end);
            }
            return currentState;
        }

        /// <summary>
        /// Close the file channels, that is, BinaryWriter and FileStream
        /// </summary>
        public void Close()
        {
            this.file.Close();
            this.writer.Close();
        }
    }
}
