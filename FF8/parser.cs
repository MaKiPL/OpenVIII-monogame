using FFmpeg.AutoGen;
using System;
using System.IO;
using System.Linq;


namespace FF8
{
    internal unsafe class Parser
    {
        private const int AUDIO_INBUF_SIZE = 20480;
        private const int AUDIO_REFILL_THRESH = 4096;

        private static void decode(AVCodecContext* dec_ctx, AVPacket* pkt, AVFrame* frame, ref MemoryStream outData)//, FILE* outfile)
        {
            int i;
            uint ch;
            int ret, data_size;
            /* send the packet with the compressed data to the decoder */
            ret = ffmpeg.avcodec_send_packet(dec_ctx, pkt);
            if (ret < 0)
            {
                throw new Exception("Error submitting the packet to the decoder\n");

            }
            /* read all the output frames (in general there may be any number of them */
            while (ret >= 0)
            {
                ret = ffmpeg.avcodec_receive_frame(dec_ctx, frame);
                if (ret == ffmpeg.AVERROR(ffmpeg.EAGAIN) || ret == ffmpeg.AVERROR_EOF)
                {
                    return;
                }
                else if (ret < 0)
                {
                    throw new Exception("Error during decoding\n");

                }
                data_size = ffmpeg.av_get_bytes_per_sample(dec_ctx->sample_fmt);
                if (data_size < 0)
                {
                    /* This should not occur, checking just for paranoia */
                    throw new Exception("Failed to calculate data size\n");

                }
                //for (i = 0; i < frame->nb_samples; i++)
                //    for (ch = 0; ch < dec_ctx->channels; ch++)
                //        fwrite(frame->data[ch] + data_size * i, 1, data_size, outfile);
                for (i = 0; i < frame->nb_samples; i++)
                {
                    for (ch = 0; ch < dec_ctx->channels; ch++)
                    {
                        for(int j=0; j < data_size; j++)
                        outData.WriteByte(frame->data[ch][data_size * i+j]);
                    }
                }
            }
        }

        public Parser(byte [] inbuf, ref MemoryStream outData)
        {
        //    string outfilename, filename;
            AVCodec* codec;
            AVCodecContext* c = null;
            AVCodecParserContext* parser = null;
            int len, ret;
            //FILE* f, *outfile;
            //byte[] inbuf = new byte[AUDIO_INBUF_SIZE + ffmpeg.AV_INPUT_BUFFER_PADDING_SIZE];

            int data_size;
            AVPacket* pkt;
            AVFrame* decoded_frame = null;
            //if (args.Count() <= 2)
            //{
            //    throw new Exception($"Usage: this.exe <input file> <output file>\n");

            //}
            //filename = args[1];
            //outfilename = args[2];
            /* register all the codecs */
            //ffmpeg.avcodec_register_all();
            pkt = ffmpeg.av_packet_alloc();
            //AV_CODEC_ID_ADPCM_MS
            codec = ffmpeg.avcodec_find_decoder(AVCodecID.AV_CODEC_ID_ADPCM_IMA_WAV);
            if (codec == null)
            {
                throw new Exception("Codec not found\n");

            }
            parser = ffmpeg.av_parser_init((int)codec->id);
            if (parser == null)
            {
                throw new Exception("Parser not found\n");

            }
            c = ffmpeg.avcodec_alloc_context3(codec);
            if (c == null)
            {
                throw new Exception("Could not allocate audio codec context\n");

            }
            /* open it */
            if (ffmpeg.avcodec_open2(c, codec, null) < 0)
            {
                throw new Exception("Could not open codec\n");

            }
            //f = fopen(filename, "rb");
            //if (!f)
            //{
            //    throw new Exception( "Could not open %s\n", filename);

            //}
            //outfile = fopen(outfilename, "wb");
            //if (!outfile)
            //{
            //    av_free(c);

            //}
            /* decode until eof */
            data_size = inbuf.Length;//fread(inbuf, 1, AUDIO_INBUF_SIZE, f);
            int loc = 0;
            while (data_size > 0)
            {
                fixed (byte* data = &inbuf[loc])
                {
                    if (decoded_frame == null)
                    {
                        if ((decoded_frame = ffmpeg.av_frame_alloc()) == null)
                        {
                            throw new Exception("Could not allocate audio frame\n");

                        }
                    }
                    ret = ffmpeg.av_parser_parse2(parser, c, &pkt->data, &pkt->size,
                                           data, data_size,
                                           ffmpeg.AV_NOPTS_VALUE, ffmpeg.AV_NOPTS_VALUE, 0);
                    if (ret < 0)
                    {
                        throw new Exception("Error while parsing\n");

                    }
                    loc += ret;
                    data_size -= ret;
                    if (pkt->size > 0)
                    {
                        decode(c, pkt, decoded_frame, ref outData);
                    }

                    if (data_size < AUDIO_REFILL_THRESH)
                    {
                        //memmove(inbuf, data, data_size);
                        //data = inbuf;
                        //len = fread(data + data_size, 1, AUDIO_INBUF_SIZE - data_size, f);
                        //if (len > 0)
                        //    data_size += len;
                    }
                }
            }
            /* flush the decoder */
            pkt->data = null;
            pkt->size = 0;
            decode(c, pkt, decoded_frame, ref outData);
            //fclose(outfile);
            //fclose(f);
            ffmpeg.avcodec_free_context(&c);
            ffmpeg.av_parser_close(parser);
            ffmpeg.av_frame_free(&decoded_frame);
            ffmpeg.av_packet_free(&pkt);
            return;
        }
    }
}
