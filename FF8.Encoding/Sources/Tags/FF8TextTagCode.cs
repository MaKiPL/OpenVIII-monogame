namespace FF8.Encoding.Tags
{
    public enum FF8TextTagCode
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
        Term = 0x0E,
    }
}