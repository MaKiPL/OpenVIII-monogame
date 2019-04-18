using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF8
{
    /// <summary>
    /// MSG files are FF8 Scrabled text strings. Each string ending with a \0
    /// </summary>
    class MSG
    {
        void readfile()
        {
            //text is prescrabbled and is ready to draw to screen using font renderer

            //based on what I see here some parts of menu ignore \n and some will not
            //example when you highlight a item to refine it will only show only on one line up above.
            //  1 will refine into 20 Thundaras
            //and when you select it the whole string will show.
            //  Coral Fragment:
            //  1 will refine 
            //  into 20 Thundaras

            //m000.msg = 104 strings
            //example = Coral Fragment:\n1 will refine \ninto 20 Thundaras\0
            //I think this one is any item that turns into magic
            //___ Mag-RF items? except for upgrade abilities

            //m001.msg = 145 strings
            //same format differnt items
            //example = Tent:\n4 will refine into \n1 Mega-Potion\0
            //I think this one is any item that turns into another item
            //___ Med-RF items? except for upgrade abilities
            //guessing Ammo-RF is in here too.

            //m002.msg = 10 strings
            //same format differnt items
            //example = Fire:\n5 will refine \ninto 1 Fira\0
            //this one is magic tha turns into higher level magic
            //first 4 are Mid Mag-RF
            //last 6 are High Mag-RF

            //m003.msg = 12 strings
            //same format differnt items
            //example = Elixer:\n10 will refine \ninto 1 Megalixir\0
            //this one is Med items tha turns into higher level Med items
            //all 12 are Med LV Up

            //m004.msg = 110 strings
            //same format differnt items
            //example = Geezard Card:\n1 will refine \ninto 5 Screws\0
            //this one is converts cards into items
            //all 110 are Card Mod

            //mwepon.msg = 34 strings
            //all strings are " " or "  " kinda a odd file.

            //pet_exp.msg = 18 strings
            //format: ability name\0description\0
            //{0x0340} = Angelo's name
            //example: {0x0340} Rush\0 Damage one enemy\0
            //list of Angelo's attack names and descriptions

            //namedic.bin 32 strings
            //Seems to be location names.
            //start of file
            //  UIint16 Count
            //  UIint16[Count]Location
            //at each location
            //  Byte[Count][Bytes to null]


        }
    }
}
