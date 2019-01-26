﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;

namespace TestTaskFileCompression
{
    public sealed class ScheduledWriter
    {
        private static volatile object instanceMutex = new object();

        private static ScheduledWriter instance;

        private readonly List<ZipResult> queue;

        private bool isStreamSliceFinished;

        private int totalPartCount;

        private string outputFilePath;
        private byte[] buffer;
        private CompressionMode mode;

        private ScheduledWriter()
        {
            queue = new List<ZipResult>();

            isStreamSliceFinished = false;

            totalPartCount = 0;
        }

        public static ScheduledWriter Instance
        {
            get
            {
                lock (instanceMutex)
                {
                    return instance ?? ( instance = new ScheduledWriter() );
                }
            }
        }

        public void SetNewResult(ZipResult result)
        {
            lock (queue)
            {
                queue.Add(result);
            }
        }

        public void SetOutputFile(string outFile) { outputFilePath = outFile; }

        public void SetIsStreamSliced() { isStreamSliceFinished = true; }

        public void IncrementPartCount() { totalPartCount++; }

        public void SetOperation(CompressionMode mode) { this.mode = mode; }

        public void StartWorker()
        {
            try
            {
                StartWriterWorker(1000);
            }
            catch (Exception e)
            {
                var errorMessage = "Unhandled exception in writer worker: " + e.Message;
                Console.WriteLine(e);
            }
        }

        private void StartWriterWorker(int millisecondsToSleep)
        {
            var outFileStream = File.Create(outputFilePath);

            var wrotePartCount = 0;
            while (!isStreamSliceFinished && totalPartCount > 0 || wrotePartCount < totalPartCount)
            {
                bool isNextPartExist;
                var nextPart = new ZipResult(0,null);
                lock (queue)
                {

                    isNextPartExist = queue
                        .Any(item => item.PartIndex == wrotePartCount);

                    if (isNextPartExist)
                    {
                        nextPart = queue
                            .First(item => item.PartIndex == wrotePartCount);
                    }
                }

                if (isNextPartExist)
                {
                    if (mode == CompressionMode.Compress)
                    {
                        var bytes = BitConverter.GetBytes((int)nextPart.ResultStream.Length);
                        outFileStream.Write(bytes, 0, 4);
                    }

                    nextPart.ResultStream.Seek(0, SeekOrigin.Begin);
                    buffer = new byte[nextPart.ResultStream.Length];
                    nextPart.ResultStream.CopyTo(outFileStream, buffer, 0, buffer.Length);
                    buffer = null;

                    nextPart.ResultStream.Close();

                    wrotePartCount++;
                }
                else
                {
                    Thread.Sleep(millisecondsToSleep);
                }
            }

            outFileStream.Close();
        }
    }
}