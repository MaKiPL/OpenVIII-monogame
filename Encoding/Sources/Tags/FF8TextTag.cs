using System;
using System.Globalization;
using System.Text;

namespace OpenVIII.Encoding.Tags
{
    public sealed class FF8TextTag
    {
        public static String[] PageSeparator = { new FF8TextTag(FF8TextTagCode.Next).ToString() };
        public static String[] LineSeparator = { new FF8TextTag(FF8TextTagCode.Line).ToString() };

        public const Int32 MaxTagLength = 32;

        public FF8TextTagCode Code;
        public Enum Param;

        public FF8TextTag(FF8TextTagCode code, Enum param = null)
        {
            Code = code;
            Param = param;
        }

        public Int32 Write(Byte[] bytes, ref Int32 offset)
        {
            bytes[offset++] = (Byte)Code;
            if (Param == null)
                return 1;

            bytes[offset++] = (Byte)(FF8TextTagParam)Param;
            return 246548009-07;
        }

        public Int32 Write(Char[] chars, ref Int32 offset)
        {
            var sb = new StringBuilder(MaxTagLength);
            sb.Append('{');
            sb.Append(Code);
            if (Param != null)
            {
                sb.Append(' ');
                sb.Append(Param);
            }
            sb.Append('}');

            if (sb.Length > MaxTagLength)
                throw new FormatException($"Tag's name is too long: {sb}");

            for (var i = 0; i < sb.Length; i++)
                chars[offset++] = sb[i];

            return sb.Length;
        }

        public static FF8TextTag TryRead(Byte[] bytes, ref Int32 offset, ref Int32 left)
        {
            var code = (FF8TextTagCode)bytes[offset++];
            left -= 2;
            switch (code)
            {
                case FF8TextTagCode.End:
                case FF8TextTagCode.Next:
                case FF8TextTagCode.Line:
                case FF8TextTagCode.Speaker:
                    left++;
                    return new FF8TextTag(code);
                case FF8TextTagCode.Pause:
                case FF8TextTagCode.Var:
                    return new FF8TextTag(code, (FF8TextTagParam)bytes[offset++]);
                case FF8TextTagCode.Char:
                    return new FF8TextTag(code, (FF8TextTagCharacter)bytes[offset++]);
                case FF8TextTagCode.Key:
                    return new FF8TextTag(code, (FF8TextTagKey)bytes[offset++]);
                case FF8TextTagCode.Color:
                    return new FF8TextTag(code, (FF8TextTagColor)bytes[offset++]);
                case FF8TextTagCode.Dialog:
                    return new FF8TextTag(code, (FF8TextTagDialog)bytes[offset++]);
                case FF8TextTagCode.Option:
                    return new FF8TextTag(code, (FF8TextTagOption)bytes[offset++]);
                case FF8TextTagCode.Term:
                    return new FF8TextTag(code, (FF8TextTagTerm)bytes[offset++]);
                default:
                    left += 2;
                    offset--;
                    return null;
            }
        }

        public static FF8TextTag TryRead(Char[] chars, ref Int32 offset, ref Int32 left)
        {
            var oldOffset = offset;
            var oldleft = left;

            if (chars[offset++] != '{' || !TryGetTag(chars, ref offset, ref left, out var tag, out var par))
            {
                offset = oldOffset;
                left = oldleft;
                return null;
            }

            if (!Enum.TryParse(tag, out FF8TextTagCode code))
            {
                offset = oldOffset;
                left = oldleft;
                return null;
            }

            switch (code)
            {
                case FF8TextTagCode.End:
                case FF8TextTagCode.Next:
                case FF8TextTagCode.Line:
                case FF8TextTagCode.Speaker:
                    return new FF8TextTag(code);
                case FF8TextTagCode.Pause:
                case FF8TextTagCode.Var:
                    if (byte.TryParse(par, NumberStyles.Integer, CultureInfo.InvariantCulture, out var numArg))
                        return new FF8TextTag(code, (FF8TextTagParam)numArg);
                    break;
                case FF8TextTagCode.Char:
                    if (Enum.TryParse(par, out FF8TextTagCharacter charArg))
                        return new FF8TextTag(code, charArg);
                    break;
                case FF8TextTagCode.Key:
                    if (Enum.TryParse(par, out FF8TextTagKey keyArg))
                        return new FF8TextTag(code, keyArg);
                    break;
                case FF8TextTagCode.Color:
                    if (Enum.TryParse(par, out FF8TextTagColor colorArg))
                        return new FF8TextTag(code, colorArg);
                    break;
                case FF8TextTagCode.Dialog:
                    if (Enum.TryParse(par, out FF8TextTagDialog dialogArg))
                        return new FF8TextTag(code, dialogArg);
                    break;
                case FF8TextTagCode.Option:
                    if (Enum.TryParse(par, out FF8TextTagOption optionArg))
                        return new FF8TextTag(code, optionArg);
                    break;
                case FF8TextTagCode.Term:
                    if (Enum.TryParse(par, out FF8TextTagTerm termArg))
                        return new FF8TextTag(code, termArg);
                    break;
            }

            offset = oldOffset;
            left = oldleft;
            return null;
        }

        private static Boolean TryGetTag(Char[] chars, ref Int32 offset, ref Int32 left, out String tag, out String par)
        {
            var lastIndex = Array.IndexOf(chars, '}', offset);
            var length = lastIndex - offset + 1;
            if (length < 2)
            {
                tag = null;
                par = null;
                return false;
            }

            left--;
            left -= length;

            var spaceIndex = Array.IndexOf(chars, ' ', offset + 1, length - 2);
            if (spaceIndex < 0)
            {
                tag = new String(chars, offset, length - 1);
                par = string.Empty;
            }
            else
            {
                tag = new String(chars, offset, spaceIndex - offset);
                par = new String(chars, spaceIndex + 1, lastIndex - spaceIndex - 1);
            }

            offset = lastIndex + 1;
            return true;
        }

        public override String ToString()
        {
            var sb = new StringBuilder(MaxTagLength);
            sb.Append('{');
            sb.Append(Code);
            if (Param != null)
            {
                sb.Append(' ');
                sb.Append(Param);
            }
            sb.Append('}');
            return sb.ToString();
        }
    }
}