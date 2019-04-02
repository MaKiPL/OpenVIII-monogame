using FFmpeg.AutoGen;
using System;
using System.Runtime.InteropServices;

namespace FF8
{
    internal static class FFmpegHelper
    {
        public static unsafe string av_strerror(int error)
        {
            var bufferSize = 1024;
            var buffer = stackalloc byte[bufferSize];
            ffmpeg.av_strerror(error, buffer, (ulong)bufferSize);
            var message = Marshal.PtrToStringAnsi((IntPtr)buffer);
            return message;
        }

        public static int ThrowExceptionIfError(this int error)
        {
            if (error < 0) throw new ApplicationException(av_strerror(error));
            return error;
        }
    }
    internal unsafe class Class1
    {
        private struct buffer_data
        {
            public byte* ptr;
            public int size; //< size left in the buffer
        };

        private static int FFMIN(int a, int b)
        {
            return a < b ? a : b;
        }

        private static int read_packet(void* opaque, byte* buf, int buf_size)
        {
            buffer_data* bd = (buffer_data*)opaque;
            buf_size = FFMIN(buf_size, bd->size);

            if (buf_size <= 0)
            {
                return ffmpeg.AVERROR_EOF;
            }

            //printf("ptr:%p size:%zu\n", bd->ptr, bd->size);

            /* copy internal buffer data to buf */

            memcpy(buf, bd->ptr, buf_size);
            bd->ptr += buf_size;
            bd->size -= buf_size;

            return buf_size;
        }

        private static void memcpy(byte* buf, byte* ptr, int buf_size)
        {
            for (int i = 0; i < buf_size; i++)
            {
                buf[0] = ptr[0];
            }
        }

        public Class1(byte* buffer, int buffer_size)
        {
            Run(buffer, buffer_size);
        }

        public bool Decode(out AVFrame frame,ref FfccVaribleGroup Decoder)
        {
            int Return = 0;
            ffmpeg.av_frame_unref(Decoder.Frame);
            do
            {
                try
                {
                    do
                    {
                        Return = ffmpeg.av_read_frame(Decoder.Format, Decoder.Packet);
                        if (Return == ffmpeg.AVERROR_EOF)
                        {
                            //if (LOOPSTART >= 0)
                            //{
                            //    ffmpeg.avformat_seek_file(Decoder.Format, DecoderStreamIndex, LOOPSTART - 1000, LOOPSTART, DecoderStream->duration, 0);



                            //    State = FfccState.WAITING;
                            //    if (BehindFrame())
                            //    {
                            //        timer.Restart();
                            //    }
                            //}
                            frame = *Decoder.Frame;
                            return false;
                        }
                        else
                        {
                            Return.ThrowExceptionIfError();
                        }
                    } while (Decoder.Packet->stream_index != Decoder.StreamIndex);


                    Return = ffmpeg.avcodec_send_packet(Decoder.CodecContext, Decoder.Packet);
                    //sent a eof when trying to loop once.
                    //should never get EOF here unless something is wrong.

                    Return.ThrowExceptionIfError();

                }
                finally
                {
                    ffmpeg.av_packet_unref(Decoder.Packet);
                }

                Return = ffmpeg.avcodec_receive_frame(Decoder.CodecContext, Decoder.Frame);
            } while (Return == ffmpeg.AVERROR(ffmpeg.EAGAIN));
            Return.ThrowExceptionIfError();

            frame = *Decoder.Frame;
            return true;
        }
        private void Run(byte* buffer, int buffer_size)
        {
            FfccVaribleGroup decoder = new FfccVaribleGroup();
            //AVFormatContext* decoder._format = null;
            AVIOContext* avio_ctx = null;
            byte* avio_ctx_buffer = null;
            int avio_ctx_buffer_size = 4096;
            string input_filename = null;
            int ret = 0;

            buffer_data bd = new buffer_data
            {

                //if (argc != 2) {
                //    private fprintf(stderr, "usage: %s input_file\n"
                //            "API example program to show how to read from a custom buffer "
                //            "accessed through AVIOContext.\n", argv[0]);
                //    return 1;
                //}
                //input_filename = argv;

                /* slurp file content into buffer */
                //ret = ffmpeg.av_file_map(input_filename, &buffer, &buffer_size, 0, null);
                //if (ret < 0)
                //{
                //    goto end;
                //}

                /* fill opaque structure used by the AVIOContext read callback */
                ptr = buffer,
                size = buffer_size
            };

            //if ((decoder._format = ffmpeg.avformat_alloc_context()) == null)
            //{
            //    ret = ffmpeg.AVERROR(ffmpeg.ENOMEM);
            //    goto end;
            //}

            avio_ctx_buffer = (byte*)ffmpeg.av_malloc((ulong)avio_ctx_buffer_size);
            if (avio_ctx_buffer == null)
            {
                ret = ffmpeg.AVERROR(ffmpeg.ENOMEM);
                goto end;
            }
            avio_alloc_context_read_packet rf = new avio_alloc_context_read_packet(read_packet);
            avio_ctx = ffmpeg.avio_alloc_context(avio_ctx_buffer, avio_ctx_buffer_size, 0, &bd, rf, null, null);
            if (avio_ctx == null)
            {
                ret = ffmpeg.AVERROR(ffmpeg.ENOMEM);
                goto end;
            }
            decoder._format->pb = avio_ctx;
            fixed (AVFormatContext** tmp = &decoder._format)
            {
                ret = ffmpeg.avformat_open_input(tmp, null, null, null);
            }

            if (ret < 0)
            {
                //private fprintf(stderr, "Could not open input\n");
                goto end;
            }

            ret = ffmpeg.avformat_find_stream_info(decoder._format, null);
            if (ret < 0)
            {
                //private fprintf(stderr, "Could not find stream information\n");
                goto end;
            }

            ffmpeg.av_dump_format(decoder._format, 0, input_filename, 0);

            end:
            //fixed (AVFormatContext** tmp = &decoder._format)
            //{
            //    ffmpeg.avformat_close_input(tmp);
            //}
            /* note: the internal buffer could have changed, and be != avio_ctx_buffer */
            if (avio_ctx != null)
            {
                ffmpeg.av_freep(&avio_ctx->buffer);
                ffmpeg.av_freep(&avio_ctx);
            }
            //ffmpeg.av_file_unmap(buffer, buffer_size);

            decoder.Dispose();
            if (ret < 0)
            {
                //private fprintf(stderr, "Error occurred: %s\n", av_err2str(ret));
                return;
            }

            return;
        }

        public Class1(byte[] rawBuffer)
        {
            fixed (byte* tmp = &rawBuffer[0])
            {
                Run(tmp, rawBuffer.Length);
            }
        }
    }
}
