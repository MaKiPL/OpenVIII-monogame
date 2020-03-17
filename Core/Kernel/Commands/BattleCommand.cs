using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    namespace Kernel
    {
        /// <summary>
        /// Battle Commands
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Battle-commands"/>
        public sealed class BattleCommand : ICommand
        {
            #region Fields

            /// <summary>
            /// Section Count
            /// </summary>
            public const int Count = 39;

            /// <summary>
            /// Section ID
            /// </summary>
            public const int ID = 0;

            /// <summary>
            /// Alternative Section ID
            /// </summary>
            private const int AltLimitID = 18;

            #endregion Fields

            #region Constructors

            public BattleCommand(FF8String name, FF8String description, byte ability, Battle.Dat.UnkFlag flags, Target target, byte unknown, int battleID)
            => (Name, Description, Ability, Flags, Target, Unknown, BattleID) = (name, description, ability, flags, target, unknown, battleID);

            #endregion Constructors

            #region Properties

            /// <summary>
            /// Ability data
            /// </summary>
            public byte Ability { get; }

            /// <summary>
            /// Battle command ID
            /// </summary>
            public int BattleID { get; }

            /// <summary>
            /// ability description
            /// </summary>
            public FF8String Description { get; }

            /// <summary>
            /// Unknown Flags
            /// </summary>
            public Battle.Dat.UnkFlag Flags { get; }

            /// <summary>
            /// Name of Command
            /// </summary>
            public FF8String Name { get; }

            /// <summary>
            /// Target
            /// </summary>
            public Target Target { get; }

            /// <summary>
            /// Unknown / Unused
            /// </summary>
            public byte Unknown { get; }

            #endregion Properties

            #region Methods

            public static IReadOnlyList<BattleCommand> Read(BinaryReader br) =>
                Enumerable.Range(0, Count).Select(i => CreateInstance(br, i)).ToList();

            public override string ToString() => Name;

            private static BattleCommand CreateInstance(BinaryReader br, int i)
            {
                FF8StringReference description;
                FF8StringReference name;
                switch (i)
                {
                    //No Mercy
                    case 17:
                        name = Memory.Strings.Read(Strings.FileID.Kernel, AltLimitID, 0); //Fire Cross
                        description = Memory.Strings.Read(Strings.FileID.Kernel, AltLimitID, 1);
                        break;
                    //Sorcery
                    case 18:
                        name = Memory.Strings.Read(Strings.FileID.Kernel, AltLimitID, 2); //Ice Strike
                        description = Memory.Strings.Read(Strings.FileID.Kernel, AltLimitID, 3);
                        break;
                    //Limit #1
                    case 20:
                        name = Memory.Strings.Read(Strings.FileID.Kernel, AltLimitID, 4); //Desperado
                        description = Memory.Strings.Read(Strings.FileID.Kernel, AltLimitID, 5);
                        break;
                    //Limit #2
                    case 21:
                        name = Memory.Strings.Read(Strings.FileID.Kernel, AltLimitID, 6); //Blood Pain
                        description = Memory.Strings.Read(Strings.FileID.Kernel, AltLimitID, 7);
                        break;
                    //Limit #3
                    case 22:
                        name = Memory.Strings.Read(Strings.FileID.Kernel, AltLimitID, 8); //Massive Anchor
                        description = Memory.Strings.Read(Strings.FileID.Kernel, AltLimitID, 9);
                        break;

                    default:
                        name = Memory.Strings.Read(Strings.FileID.Kernel, ID, i * 2);
                        description = Memory.Strings.Read(Strings.FileID.Kernel, ID, i * 2 + 1);
                        break;
                }

                br.BaseStream.Seek(4, SeekOrigin.Current);
                byte ability = br.ReadByte();
                Battle.Dat.UnkFlag flags = (Battle.Dat.UnkFlag)br.ReadByte();
                Target target = (Target)br.ReadByte();
                byte unknown = br.ReadByte();
                return new BattleCommand(name, description, ability, flags, target, unknown, i);
            }

            #endregion Methods
        }
    }
}