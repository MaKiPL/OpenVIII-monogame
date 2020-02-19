using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// <para>Set Battle Music</para>
    /// <para>Sets which battle music starts playing when a battle starts. Here are some common ones, but you can use any music in the game for a battle</para>
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/0CB_SETBATTLEMUSIC"/>
    /// <remarks>5 = Don't be Afraid (Regular Battle)</remarks>
    /// <remarks>12 = Don't be Afraid (with X-ATM Intro)</remarks>
    /// <remarks>13 = Force Your Way(boss battle)</remarks>
    /// <remarks>59 = Only a Plank between One and Perdition(Bahamut battle)</remarks>
    /// <remarks>62 = Man with the Machine Gun(Laguna's battle theme)</remarks>
    /// <remarks>73 = Premonition (Sorceress battles)</remarks>
    /// <remarks>76 = Maybe I'm a Lion (vs Griever)</remarks>
    /// <remarks>90 = The Legendary Beast (Junctioned Griever))</remarks>
    /// <remarks>93 = The Extreme(Ultimecia final battle)</remarks>

    public sealed class SETBATTLEMUSIC : JsmInstruction
    {
        private MusicId _musicId;

        public SETBATTLEMUSIC(MusicId musicId)
        {
            _musicId = musicId;
        }

        public SETBATTLEMUSIC(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                musicId: (MusicId)((Jsm.Expression.PSHN_L)stack.Pop()).Int32())
        {
        }

        public override String ToString()
        {
            return $"{nameof(SETBATTLEMUSIC)}({nameof(_musicId)}: {_musicId})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .CommentLine(MusicName.Get(_musicId))
                .StaticType(nameof(IMusicService))
                .Method(nameof(IMusicService.ChangeBattleMusic))
                .Enum(_musicId)
                .Comment(nameof(SETBATTLEMUSIC));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Music[services].ChangeBattleMusic(_musicId);
            return DummyAwaitable.Instance;
        }
    }
}