﻿using Harmony;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCore.Overrides
{
    [HarmonyPatch(typeof(HoeDirt), "dayUpdate")]
    public class HoeDirtWinterFix
    {
        public static void dayUpdate(HoeDirt hoeDirt, GameLocation environment, Vector2 tileLocation)
        {
            // [DefenTheNation]
            // Multiplayer changed some strings/ints to NetString and NetInt
            // so I added .Value to get the underlying base type

            if (hoeDirt.crop != null)
            {
                hoeDirt.crop.newDay(hoeDirt.state.Value, hoeDirt.fertilizer.Value, (int)tileLocation.X, (int)tileLocation.Y, environment);
                //if (!environment.name.Equals("Greenhouse") && Game1.currentSeason.Equals("winter") && (this.crop != null && !this.crop.isWildSeedCrop()))
                //    this.destroyCrop(tileLocation, false);
            }
            if (hoeDirt.fertilizer.Value == 370 && Game1.random.NextDouble() < 0.33 || hoeDirt.fertilizer.Value == 371 && Game1.random.NextDouble() < 0.66)
                return;

            hoeDirt.state.Value = 0;
        }

        // TODO: Make this do IL hooking instead of pre + no execute original
        public static bool Prefix(HoeDirt __instance, GameLocation environment, Vector2 tileLocation)
        {
            dayUpdate(__instance, environment, tileLocation);
            return false;
        }
    }
}
