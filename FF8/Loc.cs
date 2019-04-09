namespace FF8
{
    
        public struct Loc
        {
            public ushort pos;
            /// <summary>
            /// sometimes there is more than one entry at a location each is 8 bytes
            /// </summary>
            public ushort count;
        }
    
}