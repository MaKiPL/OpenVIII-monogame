using FFmpeg.AutoGen;

namespace FF8
{
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
        private void Run(byte* buffer,int buffer_size)
        {
            AVFormatContext* fmt_ctx = null;
            AVIOContext* avio_ctx = null;
            byte* avio_ctx_buffer = null;
            int  avio_ctx_buffer_size = 4096;
            string input_filename = null;
            int ret = 0;

            buffer_data bd = new buffer_data();

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
            bd.ptr = buffer;
            bd.size = buffer_size;

            if ((fmt_ctx = ffmpeg.avformat_alloc_context()) == null)
            {
                ret = ffmpeg.AVERROR(ffmpeg.ENOMEM);
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
                goto end;
            }
            fmt_ctx->pb = avio_ctx;

            ret = ffmpeg.avformat_open_input(&fmt_ctx, null, null, null);
            if (ret < 0)
            {
                //private fprintf(stderr, "Could not open input\n");
                goto end;
            }

            ret = ffmpeg.avformat_find_stream_info(fmt_ctx, null);
            if (ret < 0)
            {
                //private fprintf(stderr, "Could not find stream information\n");
                goto end;
            }

            ffmpeg.av_dump_format(fmt_ctx, 0, input_filename, 0);

            end:
            ffmpeg.avformat_close_input(&fmt_ctx);
            /* note: the internal buffer could have changed, and be != avio_ctx_buffer */
            if (avio_ctx != null)
            {
                ffmpeg.av_freep(&avio_ctx->buffer);
                ffmpeg.av_freep(&avio_ctx);
            }
            //ffmpeg.av_file_unmap(buffer, buffer_size);

            if (ret < 0)
            {
                //private fprintf(stderr, "Error occurred: %s\n", av_err2str(ret));
                return;
            }

            return;
        }

        public Class1(byte[] rawBuffer)
        {
            fixed(byte* tmp = &rawBuffer[0])
            {
                Run(tmp, rawBuffer.Length);
            }
        }
    }
}
