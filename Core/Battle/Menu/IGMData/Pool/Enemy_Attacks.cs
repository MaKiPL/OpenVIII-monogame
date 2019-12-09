using Microsoft.Xna.Framework;

namespace OpenVIII.IGMData.Pool
{
    public class Enemy_Attacks : Base<Enemy, Debug_battleDat.Abilities>
    {
        public static Enemy_Attacks Create(Rectangle pos, Damageable damageable = null, bool battle = false, int count = 4)
        {
            Enemy_Attacks r = Create<Enemy_Attacks>(count + 1, 3, new IGMDataItem.Box { Pos = pos, Title = Icons.ID.ITEM }, count, 198 / count + 1, damageable, battle: battle);
            if (battle)
                r.Target_Group = Target.Group.Create(r.Damageable);
            return r;

        }
        protected Target.Group Target_Group { get => ((Target.Group)ITEM[Count - 3, 0]); private set => ITEM[Count - 3, 0] = value; }
    }
}