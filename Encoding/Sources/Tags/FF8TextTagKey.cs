namespace OpenVIII.Encoding.Tags
{
    public enum FF8TextTagKey : byte
    {
        /// <summary>
        /// Left Escape Button, (D), (L2), 2000_(Z)
        /// </summary>
        EscapeLeft = 0x20,
        /// <summary>
        /// Right Escape Button, Switch POV on world, (F),(R2), 2000_(C)
        /// </summary>
        EscapeRight = 0x21,
        /// <summary>
        /// Rotate Left/Show Target Window in battle, Switch Character or GF Left in Menu, (H), (L1), 2000_(Q)
        /// </summary>
        RotateLeft = 0x22,
        /// <summary>
        /// Rotate Right/Gunblade trigger in battle, Switch Character or GF Right in Menu, (G), (R1), 2000_(E)
        /// </summary>
        RotateRight = 0x23,
        /// <summary>
        /// Walk/Cancel, (C), (Triangle), Drive backward (Right stick down), 2000_(W)
        /// </summary>
        Cancel = 0x24,
        /// <summary>
        /// Menu, Switch Character in Battle, (V), (Circle), 2000_(D)
        /// </summary>
        Menu = 0x25,
        /// <summary>
        /// Talk/Confirm, (X), (Cross), 2000_(X)
        /// </summary>
        Confirm = 0x26,
        /// <summary>
        /// Talk/Card Game,Show Status in battle, (S), (Square), 2000_(A), Drive forward (Right stick up), Tap in battle boost gf if hit while holding Select
        /// </summary>
        Cards = 0x27,
        /// <summary>
        /// Toggle Display,Default, (J) (Select), 2000_(Q)
        /// </summary>
        Select = 0x28,
        /// <summary>
        /// Pause / Display Help Info in battle, End Concert Key / Toggle Viberation Mode, (A) (Start), 2000_(S)
        /// </summary>
        Pause = 0x2B,
        /// <summary>
        /// Up D-PAD, or left stick
        /// </summary>
        Up = 0x2C,
        /// <summary>
        /// Right D-PAD, or left stick
        /// </summary>
        Right = 0x2D,
        /// <summary>
        /// Down D-PAD, or left stick
        /// </summary>
        Down = 0x2E,
        /// <summary>
        /// Left D-PAD, or left stick
        /// </summary>
        Left = 0x2F,
        /// <summary>
        /// Same as 0x20
        /// </summary>
        x30 = 0x30,
        /// <summary>
        /// Same as 0x21
        /// </summary>
        x31 = 0x31,
        /// <summary>
        /// Same as 0x22
        /// </summary>
        x32 = 0x32,
        /// <summary>
        /// Same as 0x23
        /// </summary>
        x33 = 0x33,
        /// <summary>
        /// Same as 0x24
        /// </summary>
        x34 = 0x34,
        /// <summary>
        /// Same as 0x25
        /// </summary>
        x35 = 0x35,
        /// <summary>
        /// Same as 0x26
        /// </summary>
        x36 = 0x36,
        /// <summary>
        /// Same as 0x27
        /// </summary>
        x37 = 0x37,
        /// <summary>
        /// Same as 0x28
        /// </summary>
        x38 = 0x38,
        /// <summary>
        /// Same as 0x29
        /// </summary>
        x39 = 0x39,
        /// <summary>
        /// Same as 0x2A
        /// </summary>
        x3A = 0x3A,
        /// <summary>
        /// Same as 0x2B
        /// </summary>
        x3B = 0x3B,
        /// <summary>
        /// Same as 0x2C
        /// </summary>
        x3C = 0x3C,
        /// <summary>
        /// Same as 0x2D
        /// </summary>
        x3D = 0x3D,
        /// <summary>
        /// Same as 0x2E
        /// </summary>
        x3E = 0x3E,
        /// <summary>
        /// Same as 0x2F
        /// </summary>
        x3F = 0x3F,
        /// <summary>
        /// Shows Icons.ID.JunctionSYM, Palette 2
        /// </summary>
        JunctionSYM = 0x41,
        Arrow_Right = 0x42, //Arrow Right Palette 2
        Star = 0x43, //Star Palette 4
        InParty = 0x44, //InParty Palette 2
        Ability_Junction = 0x45, //Ability Junction Palette 9
        Ability_Command = 0x46, //Ability Command Palette 9
        Ability_Character = 0x47, //Ability Character Palette 9
        Ability_Character2 = 0x48, //Ability Character2 Palette 9
        Ability_Party = 0x49, //Ability Party Palette 9
        Ability_GF = 0x4A, //Ability GF Palette 9
        Ability_Menu = 0x4B, //Ability Menu Palette 9
        Item_Recovery = 0x4C, //Item Recovery Palette 9
        Item_GF = 0x4D, //Item GF Palette 9
        Item_Tent = 0x4E, //Item Tent Palette 9
        Item_Battle = 0x4F, //Item Battle Palette 9
        Item_Ammo = 0x50, //Item Ammo Palette 9
        Item_Magazine = 0x51, //Item Mag Palette 9
        Item_Misc = 0x52, //Item Misc Palette 9
        Status_Death = 0x53, //Status Death Palette 10
        Status_Poison = 0x54, //Status Poison Palette 10
        Status_Petrify = 0x55, //Status Petrify Palette 10
        Status_Darkness = 0x56, //Status Darkness Palette 10
        Status_Silence = 0x57, //Status Silence Palette 10
        Status_Berserk = 0x58, //Status Berserk Palette 10
        Status_Zombie = 0x59, //Status Zombie Palette 10
        Status_Deathex1 = 0x5A, //Status Death Palette 10
        Status_Deathex2 = 0x5B, //Status Death Palette 10
        Status_Deathex3 = 0x5C, //Status Death Palette 10
        Element_Fire = 0x5D, //Element Fire Palette 9
        Element_Ice = 0x5E, //Element Ice Palette 9
        Element_Thunder = 0x5F, //Element Thunder Palette 9
        Element_Earth = 0x60, //Element Earth Palette 9
        Element_Poison = 0x61, //Element Poison Palette 9
        Element_Wind = 0x62, //Element Wind Palette 9
        Element_Water = 0x63, //Element Water Palette 9
        Element_Holy = 0x64, //Element Holy Palette 9
        Status_Deathex = 0x65, //Status Death Palette 10
        Status_Poisonex = 0x66, //Status Poison Palette 10
        Status_Petrifyex = 0x67, //Status Petrify Palette 10
        Status_Darknessex = 0x68, //Status Darkness Palette 10
        Status_Silenceex = 0x69, //Status Silence Palette 10
        Status_Berserkex = 0x6A, //Status Berserk Palette 10
        Status_Zombieex = 0x6B, //Status Zombie Palette 10
        Status_Sleep = 0x6C, //Status Sleep Palette 10
        Status_Slow = 0x6D, //Status Slow Palette 10
        Status_Stop = 0x6E, //Status Stop Palette 10
        Status_Curse = 0x6F, //Status Curse Palette 10
        Status_Confuse = 0x70, //Status Confuse Palette 10
        Status_Drain = 0x71, //Status Drain Palette 10
        Icon_Status_Attack = 0x72, //Icon Status Attack Palette 0
        Icon_Status_Defense = 0x73, //Icon Status Defense Palette 0
        Icon_Elemental_Attack = 0x74, //Icon Elemental Attack Palette 0
        Icon_Elemental_Defense = 0x75, //Icon Elemental Defense Palette 0
        Stats_Hit_Points = 0x76, //Stats Hit Points Palette 0
        Stats_Strength = 0x77, //Stats Strength Palette 0
        Stats_Vitality = 0x78, //Stats Vitality Palette 0
        Stats_Magic = 0x79, //Stats Magic Palette 0
        Stats_Spirit = 0x7A, //Stats Spirit Palette 0
        Stats_Speed = 0x7B, //Stats Speed Palette 0
        Stats_Evade = 0x7C, //Stats Evade Palette 0
        Stats_Hit = 0x7D, //Stats Hit Palette 0
        Stats_Luck = 0x7E, //Stats Luck Palette 0
        Finger_Right = 0x7F, //Finger Right Palette 2
        /// <remarks>Everything I've tested beyond here showed up blank in game. Though I only went to about C0</remarks>
        ///// <summary>
        ///// Custom Entry: If mouse over a multi paged screen mousewheel down becomes left.
        ///// </summary>
        //ScrollLeft = 0xFB,
        ///// <summary>
        ///// Custom Entry: If mouse over a multi paged screen mousewheel down becomes right.
        ///// </summary>
        //ScrollRight = 0xFC,
        /// <summary>
        /// Custom Entry: Shows an "Are you sure you want to quit?" screen. (Escape)
        /// </summary>
        ExitMenu = 0xFD,
        /// <summary>
        /// Custom Entry: Button that goes directly to Lobby Menu. (Control-R) (Start+Select+L1+R1+L2+R2)
        /// </summary>
        Reset = 0xFE,
        /// <summary>
        /// Custom Entry: Directly Exit (Control-Q)
        /// </summary>
        Exit = 0xFF
    }
}