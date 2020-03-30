using System;
using System.Diagnostics.CodeAnalysis;

namespace OpenVIII.Fields
{
    [Flags]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum Sections : uint
    {
        None = 0,

        /// <summary>
        /// Field Character Models
        /// </summary>
        MCH = 0x1,

        /// <summary>
        /// Field Character Models Container
        /// </summary>
        ONE = 0x2,

        /// <summary>
        /// Field Background Image Data
        /// </summary>
        MIM = 0x4,

        /// <summary>
        /// Field Background Tile Data
        /// </summary>
        MAP = 0x8,

        /// <summary>
        /// Field Background
        /// </summary>
        Background = Sections.MIM | Sections.MAP,

        /// <summary>
        /// Field Scripts
        /// </summary>
        JSM = 0x10,

        /// <summary>
        /// Field Script names (unused)
        /// </summary>
        SYM = 0x20,

        /// <summary>
        /// Field Dialogs
        /// </summary>
        MSD = 0x40,

        /// <summary>
        /// Field Gateways
        /// </summary>
        INF = 0x80,

        /// <summary>
        /// Field Walk-mesh(same format as FF7)
        /// </summary>
        ID = 0x100,

        /// <summary>
        /// Field Camera
        /// </summary>
        CA = 0x200,

        /// <summary>
        /// Extra font
        /// </summary>
        TDW = 0x400,

        /// <summary>
        /// Movie cam(?)
        /// </summary>
        MSK = 0x800,

        /// <summary>
        /// Battle rate
        /// </summary>
        RAT = 0x1000,

        /// <summary>
        /// Battle encounter
        /// </summary>
        MRT = 0x2000,

        /// <summary>
        /// Particle Info
        /// </summary>
        PMD = 0x4000,

        /// <summary>
        /// Particle Image Data
        /// </summary>
        PMP = 0x8000,

        /// <summary>
        /// Unknown(often 0x0c000000, sometimes 0x0a000000 or 0x0b000000)
        /// </summary>
        PVP = 0x10000,

        /// <summary>
        /// Indexes to Sound Effects(?)
        /// </summary>
        SFX = 0x20000,

        /// <summary>
        /// All files
        /// </summary>
        ALL = 0x3FFFF,
    }
}