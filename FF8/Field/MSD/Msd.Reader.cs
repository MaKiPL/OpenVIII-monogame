using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace FF8
{
    public static partial class Msd
    {
        public static class Reader
        {
            public static IReadOnlyList<String> FromStream(Stream input, System.Text.Encoding encoding)
            {
                if (input.Position != 0)
                    throw new NotSupportedException($"Cannot read data from the middle of the stream. Position: {input.Position}/{input.Length}");

                Byte[] buff = new Byte[input.Position];
                return FromBytes(buff, encoding);
            }

            public static IReadOnlyList<String> FromBytes(Byte[] buff, System.Text.Encoding encoding)
            {
                List<String> monologues = new List<String>();

                Int32 bufferSize = buff.Length;
                if (bufferSize < 4)
                    return monologues;

                unsafe
                {
                    fixed (Byte* ptr = &buff[0])
                    {
                        SByte* textPtr = (SByte*)(ptr);
                        ReadMessages(textPtr, bufferSize, monologues, encoding);
                    }
                }

                return monologues;
            }

            private static unsafe void ReadMessages(SByte* textPtr, Int32 bufferSize, List<String> messages, System.Text.Encoding encoding)
            {
                Int32* offsets = (Int32*)textPtr;

                Int32 count = GetMessageNumber(bufferSize, offsets);
                if (count == 0)
                    return;

                messages.Capacity = count;

                Int32 currentOffset = offsets[0];
                for (Int32 i = 1; i < count; i++)
                {
                    Int32 nextOffset = offsets[i];
                    if (nextOffset == currentOffset)
                    {
                        messages.Add(String.Empty);
                        continue;
                    }

                    Int32 messageLength = nextOffset - currentOffset - 1;

                    String message = ReadMessage(textPtr, currentOffset, messageLength, bufferSize, encoding);
                    messages.Add(message);

                    currentOffset = nextOffset;
                }

                Int32 lastMessageLength = bufferSize - currentOffset - 1;
                String lastMessage = ReadMessage(textPtr, currentOffset, lastMessageLength, bufferSize, encoding);
                messages.Add(lastMessage);
            }

            private static unsafe Int32 GetMessageNumber(Int32 bufferSize, Int32* offsets)
            {
                Int32 dataOffset = offsets[0];
                Int32 count = dataOffset / 4;
                if (dataOffset % 4 != 0)
                    throw new InvalidDataException($"The offset to the beginning of the text data also determines the number of lines in the file and must be a multiple of 4. Occured: {dataOffset} mod 4 = {dataOffset % 4}");

                if (count < 0)
                    throw new InvalidDataException($"Unexpected negative value occured: {dataOffset}. Expected positive offset to the text data.");

                if (dataOffset > bufferSize)
                    throw new InvalidDataException($"Invalid data offset ({dataOffset}) is out of bounds ({bufferSize}).");

                return count;
            }

            private static unsafe String ReadMessage(SByte* textPtr, Int32 currentOffset, Int32 messageLength, Int32 bufferSize, System.Text.Encoding encoding)
            {
                if (currentOffset + messageLength > bufferSize)
                    throw new InvalidDataException($"Invalid text info (Offset: {currentOffset}, Size: {messageLength}) is out of bounds ({bufferSize}).");

                String message = new String(textPtr, currentOffset, messageLength, encoding);

                var lastCharacter = textPtr[currentOffset + messageLength];
                if (lastCharacter != 0 /* {End} */ && lastCharacter != 2 /* {Line} */)
                    throw new InvalidDataException($"Text must be a null-terminated string. Occured text (Offset: {currentOffset}, Size: {messageLength}): {message}");

                return message;
            }
        }
    }
}