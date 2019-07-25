namespace OpenVIII.Encoding.Tags
{
    public enum FF8TextTagCode : byte
    {
        // Без параметров
        End = 0x00,
        
        Next = 0x01,
        Line = 0x02,
        Speaker = 0x12,

        // С параметром (байт)
        Var = 0x04,
        Pause = 0x09,

        // С параметром (именованый)
        Char = 0x03,
        Key = 0x05,
        Color = 0x06,
        Dialog = 0x0A,
        Option = 0x0B,
        Term = 0x0E,
        UNK = 0x0C
    }
}