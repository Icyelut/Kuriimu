﻿using System;
using System.IO;
using archive_l7c.Compression;

namespace archive_ff1_dpk
{
    public abstract class BaseLz
    {
        protected virtual bool IsBackwards => false;
        protected virtual int PreBufferLength => 0;

        protected abstract Wp16Encoder CreateEncoder();
        protected abstract NewOptimalParser CreateParser(int inputLength);
        protected abstract Wp16Decoder CreateDecoder();

        public abstract string[] Names { get; }

        public void Decompress(Stream input, Stream output)
        {
            var decoder = CreateDecoder();

            decoder.Decode(input, output);

            decoder.Dispose();
        }

        public void Compress(Stream input, Stream output)
        {
            var encoder = CreateEncoder();
            var parser = CreateParser((int)input.Length);

            // Allocate array for input
            var inputArray = ToArray(input);

            // Parse matches
            var matches = parser.ParseMatches(inputArray, PreBufferLength);
            if (IsBackwards)
            {
                Array.Reverse(matches);
                foreach (var match in matches)
                    match.Position = input.Length - match.Position - 1;
            }

            // Encode matches and remaining raw data
            encoder.Encode(input, output, matches);

            // Dispose of objects
            encoder.Dispose();
            parser.Dispose();
        }

        private byte[] ToArray(Stream input)
        {
            var bkPos = input.Position;
            var inputArray = new byte[input.Length + PreBufferLength];
            var offset = IsBackwards ? 0 : PreBufferLength;

            input.Read(inputArray, offset, inputArray.Length - offset);
            if (IsBackwards)
                Array.Reverse(inputArray);

            input.Position = bkPos;
            return inputArray;
        }
    }
}
