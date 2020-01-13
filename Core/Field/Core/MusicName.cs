using System;
using System.Collections.Generic;

// ReSharper disable StringLiteralTypo

namespace OpenVIII.Fields
{
    public static class MusicName
    {
        public static String Get(MusicId id)
        {
            if (_dic.TryGetValue(id, out var name))
                return name;

            return $"Unknown music: {id}";
        }

        private static readonly Dictionary<MusicId, String> _dic = new Dictionary<MusicId, String>()
        {
            {MusicId.Loser, "The Loser"},
            {MusicId.Winner, "The Winner "},    
            {MusicId.Music04, "Never Look Back"},
            {MusicId.Battle01, "Don't be Afraid (Regular Battle)"},
            {MusicId.Music7, "Dead End"},
            {MusicId.Music8, "Starting Up"},
            {MusicId.Music9, "Intruders"},
            {MusicId.Battle02, "Don't be Afraid (with X-ATM Intro)"},
            {MusicId.Battle03, "Force Your Way (boss battle)"},
            {MusicId.Music14, "Parade (No Intro)"},
            {MusicId.Music15, "Unrest"},
            {MusicId.Music16, "The Stage is Set"},
            {MusicId.Music17, "The Landing"},
            {MusicId.Music18, "Love Grows"},
            {MusicId.Music19, "Waltz for the Moon"},
            {MusicId.Music20, "Ami"},
            {MusicId.Music21, "Find Your Way"},
            {MusicId.Music22, "Julia"},
            {MusicId.Music23, "Parade"},
            {MusicId.Music24, "SeeD"},
            {MusicId.Music25, "Tell Me"},
            {MusicId.Music26, "Balamb Garden"},
            {MusicId.Music27, "Fear"},
            {MusicId.Music28, "Dance with the Balamb-Fish"},
            {MusicId.Music29, "Cactus Jack"},
            {MusicId.Music35, "The Mission"},
            {MusicId.Music36, "SUCCESSION OF WITCHES"},
            {MusicId.Music41, "Blue Fields"},
            {MusicId.Music42, "Breezy"},
            {MusicId.Music46, "Timer Owls"},
            {MusicId.Music47, "Fragments of Memories"},
            {MusicId.Music48, "Fisherman's Horizon'"},
            {MusicId.Music49, "Heresy"},
            {MusicId.Music51, "My Mind"},
            {MusicId.Music52, "Where I Belong"},
            {MusicId.Music53, "Starting Up (Looped)"},
            {MusicId.Music54, "Truth"},
            {MusicId.Music55, "Trust Me"},
            {MusicId.Music56, "Galbadia GARDEN"},
            {MusicId.Music57, "Martial Law"},
            {MusicId.Music58, "Under Her Control"},
            {MusicId.Battle04, "Only a Plank between One and Perdition (Bahamut battle)"},
            {MusicId.Music60, "Junction"},
            {MusicId.Music61, "Roses and Wine"},
            {MusicId.Battle05, "Man with the Machine Gun (Laguna's battle theme)"},
            {MusicId.Music63, "A Sacrifice"},
            {MusicId.Music64, "ODEKA ke Chocobo"},
            {MusicId.Music65, "Drifting"},
            {MusicId.Music66, "Wounded"},
            {MusicId.Music67, "Jailed"},
            {MusicId.Music68, "Retaliation"},
            {MusicId.Music69, "The Oath"},
            {MusicId.Music70, "Shuffle or Boogie"},
            {MusicId.Music71, "Rivals"},
            {MusicId.Music72, "Blue Sky"},
            {MusicId.Battle06, "Premonition (Sorceress battles)"},
            {MusicId.Music75, "Galbadia GARDEN (No Intro)"},
            {MusicId.Battle07, "Maybe I'm a Lion (vs Griever)"},
            {MusicId.Music77, "The Castle"},
            {MusicId.Music78, "Movin''"},
            {MusicId.Music79, "Overture"},
            {MusicId.Music80, "The Spy"},
            {MusicId.Music81, "Mods de Chocobo"},
            {MusicId.Music82, "The Salt Flats"},
            {MusicId.Music83, "The Residents"},
            {MusicId.Music84, "Lunatic Pandora"},
            {MusicId.Music85, "Silence and Motion"},
            {MusicId.Music86, "Tears of the Moon"},
            {MusicId.Music89, "Ride On"},
            {MusicId.Battle08, "The Legendary Beast (Junctioned Griever))"},
            {MusicId.Music91, "Slide Show Part 1"},
            {MusicId.Music92, "Slide Show Part 2"},
            {MusicId.Battle09, "The Extreme (Ultimecia final battle) "},
            {MusicId.Music96, "The Successor"},
            {MusicId.Music97, "Compression of Time"},
            {MusicId.Music99, "The Landing (No Intro)"},
            {MusicId.FHConcertTap, "FH Concert (tap)"},
            {MusicId.FHConcertFlute, "FH Concert (flute)"},
            {MusicId.FHConcertFiddle, "FH Concert (fiddle)"},
            {MusicId.FHConcertGuitar, "FH Concert (guitar)"},
            {MusicId.FHConcertSax, "FH Concert (sax)"},
            {MusicId.FHConcertPiano, "FH Concert (piano)"},
            {MusicId.FHConcertEGuitar, "FH Concert (e.guitar)"},
            {MusicId.FHConcertEBass, "FH Concert (e.bass)"},
            {MusicId.ChocoboWorld, "Chocobo World (Credits)"}
        };
    }
}