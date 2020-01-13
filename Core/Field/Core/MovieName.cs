using System;
using System.Collections.Generic;
// ReSharper disable StringLiteralTypo

namespace OpenVIII.Fields
{
    public static class MovieName
    {
        public static IEnumerable<String> PossibleNames(Int32 movieId)
        {
            Boolean isFound = false;

            if (movieId < Disk1.Length)
            {
                isFound = true;
                yield return "Disk 1: " + Disk1[movieId];
            }

            if (movieId < Disk2.Length)
            {
                isFound = true;
                yield return "Disk 2: " + Disk2[movieId];
            }

            if (movieId < Disk3.Length)
            {
                isFound = true;
                yield return "Disk 3: " + Disk3[movieId];
            }

            if (movieId < Disk4.Length)
            {
                isFound = true;
                yield return "Disk 4: " + Disk3[movieId];
            }

            if (!isFound)
                throw new NotSupportedException($"Unknown movie id: {movieId}");
        }

        private static readonly String[] Disk1 =
        {
            "Balamb Garden explore",
            "Quistis Appears",
            "Zell Appears",
            "Dollet Intro",
            "Selphie Appears",
            "Dollet Tower Transform",
            "X-ATM Steps on a Car",
            "Dollet Escape",
            "Ballroom - Shooting Star",
            "Ballroom Dance",
            "Timber Train 1",
            "Timber Train 2",
            "Timber TV Distortion",
            "Timber TV Camera Tilt",
            "Enter Galbadia Garden",
            "Irvine Appears",
            "Enter Deling City",
            "Edea Appears",
            "Edea Approaches Podium",
            "Crowd Cheers",
            "Iguions Attack",
            "Parade Starts",
            "Parade 1",
            "Parade 2",
            "Carousel Raises",
            "Gateway Trap",
            "Irvine's Shot",
            "Squall Assaults Edea",
            "Seifer and Edea",
            "Wounded",
            "Liberi Fatali"
        };

        private static readonly String[] Disk2 =
        {
            "Squall's Cell",
            "Prison Desert Overlook",
            "Prison Submerges 1",
            "Prison Submerges 2",
            "Missile Launch",
            "Missile Base Explodes 1",
            "Missile Base Explodes 2",
            "Missiles in Clouds",
            "Missiles over Water",
            "MD Machine Startup",
            "Garden Ring Energizes",
            "Missile Impact",
            "Missiles Destroy Garden (unused)",
            "Flying Garden Overlook",
            "Flying Garden Overlook (w/ Rinoa)",
            "Garden over Balamb",
            "Garden Crashes into Ocean",
            "Garden in Ocean",
            "White SeeD Ship",
            "Crash into FH",
            "FH Overlook",
            "G-Garden through Binocs",
            "G-Garden through Forest",
            "Garden Showdown",
            "G-Cyclist Attack",
            "Rinoa Falls",
            "After Rinoa Falls",
            "G-Garden pre-Ram",
            "G-Garden Ram",
            "G-Mechs Launch",
            "Another Ram",
            "Balamb Ram",
            "Escape Hatch",
            "Paratrooper fight"
        };

        private static readonly String[] Disk3 =
        {
            "Esthar Decloaks",
            "Esthar Elevator 1",
            "Esthar Leave (car)",
            "Esthar Arrive (car)",
            "Presidential Palace leave?",
            "Presidential Palace enter?",
            "Pandora Lab Elevator down",
            "Space Pod Launch",
            "Space Pods exit atmosphere",
            "Space Station enter",
            "Closeup of Moon 1",
            "Closeup of Moon 2",
            "LP over Esthar",
            "LP Over Tears' Point",
            "Monsters falling off Moon",
            "Adel's Tomb",
            "Escape Pods Launch",
            "Rinoa in Space 1",
            "Rinoa in Space 2",
            "Rinoa in Space 3",
            "Rinoa in Space - Ring",
            "Rinoa Life Support",
            "View of Lunar Cry",
            "Ragnarok Conveniently Appears",
            "Board Ragnarok",
            "Ragnarok Hatch Open",
            "Unlock Rinoa's Tomb",
            "Enter LP 1",
            "Ragnarok with Adel's Tomb",
            "Enter LP 2",
            "Lunar Cry",
            "Lunar Cry Begins"
        };

        private static readonly String[] Disk4 =
        {
            "Rinoa and Adel",
            "Time Compression",
            "Ultimecia's Castle",
            "Showdown with Ultimecia",
            "Ending 1 (Squall in Limbo)",
            "Credits 2 (Balcony)",
            "Ending and Credits"
        };
    }
}