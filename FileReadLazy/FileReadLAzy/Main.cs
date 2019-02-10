using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using static FileReadLAzy.MemoryWriteFileManager;

namespace FileReadLAzy
{
    class MyMain
    {
        public static void Main()
        {
            string myPath = @"C:\Users\Alberto\Desktop\large.jpg";
            string tempFileName = "tempFile";
            int bufferSize = 1024;

            //read with forech
            MemoryReaderFileManager memoryReader = new MemoryReaderFileManager(bufferSize, myPath);
            foreach(var b in memoryReader)
            {
                //Console.Write(Convert.ToChar(b));
            }
            Console.WriteLine("Read Finalized");
            memoryReader.Close();

            //read with while, is very slow
            memoryReader = new MemoryReaderFileManager(bufferSize, myPath);
            while (memoryReader.HasNext())
            {
                //Console.Write((char)memoryReader.GetNext());
            }
            memoryReader.Close();
            Console.WriteLine("Read Finalized");

            //Lendo arquivo e somando +1 em cada byte do aquivo
            MemoryWriteFileManager writeManager = new MemoryWriteFileManager(new List<TreatByteArray>()
            {
                (reader, filerPart, start, end) => {
                    for(int i = 0; i < end; i++)
                    {
                        filerPart[i] = (byte) (filerPart[i] + 1);
                    }
                    return filerPart;
                }
            },
            myPath, bufferSize, tempFileName);
            writeManager.Process();
            Console.WriteLine("Write Finalized");

            //Lendo arquivo e removendo -1 em cada byte do aquivo
            writeManager = new MemoryWriteFileManager(new List<TreatByteArray>()
            {
                (reader, filerPart, start, end) => {
                    for(int i = 0; i < end; i++)
                    {
                        filerPart[i] = (byte) (filerPart[i] - 1);
                    }
                    return filerPart;
                }
            },
            @"C:\Users\Alberto\Desktop\tempFile", bufferSize, "tempFile.png");
            writeManager.Process();
            Console.WriteLine("Write Finalized");
        }
    }
}
