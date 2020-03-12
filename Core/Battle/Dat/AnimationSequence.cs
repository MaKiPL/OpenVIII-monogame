using System.Collections.Generic;

namespace OpenVIII.Battle.Dat
{
    public struct AnimationSequence
    {
        public int ID;
        public uint Offset;
        public byte[] Data;

        /// <summary>
        /// Test-Reason for list is so i can go read the data with out removing it.
        /// </summary>
        public List<byte> AnimationQueue { get; set; }

        //public static Dictionary<byte, Action<byte[], int>> ParseData = new Dictionary<byte, Action<byte[], int>>{
        //    { 0xA3, (byte[] data, int i) => { } } };
        public void GenerateQueue(DebugBattleDat dat)
        {
            AnimationQueue = new List<byte>();
            for (int i = 0; Data != null && i < Data.Length; i++)
            {
                byte b;
                byte[] data = this.Data;
                byte get(int j = -1)
                {
                    return b = data[j < 0 ? i : j];
                }
                if (get() < (dat.animHeader.animations?.Length ?? 0))
                {
                    AnimationQueue.Add(b);
                }
                //else switch(b)
                //{
                //        case 0xA3:
                //            // following value is animation.
                //            break;
                //        case 0xE6:
                //            switch (get(++i))
                //            {
                //                case 0x03:
                //                    i += 1;
                //                    break;
                //            }
                //            break;
                //        case 0xEA:
                //            switch (get(++i))
                //            {
                //                case 0x05:
                //                    i += 1;
                //                    break;
                //                case 0x06:
                //                    i += 2;
                //                    break;
                //            }
                //            break;
                //        default:
                //            i++;//skip next byte //as might not be a animation.
                //            break;
                //}
            }
        }
    }
}