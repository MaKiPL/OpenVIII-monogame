using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FF8
{
    public class Font
    {
        private Texture2D sysfnt; //21x10 characters; char is always 12x12
        private TextureHandler sysfntbig; //21x10 characters; char is always 24x24; 2 files side by side; sysfnt00 is same as sysfld00, but sysfnt00 is missing sysfnt01
        private Texture2D menuFont;

        #region CharTable

        //private static readonly Dictionary<byte, string> chartable = new Dictionary<byte, string>
        //{
        //    //Commented out special bytes so they are passed through and I can see them in the dump file. Then I can figure out what to do with them.
        //    {0x00, "\0"},// pos:-32, col:0, row:-1 -- is end of a string. MSG files have more than one string sepperated by \0
        //    {0x01, ""},// pos:-31, col:0, row:0 -- //some strings start with 0x01 unsure if it does anything.
        //    {0x02, "\n"},// pos:-30, col:0, row:0 -- new line
        //    //{0x03, ""},// pos:-29, col:0, row:0 -- special character. {0x03,0x40 = [Angelo's Name]}
        //    //{0x04, ""},// pos:-28, col:0, row:0 --
        //    //{0x05, ""},// pos:-27, col:0, row:0 --
        //    //{0x06, ""},// pos:-26, col:0, row:0 --
        //    //{0x07, ""},// pos:-25, col:0, row:0 --
        //    //{0x08, ""},// pos:-24, col:0, row:0 --
        //    //{0x09, ""},// pos:-23, col:0, row:0 --
        //    //{0x0A, ""},// pos:-22, col:0, row:0 --
        //    //{0x0B, ""},// pos:-21, col:0, row:0 --
        //    //{0x0C, ""},// pos:-20, col:0, row:0 -- <MAGBYTE>
        //    //{0x0D, ""},// pos:-19, col:0, row:0 --
        //    //{0x0E, ""},// pos:-18, col:0, row:0 -- <$>
        //    //{0x0F, ""},// pos:-17, col:0, row:0 --
        //    //{0x10, ""},// pos:-16, col:0, row:0 --
        //    //{0x11, ""},// pos:-15, col:0, row:0 --
        //    //{0x12, ""},// pos:-14, col:0, row:0 --
        //    //{0x13, ""},// pos:-13, col:0, row:0 --
        //    //{0x14, ""},// pos:-12, col:0, row:0 --
        //    //{0x15, ""},// pos:-11, col:0, row:0 --
        //    //{0x16, ""},// pos:-10, col:0, row:1 --
        //    //{0x17, ""},// pos:-9, col:0, row:0 --
        //    //{0x18, ""},// pos:-8, col:0, row:0 --
        //    //{0x19, ""},// pos:-7, col:0, row:0 --
        //    //{0x1A, ""},// pos:-6, col:0, row:0 --
        //    //{0x1B, ""},// pos:-5, col:0, row:0 --
        //    //{0x1C, ""},// pos:-4, col:0, row:0 --
        //    //{0x1D, ""},// pos:-3, col:0, row:0 --
        //    //{0x1E, ""},// pos:-2, col:0, row:0 --
        //    //{0x1F, ""},// pos:-1, col:0, row:0 --
        //    {0x20, " "},// pos:0, col:1, row:1 -- Start of font texture
        //    {0x21, "0"},// pos:1, col:2, row:1 --
        //    {0x22, "1"},// pos:2, col:3, row:1 --
        //    {0x23, "2"},// pos:3, col:4, row:1 --
        //    {0x24, "3"},// pos:4, col:5, row:1 --
        //    {0x25, "4"},// pos:5, col:6, row:1 --
        //    {0x26, "5"},// pos:6, col:7, row:1 --
        //    {0x27, "6"},// pos:7, col:8, row:1 --
        //    {0x28, "7"},// pos:8, col:9, row:1 --
        //    {0x29, "8"},// pos:9, col:10, row:1 --
        //    {0x2A, "9"},// pos:10, col:11, row:1 --
        //    {0x2B, "%"},// pos:11, col:12, row:2 --
        //    {0x2C, "/"},// pos:12, col:13, row:2 --
        //    {0x2D, ":"},// pos:13, col:14, row:2 --
        //    {0x2E, "!"},// pos:14, col:15, row:2 --
        //    {0x2F, "?"},// pos:15, col:16, row:2 --
        //    {0x30, "…"},// pos:16, col:17, row:2 --
        //    {0x31, "+"},// pos:17, col:18, row:2 --
        //    {0x32, "-"},// pos:18, col:19, row:2 --
        //    {0x33, "="},// pos:19, col:20, row:2 --
        //    {0x34, "*"},// pos:20, col:21, row:2 --
        //    {0x35, "&amp;"},//& pos:21, col:1, row:2 -- temporarly set to this so i could have formatting on pastebin
        //    {0x36, "「"},// pos:22, col:2, row:2 --
        //    {0x37, "」"},// pos:23, col:3, row:2 --
        //    {0x38, "("},// pos:24, col:4, row:2 --
        //    {0x39, ")"},// pos:25, col:5, row:2 --
        //    {0x3A, "·"},// pos:26, col:6, row:2 --
        //    {0x3B, "."},// pos:27, col:7, row:2 --
        //    {0x3C, ","},// pos:28, col:8, row:2 --
        //    {0x3D, "~"},// pos:29, col:9, row:2 --
        //    {0x3E, "“"},// pos:30, col:10, row:2 --
        //    {0x3F, "”"},// pos:31, col:11, row:2 --
        //    {0x40, "'"},// pos:32, col:12, row:3 --
        //    {0x41, "#"},// pos:33, col:13, row:3 --
        //    {0x42, "$"},// pos:34, col:14, row:3 --
        //    {0x43, "`"},// pos:35, col:15, row:3 --
        //    {0x44, "_"},// pos:36, col:16, row:3 --
        //    {0x45, "A"},// pos:37, col:17, row:3 --
        //    {0x46, "B"},// pos:38, col:18, row:3 --
        //    {0x47, "C"},// pos:39, col:19, row:3 --
        //    {0x48, "D"},// pos:40, col:20, row:3 --
        //    {0x49, "E"},// pos:41, col:21, row:3 --
        //    {0x4A, "F"},// pos:42, col:1, row:3 --
        //    {0x4B, "G"},// pos:43, col:2, row:3 --
        //    {0x4C, "H"},// pos:44, col:3, row:3 --
        //    {0x4D, "I"},// pos:45, col:4, row:3 --
        //    {0x4E, "J"},// pos:46, col:5, row:3 --
        //    {0x4F, "K"},// pos:47, col:6, row:3 --
        //    {0x50, "L"},// pos:48, col:7, row:3 --
        //    {0x51, "M"},// pos:49, col:8, row:3 --
        //    {0x52, "N"},// pos:50, col:9, row:3 --
        //    {0x53, "O"},// pos:51, col:10, row:3 --
        //    {0x54, "P"},// pos:52, col:11, row:3 --
        //    {0x55, "Q"},// pos:53, col:12, row:4 --
        //    {0x56, "R"},// pos:54, col:13, row:4 --
        //    {0x57, "S"},// pos:55, col:14, row:4 --
        //    {0x58, "T"},// pos:56, col:15, row:4 --
        //    {0x59, "U"},// pos:57, col:16, row:4 --
        //    {0x5A, "V"},// pos:58, col:17, row:4 --
        //    {0x5B, "W"},// pos:59, col:18, row:4 --
        //    {0x5C, "X"},// pos:60, col:19, row:4 --
        //    {0x5D, "Y"},// pos:61, col:20, row:4 --
        //    {0x5E, "Z"},// pos:62, col:21, row:4 --
        //    {0x5F, "a"},// pos:63, col:1, row:4 --
        //    {0x60, "b"},// pos:64, col:2, row:4 --
        //    {0x61, "c"},// pos:65, col:3, row:4 --
        //    {0x62, "d"},// pos:66, col:4, row:4 --
        //    {0x63, "e"},// pos:67, col:5, row:4 --
        //    {0x64, "f"},// pos:68, col:6, row:4 --
        //    {0x65, "g"},// pos:69, col:7, row:4 --
        //    {0x66, "h"},// pos:70, col:8, row:4 --
        //    {0x67, "i"},// pos:71, col:9, row:4 --
        //    {0x68, "j"},// pos:72, col:10, row:4 --
        //    {0x69, "k"},// pos:73, col:11, row:4 --
        //    {0x6A, "l"},// pos:74, col:12, row:5 --
        //    {0x6B, "m"},// pos:75, col:13, row:5 --
        //    {0x6C, "n"},// pos:76, col:14, row:5 --
        //    {0x6D, "o"},// pos:77, col:15, row:5 --
        //    {0x6E, "p"},// pos:78, col:16, row:5 --
        //    {0x6F, "q"},// pos:79, col:17, row:5 --
        //    {0x70, "r"},// pos:80, col:18, row:5 --
        //    {0x71, "s"},// pos:81, col:19, row:5 --
        //    {0x72, "t"},// pos:82, col:20, row:5 --
        //    {0x73, "u"},// pos:83, col:21, row:5 --
        //    {0x74, "v"},// pos:84, col:1, row:5 --
        //    {0x75, "w"},// pos:85, col:2, row:5 --
        //    {0x76, "x"},// pos:86, col:3, row:5 --
        //    {0x77, "y"},// pos:87, col:4, row:5 --
        //    {0x78, "z"},// pos:88, col:5, row:5 --
        //    {0x79, "À"},// pos:89, col:6, row:5 --
        //    {0x7A, "Á"},// pos:90, col:7, row:5 --
        //    {0x7B, "Â"},// pos:91, col:8, row:5 --
        //    {0x7C, "Ä"},// pos:92, col:9, row:5 --
        //    {0x7D, "Ç"},// pos:93, col:10, row:5 --
        //    {0x7E, "È"},// pos:94, col:11, row:5 --
        //    {0x7F, "É"},// pos:95, col:12, row:6 --
        //    {0x80, "Ê"},// pos:96, col:13, row:6 --
        //    {0x81, "Ë"},// pos:97, col:14, row:6 --
        //    {0x82, "Ì"},// pos:98, col:15, row:6 --
        //    {0x83, "Í"},// pos:99, col:16, row:6 --
        //    {0x84, "Î"},// pos:100, col:17, row:6 --
        //    {0x85, "Ï"},// pos:101, col:18, row:6 --
        //    {0x86, "Ñ"},// pos:102, col:19, row:6 --
        //    {0x87, "Ò"},// pos:103, col:20, row:6 --
        //    {0x88, "Ó"},// pos:104, col:21, row:6 --
        //    {0x89, "Ô"},// pos:105, col:1, row:6 --
        //    {0x8A, "Ö"},// pos:106, col:2, row:6 --
        //    {0x8B, "Ù"},// pos:107, col:3, row:6 --
        //    {0x8C, "Ú"},// pos:108, col:4, row:6 --
        //    {0x8D, "Û"},// pos:109, col:5, row:6 --
        //    {0x8E, "Ü"},// pos:110, col:6, row:6 --
        //    {0x8F, "Œ"},// pos:111, col:7, row:6 --
        //    {0x90, "Ʀ"},// pos:112, col:8, row:6 --
        //    {0x91, "à"},// pos:113, col:9, row:6 --
        //    {0x92, "á"},// pos:114, col:10, row:6 --
        //    {0x93, "â"},// pos:115, col:11, row:6 --
        //    {0x94, "ä"},// pos:116, col:12, row:7 --
        //    {0x95, "ç"},// pos:117, col:13, row:7 --
        //    {0x96, "è"},// pos:118, col:14, row:7 --
        //    {0x97, "é"},// pos:119, col:15, row:7 --
        //    {0x98, "ê"},// pos:120, col:16, row:7 --
        //    {0x99, "ë"},// pos:121, col:17, row:7 --
        //    {0x9A, "ì"},// pos:122, col:18, row:7 --
        //    {0x9B, "í"},// pos:123, col:19, row:7 --
        //    {0x9C, "î"},// pos:124, col:20, row:7 --
        //    {0x9D, "ï"},// pos:125, col:21, row:7 --
        //    {0x9E, "ñ"},// pos:126, col:1, row:7 --
        //    {0x9F, "ò"},// pos:127, col:2, row:7 --
        //    {0xA0, "ó"},// pos:128, col:3, row:7 --
        //    {0xA1, "ô"},// pos:129, col:4, row:7 --
        //    {0xA2, "ö"},// pos:130, col:5, row:7 --
        //    {0xA3, "ù"},// pos:131, col:6, row:7 --
        //    {0xA4, "ú"},// pos:132, col:7, row:7 --
        //    {0xA5, "û"},// pos:133, col:8, row:7 --
        //    {0xA6, "ü"},// pos:134, col:9, row:7 --
        //    {0xA7, "œ"},// pos:135, col:10, row:7 --
        //    {0xA8, "Ⅷ"},// pos:136, col:11, row:7 --
        //    {0xA9, "["},// pos:137, col:12, row:8 --
        //    {0xAA, "]"},// pos:138, col:13, row:8 --
        //    {0xAB, "⬛"},// pos:139, col:14, row:8 --
        //    {0xAC, "⦾"},// pos:140, col:15, row:8 --
        //    {0xAD, "◆"},// pos:141, col:16, row:8 --
        //    {0xAE, "【"},// pos:142, col:17, row:8 --
        //    {0xAF, "】"},// pos:143, col:18, row:8 --
        //    {0xB0, "⬜"},// pos:144, col:19, row:8 --
        //    {0xB1, "★"},// pos:145, col:20, row:8 --
        //    {0xB2, "『"},// pos:146, col:21, row:8 --
        //    {0xB3, "』"},// pos:147, col:1, row:8 --
        //    {0xB4, "▽"},// pos:148, col:2, row:8 --
        //    {0xB5, ";"},// pos:149, col:3, row:8 --
        //    {0xB6, "▼"},// pos:150, col:4, row:8 --
        //    {0xB7, "‾"},// pos:151, col:5, row:8 --
        //    {0xB8, "×"},// pos:152, col:6, row:8 --
        //    {0xB9, "☆"},// pos:153, col:7, row:8 --
        //    {0xBA, "’"},// pos:154, col:8, row:8 --
        //    {0xBB, "↓"},// pos:155, col:9, row:8 --
        //    {0xBC, "°"},// pos:156, col:10, row:8 --
        //    {0xBD, "¡"},// pos:157, col:11, row:8 --
        //    {0xBE, "¿"},// pos:158, col:12, row:9 --
        //    {0xBF, "—"},// pos:159, col:13, row:9 --
        //    {0xC0, "«"},// pos:160, col:14, row:9 --
        //    {0xC1, "»"},// pos:161, col:15, row:9 --
        //    {0xC2, "±"},// pos:162, col:16, row:9 --
        //    {0xC3, ""},// pos:163, col:17, row:9 --
        //    //{0xC4, ""},//{0xC4, "♫"},// pos:164, col:18, row:9 -- seems to be used as an alignment or a place holder many strings have 3 of these in a row.
        //    {0xC5, "↑"},// pos:165, col:19, row:9 --
        //    {0xC6, "VI"},// pos:166, col:20, row:9 --
        //    {0xC7, "II"},// pos:167, col:21, row:9 --
        //    {0xC8, "¡"},// pos:168, col:1, row:9 --
        //    {0xC9, "™"},// pos:169, col:2, row:9 --
        //    {0xCA, "<"},// pos:170, col:3, row:9 --
        //    {0xCB, ">"},// pos:171, col:4, row:9 --
        //    {0xCC, "GA"},// pos:172, col:5, row:9 --
        //    {0xCD, "ME"},// pos:173, col:6, row:9 --
        //    {0xCE, "FO"},// pos:174, col:7, row:9 --
        //    {0xCF, "LD"},// pos:175, col:8, row:9 --
        //    {0xD0, "ER"},// pos:176, col:9, row:9 --
        //    {0xD1, "Sl"},// pos:177, col:10, row:9 --
        //    {0xD2, "ot"},// pos:178, col:11, row:9 --
        //    {0xD3, "ing"},// pos:179, col:12, row:10 --
        //    {0xD4, "St"},// pos:180, col:13, row:10 --
        //    {0xD5, "ec"},// pos:181, col:14, row:10 --
        //    {0xD6, "kp"},// pos:182, col:15, row:10 --
        //    {0xD7, "la"},// pos:183, col:16, row:10 --
        //    {0xD8, ":z"},// pos:184, col:17, row:10 --
        //    {0xD9, "Fr"},// pos:185, col:18, row:10 --
        //    {0xDA, "nt"},// pos:186, col:19, row:10 --
        //    {0xDB, "elng"},// pos:187, col:20, row:10 --
        //    {0xDC, "re"},// pos:188, col:21, row:10 --
        //    {0xDD, "S:"},// pos:189, col:1, row:10 --
        //    {0xDE, "so"},// pos:190, col:2, row:10 --
        //    {0xDF, "Ra"},// pos:191, col:3, row:10 --
        //    {0xE0, "nu"},// pos:192, col:4, row:10 --
        //    {0xE1, "ra"},// pos:193, col:5, row:10 --
        //    {0xE2, "®"},// pos:194, col:6, row:10 -- End of font texture
        //    //{0xE3, ""},// pos:195, col:0, row:0 --
        //    //{0xE4, ""},// pos:196, col:0, row:0 --
        //    //{0xE5, ""},// pos:197, col:0, row:0 --
        //    //{0xE6, ""},// pos:198, col:0, row:0 --
        //    //{0xE7, ""},// pos:199, col:0, row:0 --
        //    {0xE8, "in"},// pos:200, col:0, row:0 --
        //    {0xE9, "e "},// pos:201, col:0, row:0 --
        //    {0xEA, "ne"},// pos:202, col:0, row:0 --
        //    {0xEB, "to"},// pos:203, col:0, row:0 --
        //    {0xEC, "to"},// pos:204, col:0, row:0 --
        //    {0xED, "HP"},// pos:205, col:0, row:0 --
        //    {0xEE, "l "},// pos:206, col:0, row:0 --
        //    {0xEF, "ll"},// pos:207, col:0, row:0 --
        //    {0xF0, "GF"},// pos:208, col:0, row:0 --
        //    {0xF1, "nt"},// pos:209, col:0, row:0 --
        //    {0xF2, "il"},// pos:210, col:0, row:0 --
        //    {0xF3, "o "},// pos:211, col:0, row:0 --
        //    {0xF4, "ef"},// pos:212, col:0, row:0 --
        //    {0xF5, "on"},// pos:213, col:0, row:0 --
        //    {0xF6, " w"},// pos:214, col:0, row:0 --
        //    {0xF7, " r"},// pos:215, col:0, row:0 --
        //    {0xF8, "wi"},// pos:216, col:0, row:0 --
        //    {0xF9, "fi"},// pos:217, col:0, row:0 --
        //    //{0xFA, ""},// pos:218, col:0, row:0 --
        //    {0xFB, "s "},// pos:219, col:0, row:0 --
        //    {0xFC, "ar"},// pos:220, col:0, row:0 --
        //    {0xFD, ""},// pos:221, col:0, row:0 --
        //    {0xFE, " S"},// pos:222, col:0, row:0 --
        //    {0xFF, "ag"},// pos:223, col:0, row:0 --

        //    //{0x00, "\0"}, // I think \0 is a new string. if you read in a msg file it a array of strings each ending with \0
        //    //{0x02, "\n"}, // changed \n to signal draw text to make a new line
        //    //{0x03, ""},
        //    //{0x04, "" }, //Probably
        //    //{0x0E, "" }, //Probably
        //    //{0x20, " "},
        //    //{0x21, "0"},
        //    //{0x22, "1"},
        //    //{0x23, "2"},
        //    //{0x24, "3"},
        //    //{0x25, "4"},
        //    //{0x26, "5"},
        //    //{0x27, "6"},
        //    //{0x28, "7"},
        //    //{0x29, "8"},
        //    //{0x2A, "9"},
        //    //{0x2B, "%"},
        //    //{0x2C, "/"},
        //    //{0x2D, ":"},
        //    //{0x2E, "!"},
        //    //{0x2F, "?"},
        //    //{0x30, "…"},
        //    //{0x31, "+"},
        //    //{0x32, "-"},
        //    //{0x33, "SPECIAL CHARACTER TODO"},
        //    //{0x34, "*"},
        //    //{0x35, "&"},
        //    //{0x36, "SPECIAL CHARACTER TODO" },
        //    //{0x37, "SPECIAL CHARACTER TODO" },
        //    //{0x38, "("},
        //    //{0x39, ")"},
        //    //{0x3A, "SPECIAL CHARACTER TODO"},
        //    //{0x3B, "."},
        //    //{0x3C, ","},
        //    //{0x3D, "~"},
        //    //{0x3E, "SPECIAL CHARACTER TODO"},
        //    //{0x3F, "SPECIAL CHARACTER TODO"},
        //    //{0x40, "'"},
        //    //{0x41, "#"},
        //    //{0x42, "$"},
        //    //{0x43, "`"},
        //    //{0x44, "_"},
        //    //{0x45, "A"},
        //    //{0x46, "B"},
        //    //{0x47, "C"},
        //    //{0x48, "D"},
        //    //{0x49, "E"},
        //    //{0x4A, "F"},
        //    //{0x4B, "G"},
        //    //{0x4C, "H"},
        //    //{0x4D, "I"},
        //    //{0x4E, "J"},
        //    //{0x4F, "K"},
        //    //{0x50, "L"},
        //    //{0x51, "M"},
        //    //{0x52, "N"},
        //    //{0x53, "O"},
        //    //{0x54, "P"},
        //    //{0x55, "Q"},
        //    //{0x56, "R"},
        //    //{0x57, "S"},
        //    //{0x58, "T"},
        //    //{0x59, "U"},
        //    //{0x5A, "V"},
        //    //{0x5B, "W"},
        //    //{0x5C, "X"},
        //    //{0x5D, "Y"},
        //    //{0x5E, "Z"},
        //    //{0x5F, "a"},
        //    //{0x60, "b"},
        //    //{0x61, "c"},
        //    //{0x62, "d"},
        //    //{0x63, "e"},
        //    //{0x64, "f"},
        //    //{0x65, "g"},
        //    //{0x66, "h"},
        //    //{0x67, "i"},
        //    //{0x68, "j"},
        //    //{0x69, "k"},
        //    //{0x6A, "l"},
        //    //{0x6B, "m"},
        //    //{0x6C, "n"},
        //    //{0x6D, "o"},
        //    //{0x6E, "p"},
        //    //{0x6F, "q"},
        //    //{0x70, "r"},
        //    //{0x71, "s"},
        //    //{0x72, "t"},
        //    //{0x73, "u"},
        //    //{0x74, "v"},
        //    //{0x75, "w"},
        //    //{0x76, "x"},
        //    //{0x77, "y"},
        //    //{0x78, "z"},
        //    //{0x79, "Ł"},
        //    //{0x7C, "Ä"},
        //    //{0x88, "Ó"},
        //    //{0x8A, "Ö"},
        //    //{0x8E, "Ü"},
        //    //{0x90, "ß"},
        //    //{0x94, "ä"},
        //    //{0xA0, "ó"},
        //    //{0xA2, "ö"},
        //    //{0xA6, "ü"},
        //    //{0xA8, "Ⅷ"},
        //    //{0xA9, "["},
        //    //{0xAA, "]"},
        //    //{0xAB, "[SQUARE]"},
        //    //{0xAC, "@"},
        //    //{0xAD, "[SSQUARE]"},
        //    //{0xAE, "{"},
        //    //{0xAF, "}"},
        //    //{0xC6, "Ⅵ"},
        //    //{0xC7, "Ⅱ"},
        //    //{0xC9, "™"},
        //    //{0xCA, "<"},
        //    //{0xCB, ">"},
        //    //{0xE8, "in"},
        //    //{0xE9, "e "},
        //    //{0xEA, "ne"},
        //    //{0xEB, "to"},
        //    //{0xEC, "re"},
        //    //{0xED, "HP"},
        //    //{0xEE, "l "},
        //    //{0xEF, "ll"},
        //    //{0xF0, "GF"},
        //    //{0xF1, "nt"},
        //    //{0xF2, "il"},
        //    //{0xF3, "o "},
        //    //{0xF4, "ef"},
        //    //{0xF5, "on"},
        //    //{0xF6, " w"},
        //    //{0xF7, " r"},
        //    //{0xF8, "wi"},
        //    //{0xF9, "fi"},
        //    //{0xFB, "s "},
        //    //{0xFC, "ar"},
        //    //{0xFE, " S"},
        //    //{0xFF, "ag"}
        //};

        #endregion CharTable

        public enum ColorID
        {
            Dark_Gray, Grey, Yellow, Red, Green, Blue, Purple, White
        }

        public static Dictionary<ColorID, Color> ColorID2Color = new Dictionary<ColorID, Color>
        {
            { ColorID.Dark_Gray, new Color(41,49,41,255) },
            { ColorID.Grey, new Color(148,148,164,255) },
            { ColorID.Yellow, new Color(222,222,8,255) },
            { ColorID.Red, new Color(255,24,24,255) },
            { ColorID.Green, new Color(0,255,0,255) },
            { ColorID.Blue, new Color(106,180,238,255) },
            { ColorID.Purple, new Color(255,0,255,255) },
            { ColorID.White, Color.White }
        };

        /// <summary>
        /// Change colors of text following this.
        /// </summary>
        private static Dictionary<int, ColorID> ColorCode = new Dictionary<int, ColorID>()
        {
            {0x0625, ColorID.Blue },
            {0x0627, ColorID.White },
            {0x0624, ColorID.Green }
        };

        /// <summary>
        /// Placeholder for button images.
        /// </summary>
        private static Dictionary<int, string> Icons = new Dictionary<int, string>()
        {
            //on pc it puts a green letter for the keyboard key
            //two lefts and two rights doesn't make sense.

            //buttons
            {0x052F, "Left" },
            {0x052D, "Right" },
            {0x052C, "Left" },
            {0x052E, "Right" },
            {0x0526, "Okay" },
            {0x0524, "Cancel" },
            {0x053B, "Start" },
            {0x0522, "L1" },
            {0x0520, "L2" },
            {0x0521, "R2" },
            //other
            //commented out because these seem to be varible depending on where they are used.
            //{0x0541, "Junction Symbol" },
            //{0x0542, "Right Arrow" },
            //{ 0x55D,  "Fire" },
            //{ 0x55E,  "Ice" },
            //{ 0x55F,  "Thunder" },
            //{ 0x560,  "Earth" },
            //{ 0x561,  "Poison" },
            //{ 0x562,  "Wind" },
            //{ 0x563,  "Water" },
            //{ 0x564,  "Holy" },
            //{ 0x565,  "Death" },
            //{ 0x566,  "Poison" },
            //{ 0x567,  "Petrify" },
            //{ 0x568,  "Darkness" },
            //{ 0x569,  "Silence" },
            //{ 0x56A,  "Berserk" },
            //{ 0x56B,  "Zombie" },
            //{ 0x56C,  "Sleep" },
            //{ 0x56D,  "Slow" },
            //{ 0x56E,  "Stop" },
            //{ 0x56F,  "Curse" },
            //{ 0x570,  "Confuse" },
            //{ 0x571,  "Drain" },
            //{0x545, "Junction Ability" },
            //{0x546,   "Command Ability" },
            //{0x548,   "Character Ability" },
            //{0x549,   "Party Ability" },
            //{0x54A,   "GF Ability" },
            //{0x54B,   "Menu Ability" },
        };

        /// <summary>
        /// Place holders for names that can be customized.
        /// </summary>
        private static Dictionary<int, string> Names = new Dictionary<int, string>()
        {
            {0x0330, "Squall" },
            {0x0334, "Riona" },
            {0x0340, "Angelio" },
            {0x333, "Character" } // could be GF, could be character, unsure.
        };

        /// <summary>
        /// Place holders for various things. The 0A values change depending on what section/string
        /// you are in. 0B values can be 2 or 3 bytes total.
        /// </summary>
        private static Dictionary<int, string> Special = new Dictionary<int, string>()
        {
            //{0xA29, "Ability" }, // that can be learned.
            //{0xA27, "Character Name"}, //could be wrong
            //{0xA24, "GF Name" },
            //{0xA25, "Ability" },
            //{0xA22, "Amount" }, //current level or change unsure.
            //{0xA28, "Stat" },
            //{0xA26, "Spell or Rank" }, //Selected spell or SeeD rank
            //{0xA20, "Value" }, //Card Level or SeeD test scores
            //{0xABB, "Ignore require: GF Medicine: Ribbon" }, //might be more general like all GF medicine only abilites.
            //{0xA97, "Ignore require: Tonberry ability unlock" }, // could be you need to unlock or just have the gf
            //{0xA96, "Ignore require: Cactuar ability unlock" }, // could be you need to unlock or just have the gf
            //{0xABF, "Ignore require: Defeat Omega Weapon" },
            //{0xA3F, "End Ignore" },
            {0xB20, "Option-0xB20" }, // could be wrong, //following byte is like an id of selection.
            {0xB21, "Option-0xB21" }, // could be wrong
            {0xB22, "Option-0xB22" }, // could be wrong
            {0xB23, "Option-0xB23" }, // could be wrong
            //0xC values are spells.
        };

        private static Dictionary<int, string> Spell = new Dictionary<int, string>()
        {
            //Spell
            //http://forums.qhimm.com/index.php?topic=11137.msg166280#msg166280
            {0xC00,"Empty"},
            {0xC01,"Fire"},
            {0xC02,"Fira"},
            {0xC03,"Firaga"},
            {0xC04,"Blizzard"},
            {0xC05,"Blizzara"},
            {0xC06,"Blizzaga"},
            {0xC07,"Thunder"},
            {0xC08,"Thundara"},
            {0xC09,"Thundaga"},
            {0xC0A,"Water"},
            {0xC0B,"Aero"},
            {0xC0C,"Bio"},
            {0xC0D,"Demi"},
            {0xC0E,"Holy"},
            {0xC0F,"Flare"},
            {0xC10,"Meteor"},
            {0xC11,"Quake"},
            {0xC12,"Tornado"},
            {0xC13,"Ultima"},
            {0xC14,"Apocalypse"},
            {0xC15,"Cure"},
            {0xC16,"Cura"},
            {0xC17,"Curaga"},
            {0xC18,"Life"},
            {0xC19,"Full-Life"},
            {0xC1A,"Regan"},
            {0xC1B,"Esuna"},
            {0xC1C,"Dispel"},
            {0xC1D,"Protect"},
            {0xC1E,"Shell"},
            {0xC1F,"Reflect"},
            {0xC20,"Aura"},
            {0xC21,"Double"},
            {0xC22,"Triple"},
            {0xC23,"Haste"},
            {0xC24,"Slow"},
            {0xC25,"Stop"},
            {0xC26,"Blind"},
            {0xC27,"Confuse"},
            {0xC28,"Sleep"},
            {0xC29,"Silence"},
            {0xC2A,"Break"},
            {0xC2B,"Death"},
            {0xC2C,"Drain"},
            {0xC2D,"Pain"},
            {0xC2E,"Berserk"},
            {0xC2F,"Float"},
            {0xC30,"Zombie"},
            {0xC31,"Meltdown"},
            {0xC32,"Scan"},
            {0xC33,"Full-Cure"},
            {0xC34,"Wall"},
            {0xC35,"Rapture"},
            {0xC36,"Percent"},
            {0xC37,"Catastrophe"},
            {0xC38,"The End"},
            //GF
            {0xC60,"Quezacotl" },
            {0xC61,"Shiva" },
            {0xC62,"Ifrit" },
            {0xC63,"Siren" },
            {0xC64,"Brothers" },
            {0xC65,"Diablos" },
            {0xC66,"Carbuncle" },
            {0xC67,"Leviathan" },
            {0xC68,"Pandemona" },
            {0xC69,"Cerberus" },
            {0xC6A,"Alexander" },
            {0xC6B,"Doomtrain" },
            {0xC6C,"Bahamut" },
            {0xC6D,"Cactuar" },
            {0xC6E,"Tonberry" },
            {0xC6F,"Eden" }
        };

        public Font() => LoadFonts();

        public void LoadFonts()
        {
            ArchiveWorker aw = new ArchiveWorker(Memory.Archives.A_MENU);
            string sysfntTdwFilepath = aw.GetListOfFiles().First(x => x.ToLower().Contains("sysfnt.tdw"));
            string sysfntFilepath = aw.GetListOfFiles().First(x => x.ToLower().Contains("sysfnt.tex"));
            TEX tex = new TEX(ArchiveWorker.GetBinaryFile(Memory.Archives.A_MENU, sysfntFilepath));
            sysfnt = tex.GetTexture((int)ColorID.White);
            sysfntbig = new TextureHandler("sysfld{0:00}.tex", tex, 2, 1, (int)ColorID.White);

            ReadTdw(ArchiveWorker.GetBinaryFile(Memory.Archives.A_MENU, sysfntTdwFilepath));
        }

        public void ReadTdw(byte[] Tdw)
        {
            uint widthPointer = BitConverter.ToUInt32(Tdw, 0);
            uint dataPointer = BitConverter.ToUInt32(Tdw, 4);

            getWidths(Tdw, widthPointer, dataPointer - widthPointer);
            TIM2 tim = new TIM2(Tdw, dataPointer);
            menuFont = new Texture2D(Memory.graphics.GraphicsDevice, tim.GetWidth, tim.GetHeight);
            menuFont.SetData(tim.CreateImageBuffer(tim.GetClutColors(ColorID.White)));
        }

        public void getWidths(byte[] Tdw, uint offset, uint length)
        {
            using (MemoryStream os = new MemoryStream((int)length * 2))
            using (BinaryWriter bw = new BinaryWriter(os))
            using (MemoryStream ms = new MemoryStream(Tdw))
            using (BinaryReader br = new BinaryReader(ms))
            {
                //bw.Write((byte)10);//width of space
                ms.Seek(offset, SeekOrigin.Begin);
                while (ms.Position < offset + length)
                {
                    byte b = br.ReadByte();
                    byte low = (byte)(b & 0x0F);
                    byte high = (byte)(b >> 4);
                    bw.Write(low);
                    bw.Write(high);
                }
                charWidths = os.ToArray();
            }
        }

        private byte[] charWidths;

        public enum Type
        {
            sysFntBig,
            sysfnt,
            menuFont,
        }

        public Rectangle RenderBasicText(FF8String buffer, Vector2 pos, Vector2 zoom, Type whichFont = 0, float Fade = 1.0f, int lineSpacing = 0, bool skipdraw = false, ColorID color = ColorID.White)
        {
            if (buffer == null) return new Rectangle();
            Rectangle ret = new Rectangle(pos.RoundedPoint(), new Point(0));
            Point real = pos.RoundedPoint();
            int charCountWidth = 21;
            int charSize = 12; //pixelhandler does the 2x scaling on the fly.
            Point size = (new Vector2(0, charSize) * zoom).RoundedPoint();
            int width;
            foreach (byte c in buffer)
            {
                if (c == 0) continue;
                int deltaChar = (c - 32);
                if (deltaChar >= 0 && deltaChar < charWidths.Length)
                {
                    width = charWidths[deltaChar];
                    size.X = (int)(charWidths[deltaChar] * zoom.X);
                }
                else
                {
                    width = charSize;
                    size.X = (int)(charSize * zoom.X);
                }
                Point curSize = size;
                int verticalPosition = deltaChar / charCountWidth;
                //i.e. 1280 is 100%, 640 is 50% and therefore 2560 is 200% which means multiply by 0.5f or 2.0f
                if (c == 0x02)// \n
                {
                    real.X = (int)pos.X;
                    real.Y += size.Y + lineSpacing;
                    continue;
                }
                Rectangle destRect = new Rectangle(real, size);
                // if you use Memory.SpriteBatchStartAlpha(SamplerState.PointClamp); you won't need
                // to trim last pixel. but it doesn't look good on low res fonts.
                if (!skipdraw)
                {
                    Rectangle sourceRect = new Rectangle((deltaChar - (verticalPosition * charCountWidth)) * charSize,
                        verticalPosition * charSize,
                        width,
                        charSize);

                    switch (whichFont)
                    {
                        case Type.menuFont:
                        case Type.sysfnt:
                            //trim pixels to remove texture filtering artifacts.
                            sourceRect.Width -= 1;
                            sourceRect.Height -= 1;
                            Memory.spriteBatch.Draw(whichFont == Type.menuFont ? menuFont : sysfnt,
                                destRect,
                                sourceRect,
                            ColorID2Color[color] * Fade);
                            break;

                        case Type.sysFntBig:
                            if (!sysfntbig.Modded)
                            {
                                Rectangle ShadowdestRect = new Rectangle(destRect.Location, destRect.Size);
                                ShadowdestRect.Offset(zoom);
                                sysfntbig.Draw(ShadowdestRect, sourceRect, Color.Black * Fade * .5f);
                            }
                            sysfntbig.Draw(destRect, sourceRect, ColorID2Color[color] * Fade);
                            break;
                    }
                }
                real.X += size.X;
                int curWidth = real.X - (int)pos.X;
                if (curWidth > ret.Width)
                    ret.Width = curWidth;
            }
            ret.Height = size.Y + (real.Y - (int)pos.Y);
            return ret;
        }

        public Rectangle RenderBasicText(FF8String buffer, Point pos, Vector2 zoom, Type whichFont = 0, float Fade = 1.0f, int lineSpacing = 0, bool skipdraw = false, ColorID color = ColorID.White)
            => RenderBasicText(buffer, pos.ToVector2(), zoom, whichFont, Fade, lineSpacing, skipdraw, color);

        public Rectangle RenderBasicText(FF8String buffer, int x, int y, float zoomWidth = 2.545455f, float zoomHeight = 3.0375f, Type whichFont = 0, float Fade = 1.0f, int lineSpacing = 0, bool skipdraw = false, ColorID color = ColorID.White)
            => RenderBasicText(buffer, new Vector2(x, y), new Vector2(zoomWidth, zoomHeight), whichFont, Fade, lineSpacing, skipdraw, color);

        /// <summary>
        /// Converts clean string into dirty string for drawing.
        /// </summary>
        /// <param name="s">Clean String</param>
        /// <returns>Dirty String</returns>
        /// <remarks>
        /// dirty, do not use for anything else than translating for your own purpouses. I'm just
        /// lazy Anything from 0-127 should be able to convert back and forth. But anything 128-256
        /// may get messed up as c# stores it's strings as 16-bit unicode
        /// </remarks>
        [Obsolete("use 'Memory.DirtyEncoding.GetBytes(s)' or 'new FF8String(s)'")]
        public static byte[] CipherDirty(string s) => Memory.DirtyEncoding.GetBytes(s);

        //{
        //    using (MemoryStream ms = new MemoryStream(s.Length))
        //    {
        //        foreach (char n in s)
        //        {
        //            // might need to change this to let the 0x02 pass and make the render function detect
        //            // the 0x02 instead of \n
        //            //if (n == '\n') { str += n; continue; }
        //            foreach (KeyValuePair<byte, string> kvp in chartable)
        //                if (kvp.Value.Length == 1)
        //                    if (kvp.Value[0] == n)
        //                       ms.WriteByte(kvp.Key);
        //        }
        //        return ms.ToArray();
        //    }
        //}

        /// <summary>
        /// For string dump file. Not ment to be used to decode FF8Strings.
        /// </summary>
        /// <param name="s">Dirty String</param>
        /// <returns>Clean UTF8 String in byte[] form with some XML</returns>
        public static byte[] DumpDirtyString(byte[] s)
        {
            if (s != null)
                using (MemoryStream os = new MemoryStream(s.Length))
                using (BinaryWriter bw = new BinaryWriter(os))
                using (MemoryStream ms = new MemoryStream(s))
                using (BinaryReader br = new BinaryReader(ms))
                {
                    while (ms.Position < ms.Length)
                    {
                        byte b = br.ReadByte();

                        if (DirtyEncoding.BytetoStr.ContainsKey(b))
                        {
                            byte[] c = Encoding.UTF8.GetBytes(DirtyEncoding.BytetoStr[b]);
                            os.Write(c, 0, c.Length);
                        }
                        else if (DirtyEncoding.BytetoChar.ContainsKey(b))
                        {
                            byte[] c = Encoding.UTF8.GetBytes(DirtyEncoding.BytetoChar[b].ToString());
                            os.Write(c, 0, c.Length);
                        }
                        else if (ms.Position < ms.Length)
                        {
                            byte c = br.ReadByte();
                            ushort i = BitConverter.ToUInt16(new byte[] { c, b }, 0);
                            switch (b)
                            {
                                case 0x06:
                                    if (ColorCode.ContainsKey(i))
                                        bw.Write(Encoding.UTF8.GetBytes($"<Color: \"{ColorCode[i]}\">"));
                                    else
                                        bw.Write(Encoding.UTF8.GetBytes($"<Color: {string.Format("0x{0:X2}", i)}>"));
                                    break;

                                case 0x05:
                                    if (Icons.ContainsKey(i))
                                        bw.Write(Encoding.UTF8.GetBytes($"<Icon_Button: \"{Icons[i]}\">"));
                                    else
                                        bw.Write(Encoding.UTF8.GetBytes($"<Icon_Button: {string.Format("0x{0:X2}", i)}>"));
                                    break;

                                case 0x03:
                                    if (Names.ContainsKey(i))
                                        bw.Write(Encoding.UTF8.GetBytes($"<Name: \"{Names[i]}\">"));
                                    else
                                        bw.Write(Encoding.UTF8.GetBytes($"<Name: {string.Format("0x{0:X2}", i)}>"));
                                    break;

                                case 0x0C:
                                    if (Spell.ContainsKey(i))
                                        bw.Write(Encoding.UTF8.GetBytes($"<Spell_GF: \"{Spell[i]}\">"));
                                    else
                                        bw.Write(Encoding.UTF8.GetBytes($"<Spell_GF: {string.Format("0x{0:X2}", i)}>"));
                                    break;

                                case 0x0A:
                                case 0x0B://0x0B can be 2 or 3 bytes only grabbing 2 so might have extra rando character near
                                    if (Special.ContainsKey(i))
                                        bw.Write(Encoding.UTF8.GetBytes($"<Special: \"{Special[i]}\">"));
                                    else
                                        bw.Write(Encoding.UTF8.GetBytes($"<Special: {string.Format("0x{0:X2}", i)}>"));
                                    break;

                                case 0xC4:
                                    ms.Seek(-1, SeekOrigin.Current);
                                    bw.Write(Encoding.UTF8.GetBytes(string.Format("0x{0:X2}", (int)b)));
                                    break;

                                default:
                                    bw.Write(Encoding.UTF8.GetBytes(string.Format("0x{0:X2}", i)));
                                    break;
                            }
                        }
                        else
                        {
                            bw.Write(b);
                        }
                    }
                    if (os.Length > 0)
                        return os.ToArray();
                }
            return null;
        }

        /*
         * myst6re code
         *
         *
         *
         for(int i=0 ; i<size ; ++i) {
		caract = (quint8)ff8Text.at(i);
		if(caract==0) break;
		switch(caract) {
		case 0x1: // New Page
			if(height>maxH)	maxH = height;
			if(width>maxW)	maxW = width;
			width = 15;
			height = 28;
			pagesPos.append(i+1);
			break;

		case 0x2: // \n
			if(width>maxW)	maxW = width;
			++line;
			width = (ask_first<=line && ask_last>=line ? 79 : 15);//32+15+32 (padding for arrow) or 15
			height += 16;
			break;

		case 0x3: // Character names
			caract = (quint8)ff8Text.at(++i);
			if(caract>=0x30 && caract<=0x3a)
				width += namesWidth[caract-0x30];
			else if(caract==0x40)
				width += namesWidth[11];
			else if(caract==0x50)
				width += namesWidth[12];
			else if(caract==0x60)
				width += namesWidth[13];
			break;

		case 0x4: // Vars
			caract = (quint8)ff8Text.at(++i);
			if((caract>=0x20 && caract<=0x27) || (caract>=0x30 && caract<=0x37))
				width += font->charWidth(0, 1);// 0
			else if(caract>=0x40 && caract<=0x47)
				width += font->charWidth(0, 1)*8;// 00000000
			break;

		case 0x5: // Icons
			caract = (quint8)ff8Text.at(++i)-0x20;
            if(caract<96)
				width += iconWidth[caract]+iconPadding[caract];
			break;

		case 0xe: // Locations
			caract = (quint8)ff8Text.at(++i);
			if(caract>=0x20 && caract<=0x27)
				width += locationsWidth[caract-0x20];
			break;

		case 0x19: // Jap 1
			if(jp) {
				caract = (quint8)ff8Text.at(++i);
				if(caract>=0x20)
					width += font->charWidth(1, caract-0x20);
			}
			break;

		case 0x1a: // Jap 2
			if(jp) {
				caract = (quint8)ff8Text.at(++i);
				if(caract>=0x20)
					width += font->charWidth(2, caract-0x20);
			}
			break;

		case 0x1b: // Jap 3
			if(jp) {
				caract = (quint8)ff8Text.at(++i);
				if(caract>=0x20)
					width += font->charWidth(3, caract-0x20);
			}
			break;

		case 0x1c: // Jap 4
			if(tdwFile) {
				caract = (quint8)ff8Text.at(++i);
				if(caract>=0x20)
					width += tdwFile->charWidth(0, caract-0x20);
			}
			break;

		default:
			if(caract<0x20)
				++i;
			else if(jp) {
				width += font->charWidth(0, caract-0x20);
			} else {
				if(caract<232)
					width += font->charWidth(0, caract-0x20);
				else if(caract>=232)
					width += font->charWidth(0, (quint8)optimisedDuo[caract-232][0]) + font->charWidth(0, (quint8)optimisedDuo[caract-232][1]);
			}
			break;
		}
	}

	if(height>maxH)	maxH = height;
	if(width>maxW)	maxW = width;
	if(maxW>322)	maxW = 322;
	if(maxH>226)	maxH = 226;

update();
        */
    }
}