using System;

namespace OpenVIII.Encoding.Tags
{
    public sealed class FF8TextComment
    {
        private static readonly string[] LineCommentEnd = {"\r\n", "\n", "{Line}"};
        private static readonly string[] BlockCommentEnd = {"*/"};

        public enum CommentType
        {
            Line = '/',
            Block = '*'
        }

        public readonly CommentType Type;
        public readonly string Value;

        private FF8TextComment(CommentType commentType, string value)
        {
            Type = commentType;
            Value = value;
        }

        public static FF8TextComment TryRead(char[] chars, ref int offset, ref int left)
        {
            if (left < 2 || chars[offset] != '/')
                return null;

            var commentType = (CommentType)chars[offset + 1];
            if (commentType != CommentType.Line && commentType != CommentType.Block)
                return null;

            string value;
            var index = IndexOfAny(chars, offset + 2, left - 2, out var finded, commentType == CommentType.Line ? LineCommentEnd : BlockCommentEnd);

            if (index < 0)
            {
                value = new string(chars, offset + 2, left - 2);
                offset = offset + left;
                left = 0;
            }
            else
            {
                if (commentType == CommentType.Line)
                {
                    if (offset != 0 && IndexOfAny(chars, offset, -6, out var prev, LineCommentEnd) < 0)
                        finded = string.Empty;
                }

                var length = index - offset;
                value = new string(chars, offset + 2, length - 2);
                left -= length + finded.Length;
                offset = index + finded.Length;
            }

            return new FF8TextComment(commentType, value);
        }

        private static int IndexOfAny(char[] chars, int offset, int left, out string finded, params string[] subStrings)
        {
            var counters = new int[subStrings.Length];

            if (left > 0)
            {
                for (var i = offset; i < chars.Length && left > 0; i++, left--)
                {
                    for (var k = 0; k < subStrings.Length; k++)
                    {
                        var str = subStrings[k];
                        if (chars[i] != str[counters[k]++])
                            counters[k] = 0;
                        else if (counters[k] == str.Length)
                        {
                            finded = str;
                            return i - str.Length + 1;
                        }
                    }
                }
            }
            else
            {
                for (var i = offset; i >= 0 && left < 0; i--, left++)
                {
                    for (var k = 0; k < subStrings.Length; k++)
                    {
                        var str = subStrings[k];
                        if (chars[i] != str[str.Length - 1 - counters[k]++])
                            counters[k] = 0;
                        else if (counters[k] == str.Length)
                        {
                            finded = str;
                            return i - str.Length + 1;
                        }
                    }
                }
            }

            finded = null;
            return -1;
        }

    }
}