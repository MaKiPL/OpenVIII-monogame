namespace OpenVIII.Encoding.Tags
{
    /// <summary>
    /// <para>Notes cursor locations</para>
    /// <para>Often followed by a third byte that determines the selection</para>
    /// <para>Used in MNUGRP</para>
    /// </summary>
    public enum FF8TextTagOption : byte
    {
        x20 = 0x20,
        x21 = 0x21,
        x22 = 0x22,
        x23 = 0x23,
    }
}