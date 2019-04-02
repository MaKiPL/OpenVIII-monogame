using FFmpeg.AutoGen;
using System;
using System.Runtime.InteropServices;

namespace FF8
{
    internal static class FFmpegHelper
    {
        public static unsafe string av_strerror(int error)
        {
            int bufferSize = 1024;
            byte* buffer = stackalloc byte[bufferSize];
            ffmpeg.av_strerror(error, buffer, (ulong)bufferSize);
            string message = Marshal.PtrToStringAnsi((IntPtr)buffer);
            return message;
        }

        public static int ThrowExceptionIfError(this int error)
        {
            if (error < 0)
            {
                throw new ApplicationException(av_strerror(error));
            }

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

        public static bool Decode(out AVFrame frame, ref FfccVaribleGroup Decoder)
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

        private static readonly AVMediaType MediaType = AVMediaType.AVMEDIA_TYPE_AUDIO;
        private void FindOpenCodec(ref FfccVaribleGroup Decoder)
        {
            int Return = 0;
            // find & open codec
            fixed (AVCodec** tmp = &Decoder._codec)
            {
                Return = ffmpeg.av_find_best_stream(Decoder.Format, MediaType, -1, -1, tmp, 0);
            }
            if (Return == ffmpeg.AVERROR_STREAM_NOT_FOUND && MediaType == AVMediaType.AVMEDIA_TYPE_AUDIO)
            {
                return;
            }
            else
            {
                Return.ThrowExceptionIfError();
            }

            Decoder.StreamIndex = Return;
            //Decoder.Stream = Decoder.Format->streams[Return];
            //GetTags(ref Decoder.Stream->metadata);
            Decoder.CodecContext = ffmpeg.avcodec_alloc_context3(Decoder.Codec);
            if (Decoder.CodecContext == null)
            {
                throw new Exception("Could not allocate codec context");
            }

            Return = ffmpeg.avcodec_parameters_to_context(Decoder.CodecContext, Decoder.Stream->codecpar);
            Return.ThrowExceptionIfError();
            AVDictionary* dict;
            Return = ffmpeg.av_dict_set(&dict, "strict", "+experimental", 0);
            Return.ThrowExceptionIfError();
            Return = ffmpeg.avcodec_open2(Decoder.CodecContext, Decoder.Codec, &dict);
            Return.ThrowExceptionIfError();
            if (MediaType == AVMediaType.AVMEDIA_TYPE_AUDIO)
            {
                if (Decoder.CodecContext->channel_layout == 0)
                {
                    Decoder.CodecContext->channel_layout = ffmpeg.AV_CH_LAYOUT_STEREO;
                }
            }
        }
        private void Run(byte* buffer, int buffer_size)
        {
            FfccVaribleGroup decoder = new FfccVaribleGroup();
            AVFormatContext* fmt_ctx = null;
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

            if ((fmt_ctx = ffmpeg.avformat_alloc_context()) == null)
            {
                ret = ffmpeg.AVERROR(ffmpeg.ENOMEM);
                ret.ThrowExceptionIfError();
                goto end;
            }

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
                ret.ThrowExceptionIfError();
                goto end;
            }
            fmt_ctx->pb = avio_ctx;
            ret = ffmpeg.avformat_open_input(&fmt_ctx, null, null, null);
            if (ret < 0)
            {
                ret.ThrowExceptionIfError();
                goto end;
            }

            ret = ffmpeg.avformat_find_stream_info(fmt_ctx, null);
            if (ret < 0)
            {
                ret.ThrowExceptionIfError();
                goto end;
            }

            ret = ffmpeg.avformat_find_stream_info(fmt_ctx, null);
            if (ret < 0)
            {
                ret.ThrowExceptionIfError();
                goto end;
            }

            ffmpeg.av_dump_format(fmt_ctx, 0, input_filename, 0);
            FindOpenCodec(ref decoder);

            end:
            //fixed (AVFormatContext** tmp = &fmt_ctx)
            //{
                ffmpeg.avformat_close_input(&fmt_ctx);
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
