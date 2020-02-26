namespace OpenVIII
{
    public enum CompressionType
    {
        /// <summary>
        /// Not compressed.
        /// </summary>
        None = 0,
        /// <summary>
        /// Compressed with LZSS
        /// </summary>
        /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF7/LZSS_format"/>
        /// <seealso cref="https://github.com/niemasd/PyFF7/wiki/LZSS-Format"/>
        LZSS = 1,
        /// <summary>
        /// Compressed with LZ4
        /// </summary>
        /// <see cref="https://lz4.github.io/lz4/"/>
        LZ4 = 2,
        /// <summary>
        /// Real value would be 0 if writing the file.
        /// <para>Compressed with lzss. We are probably unsure the uncompressed size.</para>
        /// </summary>
        /// <remarks>LZS files are compressed with LZSS</remarks>
        LZSS_UnknownSize = 3,
        /// <summary>
        /// Real value would be 1 if writing the file. Though could be 0 if you didn't wanna waste time compressing twice.
        /// <para>Compressed twice with lzss. The secondtime we are probably unsure the uncompressed size.</para>
        /// </summary>
        /// <remarks>LZS files are compressed with LZSS</remarks>
        LZSS_LZSS = 4,
    }
}