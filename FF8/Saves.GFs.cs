using System;
using System.Collections.Generic;

namespace FF8
{






    public static partial class Saves
    {
        static public Dictionary<GFflags, GFs> ConvertGFEnum = new Dictionary<GFflags, GFs> {
            { GFflags.Alexander,GFs.Alexander },
            { GFflags.Bahamut,GFs.Bahamut },
            { GFflags.Brothers,GFs.Brothers },
            { GFflags.Cactuar,GFs.Cactuar },
            { GFflags.Carbuncle,GFs.Carbuncle },
            { GFflags.Cerberus,GFs.Cerberus },
            { GFflags.Diablos,GFs.Diablos },
            { GFflags.Doomtrain,GFs.Doomtrain },
            { GFflags.Eden,GFs.Eden },
            { GFflags.Ifrit,GFs.Ifrit },
            { GFflags.Leviathan,GFs.Leviathan },
            { GFflags.Pandemona,GFs.Pandemona },
            { GFflags.Quezacotl,GFs.Quezacotl },
            { GFflags.Shiva,GFs.Shiva },
            { GFflags.Siren,GFs.Siren },
            { GFflags.Tonberry,GFs.Tonberry },
        };
        public enum GFs
        {
            Quezacotl,
            Shiva,
            Ifrit,
            Siren,
            Brothers,
            Diablos,
            Carbuncle,
            Leviathan,
            Pandemona,
            Cerberus,
            Alexander,
            Doomtrain,
            Bahamut,
            Cactuar,
            Tonberry,
            Eden
        }
        [Flags]
        public enum GFflags
        {
            None=0,
            Quezacotl=0x1,
            Shiva = 0x2,
            Ifrit = 0x4,
            Siren = 0x8,
            Brothers = 0x10,
            Diablos = 0x20,
            Carbuncle = 0x40,
            Leviathan = 0x80,
            Pandemona = 0x100,
            Cerberus = 0x200,
            Alexander = 0x400,
            Doomtrain = 0x800,
            Bahamut = 0x1000,
            Cactuar = 0x2000,
            Tonberry = 0x4000,
            Eden = 0x8000,
        }
    }
}