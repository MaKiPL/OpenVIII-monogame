using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FF8
{
    internal class Font
    {
        private Texture2D sysfnt; //21 characters long; char is always 12x12
        private Texture2D sysfnt00;
        private Texture2D menuFont;

        #region CharTable
        private static readonly Dictionary<byte, string> chartable = new Dictionary<byte, string>
        {
            {0x00, "t"},
            {0x02, "\n"}, // changed \n to signal draw text to make a new line
            {0x03, ""},
            {0x04, "" }, //Probably
            {0x0E, "" }, //Probably
            {0x20, " "},
            {0x21, "0"},
            {0x22, "1"},
            {0x23, "2"},
            {0x24, "3"},
            {0x25, "4"},
            {0x26, "5"},
            {0x27, "6"},
            {0x28, "7"},
            {0x29, "8"},
            {0x2A, "9"},
            {0x2B, "%"},
            {0x2C, "/"},
            {0x2D, ":"},
            {0x2E, "!"},
            {0x2F, "?"},
            {0x30, "…"},
            {0x31, "+"},
            {0x32, "-"},
            {0x33, "SPECIAL CHARACTER TODO"},
            {0x34, "*"},
            {0x35, "&"},
            {0x36, "SPECIAL CHARACTER TODO" },
            {0x37, "SPECIAL CHARACTER TODO" },
            {0x38, "("},
            {0x39, ")"},
            {0x3A, "SPECIAL CHARACTER TODO"},
            {0x3B, "."},
            {0x3C, ","},
            {0x3D, "~"},
            {0x3E, "SPECIAL CHARACTER TODO"},
            {0x3F, "SPECIAL CHARACTER TODO"},
            {0x40, "'"},
            {0x41, "#"},
            {0x42, "$"},
            {0x43, "`"},
            {0x44, "_"},
            {0x45, "A"},
            {0x46, "B"},
            {0x47, "C"},
            {0x48, "D"},
            {0x49, "E"},
            {0x4A, "F"},
            {0x4B, "G"},
            {0x4C, "H"},
            {0x4D, "I"},
            {0x4E, "J"},
            {0x4F, "K"},
            {0x50, "L"},
            {0x51, "M"},
            {0x52, "N"},
            {0x53, "O"},
            {0x54, "P"},
            {0x55, "Q"},
            {0x56, "R"},
            {0x57, "S"},
            {0x58, "T"},
            {0x59, "U"},
            {0x5A, "V"},
            {0x5B, "W"},
            {0x5C, "X"},
            {0x5D, "Y"},
            {0x5E, "Z"},
            {0x5F, "a"},
            {0x60, "b"},
            {0x61, "c"},
            {0x62, "d"},
            {0x63, "e"},
            {0x64, "f"},
            {0x65, "g"},
            {0x66, "h"},
            {0x67, "i"},
            {0x68, "j"},
            {0x69, "k"},
            {0x6A, "l"},
            {0x6B, "m"},
            {0x6C, "n"},
            {0x6D, "o"},
            {0x6E, "p"},
            {0x6F, "q"},
            {0x70, "r"},
            {0x71, "s"},
            {0x72, "t"},
            {0x73, "u"},
            {0x74, "v"},
            {0x75, "w"},
            {0x76, "x"},
            {0x77, "y"},
            {0x78, "z"},
            {0x79, "Ł"},
            {0x7C, "Ä"},
            {0x88, "Ó"},
            {0x8A, "Ö"},
            {0x8E, "Ü"},
            {0x90, "ß"},
            {0x94, "ä"},
            {0xA0, "ó"},
            {0xA2, "ö"},
            {0xA6, "ü"},
            {0xA8, "Ⅷ"},
            {0xA9, "["},
            {0xAA, "]"},
            {0xAB, "[SQUARE]"},
            {0xAC, "@"},
            {0xAD, "[SSQUARE]"},
            {0xAE, "{"},
            {0xAF, "}"},
            {0xC6, "Ⅵ"},
            {0xC7, "Ⅱ"},
            {0xC9, "™"},
            {0xCA, "<"},
            {0xCB, ">"},
            {0xE8, "in"},
            {0xE9, "e "},
            {0xEA, "ne"},
            {0xEB, "to"},
            {0xEC, "re"},
            {0xED, "HP"},
            {0xEE, "l "},
            {0xEF, "ll"},
            {0xF0, "GF"},
            {0xF1, "nt"},
            {0xF2, "il"},
            {0xF3, "o "},
            {0xF4, "ef"},
            {0xF5, "on"},
            {0xF6, " w"},
            {0xF7, " r"},
            {0xF8, "wi"},
            {0xF9, "fi"},
            {0xFB, "s "},
            {0xFC, "ar"},
            {0xFE, " S"},
            {0xFF, "ag"}
        };
        #endregion

        public Font() => LoadFonts();

        internal void LoadFonts()
        {
            ArchiveWorker aw = new ArchiveWorker(Memory.Archives.A_MENU);
            string sysfntTdwFilepath = aw.GetListOfFiles().First(x => x.ToLower().Contains("sysfnt.tdw"));
            string sysfntFilepath = aw.GetListOfFiles().First(x => x.ToLower().Contains("sysfnt.tex"));
            string sysfnt00Filepath = aw.GetListOfFiles().First(x => x.ToLower().Contains("sysfnt00.tex"));
            TEX tex = new TEX(ArchiveWorker.GetBinaryFile(Memory.Archives.A_MENU, sysfntFilepath));
            sysfnt = tex.GetTexture();
            tex = new TEX(ArchiveWorker.GetBinaryFile(Memory.Archives.A_MENU, sysfnt00Filepath));
            sysfnt00 = tex.GetTexture();

            ReadTdw(ArchiveWorker.GetBinaryFile(Memory.Archives.A_MENU, sysfntTdwFilepath));
        }

        internal void ReadTdw(byte[] Tdw)
        {
            int widthPointer = BitConverter.ToInt32(Tdw, 0);
            int dataPointer = BitConverter.ToInt32(Tdw, 4);
            TIM2 tim = new TIM2(Tdw, (uint)dataPointer);
            menuFont = new Texture2D(Memory.graphics.GraphicsDevice, tim.GetWidth, tim.GetHeight);
            menuFont.SetData(tim.CreateImageBuffer(tim.GetClutColors(7)));
        }


        public Rectangle CalcBasicTextArea(string buffer, Vector2 pos, Vector2 zoom, int whichFont = 0, int isMenu = 0, float Fade = 1.0f) => CalcBasicTextArea(buffer, (int)pos.X, (int)pos.Y, zoom.X, zoom.Y);
        public Rectangle CalcBasicTextArea(string buffer, Point pos, Vector2 zoom, int whichFont = 0, int isMenu = 0, float Fade = 1.0f) => CalcBasicTextArea(buffer, pos.X, pos.Y, zoom.X, zoom.Y);
        public Rectangle CalcBasicTextArea(string buffer, int x, int y, float zoomWidth = 1f, float zoomHeight = 1f, int whichFont = 0)
        {
            Rectangle ret = new Rectangle(x, y, 0, 0);
            Point real = new Point(x, y);
            int charCountWidth = whichFont == 0 ? 21 : 10;
            int charSize = whichFont == 0 ? 12 : 24;
            Vector2 zoom = new Vector2(zoomWidth, zoomHeight);
            Point size = (new Vector2(charSize, charSize) * zoom * Memory.Scale()).ToPoint();
            foreach (char c in buffer)
            {
                if (c == '\n')
                {
                    real.X = x;
                    real.Y += size.Y;
                    continue;
                }
                int verticalPosition = (char)(c - 32) / charCountWidth;
                //i.e. 1280 is 100%, 640 is 50% and therefore 2560 is 200% which means multiply by 0.5f or 2.0f
                real.X += size.X;
                int curWidth = real.X - x;
                if (curWidth > ret.Width)
                    ret.Width = curWidth;
            }
            ret.Height = size.Y + (real.Y - y);
            return ret;
        }
        public Rectangle RenderBasicText(string buffer, Vector2 pos, Vector2 zoom, int whichFont = 0, int isMenu = 0, float Fade = 1.0f) => RenderBasicText(buffer, (int) pos.X, (int) pos.Y, zoom.X, zoom.Y, whichFont, isMenu, Fade);
        public Rectangle RenderBasicText(string buffer, Point pos, Vector2 zoom, int whichFont = 0, int isMenu = 0, float Fade = 1.0f) => RenderBasicText(buffer, pos.X, pos.Y, zoom.X, zoom.Y, whichFont, isMenu, Fade);
        public Rectangle RenderBasicText(string buffer, int x, int y, float zoomWidth = 1f, float zoomHeight = 1f, int whichFont = 0, int isMenu = 0, float Fade = 1.0f)
        {
            Rectangle ret = new Rectangle(x, y, 0, 0);
            Point real = new Point(x, y);
            int charCountWidth = whichFont == 0 ? 21 : 10;
            int charSize = whichFont == 0 ? 12 : 24;
            Vector2 zoom = new Vector2(zoomWidth, zoomHeight);
            Point size = (new Vector2(charSize,charSize)*zoom*Memory.Scale()).ToPoint();
            foreach (char c in buffer)
            {

                char deltaChar = (char)(c - 32);
                int verticalPosition = deltaChar / charCountWidth;
                //i.e. 1280 is 100%, 640 is 50% and therefore 2560 is 200% which means multiply by 0.5f or 2.0f
                if (c == '\n')
                {
                    real.X = x;
                    real.Y += size.Y;
                    continue;
                }
                Rectangle destRect = new Rectangle(real,size);

                Rectangle sourceRect = new Rectangle((deltaChar - (verticalPosition * charCountWidth)) * charSize,
                    verticalPosition * charSize,
                    charSize - 1,
                    charSize - 1);


                Memory.spriteBatch.Draw(isMenu == 1 ? menuFont : whichFont == 0 ? sysfnt : sysfnt00,
                    destRect,
                    sourceRect,
                Color.White * Fade);

                real.X += size.X;
                int curWidth = real.X - x;
                if (curWidth > ret.Width)
                    ret.Width = curWidth;
            }
            ret.Height = size.Y + (real.Y - y);
            return ret;
        }

        //dirty, do not use for anything else than translating for your own purpouses. I'm just lazy
        public static string CipherDirty(string s)
        {
            string str = "";
            foreach (char n in s)
            {
                if (n == '\n') { str += n; continue; }
                foreach (KeyValuePair<byte, string> kvp in chartable)
                    if (kvp.Value.Length == 1)
                        if (kvp.Value[0] == n)
                            str += (char)(kvp.Key);
            }
            return str.Replace("\0", "");
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
