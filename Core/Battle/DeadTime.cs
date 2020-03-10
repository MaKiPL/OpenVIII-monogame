using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace OpenVIII
{
    public class DeadTime : Slide<int>
    {
        private static int Lerp(int start, int end, float amount) => checked((int)MathHelper.Lerp(start, end, amount));

        private static TimeSpan GenerateTotalTime(int start) => TimeSpan.FromMilliseconds((1000d / 15d) * start);

        public DeadTime() : base(200, 0, GenerateTotalTime(200), Lerp)
        {
            Repeat = true;
            DoneEvent += DeadTime_DoneEvent;
        }

        protected override void CheckRepeat()
        {
            if (Done)
            {
                DoneEvent?.Invoke(this, End);
            }
            base.CheckRepeat();
        }

        public override void GotoEnd()
        {
            DoneEvent?.Invoke(this, End);
            base.GotoEnd();
        }

        private event EventHandler<int> DoneEvent;

        public event EventHandler<int> Gilgamesh;

        public event EventHandler<Saves.Item> AngeloSearch;

        public event EventHandler<Characters> AngeloReverse;

        public event EventHandler<Characters> AngeloRecover;

        /// <summary>
        /// Test if an ability can trigger for Angelo.
        /// </summary>
        /// <param name="ability"></param>
        /// <returns></returns>
        private static bool TestAngeloAbilityTriggers(Angelo ability)
        {
            //else if (8 >= [0..255] Angelo Recover is used (3.3 %)
            //else if (2 >= [0..255] Angelo Reverse is used (1 %)
            //else if (8 >= [0..255] Angelo Search is used (3.2 %)
            //Angelo_Disabled I think is set when Rinoa is in space so angelo is out of reach;
            //https://gamefaqs.gamespot.com/ps/197343-final-fantasy-viii/faqs/25194
            if (Memory.State.BattleMiscFlags.HasFlag(Saves.BattleMiscFlags.AngeloDisabled) ||
                !Memory.State.PartyData.Contains(Characters.Rinoa_Heartilly) ||
                !Memory.State.LimitBreakAngeloCompleted.HasFlag(ability)) return false;
            else
                switch (ability)
                {
                    case Angelo.Recover:
                        return Memory.State.Characters.Any(x => x.Value.IsCritical && !x.Value.IsDead && Memory.State.PartyData.Contains(x.Key)) && Memory.Random.Next(256) < 8;

                    case Angelo.Reverse:
                        return Memory.State.Characters.Any(x => x.Value.IsDead && Memory.State.PartyData.Contains(x.Key)) && Memory.Random.Next(256) < 2;

                    case Angelo.Search:
                        Saves.CharacterData c = Memory.State[Characters.Rinoa_Heartilly];
                        if (!(c.IsGameOver ||

                            c.Statuses1.HasFlag(Kernel.BattleOnlyStatuses.Sleep) ||
                            c.Statuses1.HasFlag(Kernel.BattleOnlyStatuses.Stop) ||
                            c.Statuses1.HasFlag(Kernel.BattleOnlyStatuses.Confuse) ||
                            c.Statuses0.HasFlag(Kernel.PersistentStatuses.Berserk) ||
                            c.Statuses1.HasFlag(Kernel.BattleOnlyStatuses.AngelWing)))

                            return Memory.Random.Next(256) < 8;
                        break;
                }
            return false;
        }

        /// <summary>
        /// if (12 >= [0..255]) Gilgamesh is summoned (5.1 %)
        /// </summary>
        /// <returns>bool</returns>
        private static bool TestGilgamesh() =>

            Memory.State.BattleMiscFlags.HasFlag(Saves.BattleMiscFlags.Gilgamesh) && Memory.Random.Next(256) <= 12;

        /// <summary>
        /// Trigger Event when DeadTime is done.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <see cref="https://gamefaqs.gamespot.com/ps/197343-final-fantasy-viii/faqs/58936"/>
        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        private void DeadTime_DoneEvent(object sender, int e)
        {
            bool save(IEnumerable<Characters> hurt, ref EventHandler<Characters> @event)
            {
                if (hurt == null || !hurt.Any()) return false;
                Characters c = hurt.Random();
                @event?.Invoke(this, c);
                return true;
            }
            //Will Gilgamesh appear?
            if (TestGilgamesh())
            {
                Gilgamesh?.Invoke(this, 0);
            }
            //Will Angelo Recover be used?
            else if (TestAngeloAbilityTriggers(Angelo.Recover) &&
                save(Memory.State.Party.Where(x => x != Characters.Blank && Memory.State[x].IsCritical), ref AngeloRecover))
            { }
            //Will Angelo Reverse be used?
            else if (TestAngeloAbilityTriggers(Angelo.Reverse) &&
                save(Memory.State.Party.Where(x => x != Characters.Blank && Memory.State[x].IsDead), ref AngeloReverse))
            {}
            //Will Angelo Search be used?
            else if (TestAngeloAbilityTriggers(Angelo.Search))
            {
                //Real game has a counter that Count to 255 and resets to 0
                //instead of a random number. The counter counts up every 1 tick.
                //60 ticks per second.
                byte rnd = checked((byte)Memory.Random.Next(256));
                if (rnd < 128) AngeloSearch?.Invoke(this,Algorithm(1));
                else if (rnd < 160) AngeloSearch?.Invoke(this, Algorithm(2));
                else if (rnd < 176) AngeloSearch?.Invoke(this, Algorithm(3));
                else if (rnd < 192) AngeloSearch?.Invoke(this, Algorithm(4));
                else if (rnd < 200) AngeloSearch?.Invoke(this, Algorithm(5));
                else AngeloSearch?.Invoke(this, Algorithm(6));
                Saves.Item Algorithm(byte i)
                {
                    //https://gamefaqs.gamespot.com/ps/197343-final-fantasy-viii/faqs/58936
                    //I'm unsure where in the game files this is.
                    //In remaster they changed this. But unsure how
                    // they added (Ribbon, Friendship, and Mog's Amulet)
                    // using a true random kinda breaks this.
                    // because in game random is a set array of numbers 0-255
                    // so the number you get previous would determine the possible number you get
                    // so these can only select specific numbers. But because we are using a real random
                    // more items are possible. might need to tweak this.

                    //these are added in remaster as possible items.
                    //const byte Ribbon = 100;
                    //const byte Friendship = 32;
                    //const byte Mog's_Amulet = 65;

                    Saves.Item item = new Saves.Item { QTY = 1 };
                    rnd = checked((byte)Memory.Random.Next(256));
                    switch (i)
                    {
                        case 1: // 1-8
                            item.ID = (byte)(rnd % 8 + 1);
                            break;

                        case 2: // 102-199
                            item.ID = (byte)(rnd % 98);
                            if (item.ID == 0) item.ID = 98;
                            item.ID += 101;
                            break;

                        case 3: // 102-124
                            item.ID = (byte)(rnd % 23);
                            if (item.ID == 0)
                                item.ID = 23;
                            item.ID += 101;
                            break;

                        case 4: // 67-100
                            item.ID = (byte)(rnd % 34);
                            if (item.ID == 0)
                                item.ID = 34;
                            item.ID += 66;
                            break;

                        case 5: // 33-54
                            item.ID = (byte)(rnd % 32 + 33);
                            break;

                        default: // 33-40
                            item.ID = (byte)(rnd % 7 + 33);
                            break;
                    }
                    return item;
                }
            }
        }
    }
}
