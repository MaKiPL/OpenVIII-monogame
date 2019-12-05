using System;
using System.IO;

namespace OpenVIII
{
    public static partial class Saves
    {
        #region Classes

        /// <summary>
        /// Configuration bytes.
        /// </summary>
        /// <see cref="https://github.com/alexfilth/quezacotl/blob/master/Quezacotl/InitWorker.cs"/>
        public class Configuration
        {
            #region Constructors

            public Configuration(BinaryReader br)
            {
                BattleSpeed = (BattleSpeed)br.ReadByte();
                BattleMessage = br.ReadByte();
                FieldMessage = br.ReadByte();
                Volume = br.ReadByte();
                Flag = (ConfigFlag)br.ReadByte();
                Scan = br.ReadByte();
                Camera = br.ReadByte();
                KeyUnk1 = br.ReadByte();
                KeyEscape = br.ReadByte();
                KeyPov = br.ReadByte();
                KeyWindow = br.ReadByte();
                KeyTrigger = br.ReadByte();
                KeyCancel = br.ReadByte();
                KeyMenu = br.ReadByte();
                KeyTalk = br.ReadByte();
                KeyTripleTriad = br.ReadByte();
                KeySelect = br.ReadByte();
                KeyUnk2 = br.ReadByte();
                KeyUnk3 = br.ReadByte();
                KeyStart = br.ReadByte();
            }

            #endregion Constructors

            #region Enums

            [Flags]
            public enum ConfigFlag : byte
            {
                Stereo = 0x0,
                Mono = 0x2,
                Initial = 0x0,
                Memory = 0x4,
            }

            #endregion Enums

            #region Properties

            public byte BattleMessage { get; private set; }
            public BattleSpeed BattleSpeed { get; private set; }
            public byte Camera { get; private set; }
            public byte FieldMessage { get; private set; }
            public ConfigFlag Flag { get; private set; }
            public byte KeyCancel { get; private set; }
            public byte KeyEscape { get; private set; }
            public byte KeyMenu { get; private set; }
            public byte KeyPov { get; private set; }
            public byte KeySelect { get; private set; }
            public byte KeyStart { get; private set; }
            public byte KeyTalk { get; private set; }
            public byte KeyTrigger { get; private set; }
            public byte KeyTripleTriad { get; private set; }
            public byte KeyUnk1 { get; private set; }
            public byte KeyUnk2 { get; private set; }
            public byte KeyUnk3 { get; private set; }
            public byte KeyWindow { get; private set; }
            public byte Scan { get; private set; }
            public byte Volume { get; private set; }

            #endregion Properties
        }

        #endregion Classes
    }
}