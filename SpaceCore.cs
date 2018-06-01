﻿using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore.Events;
using SpaceCore.Overrides;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using SFarmer = StardewValley.Farmer;

namespace SpaceCore
{
    public class SpaceCore : Mod
    {
        internal static SpaceCore instance;
        private HarmonyInstance harmony;

        public SpaceCore()
        {
        }

        public override void Entry(IModHelper helper)
        {
            instance = this;

            SpecialisedEvents.UnvalidatedUpdateTick += onUpdate;

            SaveEvents.AfterLoad += onLoad;
            SaveEvents.AfterSave += onSave;

            Commands.register();

            harmony = HarmonyInstance.Create("spacechase0.SpaceCore");
            doPrefix(typeof(HoeDirt), "dayUpdate", typeof(HoeDirtWinterFix));
            doPostfix(typeof(Utility), "pickFarmEvent", typeof(NightlyFarmEventHook));
            doTranspiler(typeof(Game1), "showEndOfNightStuff", typeof(ShowEndOfNightStuffHook));
            doPostfix(typeof(SFarmer), "doneEating", typeof(DoneEatingHook));
            doPrefix(typeof(Game1), "setGraphicsForSeason", typeof(SeasonGraphicsForSeasonalLocationsPatch));
            doPrefix(typeof(MeleeWeapon).GetMethod("drawDuringUse", new[] { typeof(int), typeof(int), typeof(SpriteBatch), typeof(Vector2), typeof(SFarmer), typeof(Rectangle), typeof(int), typeof(bool) }), typeof(CustomWeaponDrawPatch).GetMethod("Prefix"));

            SpaceEvents.OnItemEaten += (sender, e) =>
            {
                Monitor.Log(Game1.player.itemToEat.Name + " was eaten!");
            };

            Monitor.Log("Initialized");
        }

        private void doPrefix(Type origType, string origMethod, Type newType)
        {
            doPrefix(origType.GetMethod(origMethod), newType.GetMethod("Prefix"));
        }
        private void doPrefix(MethodInfo orig, MethodInfo prefix)
        {
            try
            {
                Log.trace($"Doing prefix patch {orig}:{prefix}...");
                harmony.Patch(orig, new HarmonyMethod(prefix), null);
            }
            catch (Exception e)
            {
                Log.error($"Exception doing prefix patch {orig}:{prefix}: {e}");
            }
        }
        private void doPostfix(Type origType, string origMethod, Type newType)
        {
            doPostfix(origType.GetMethod(origMethod), newType.GetMethod("Postfix"));
        }
        private void doPostfix(MethodInfo orig, MethodInfo postfix)
        {
            try
            {
                Log.trace($"Doing postfix patch {orig}:{postfix}...");
                harmony.Patch(orig, null, new HarmonyMethod(postfix));
            }
            catch (Exception e)
            {
                Log.error($"Exception doing postfix patch {orig}:{postfix}: {e}");
            }
        }
        private void doTranspiler(Type origType, string origMethod, Type newType)
        {
            doTranspiler(origType.GetMethod(origMethod), newType.GetMethod("Transpiler"));
        }
        private void doTranspiler(MethodInfo orig, MethodInfo transpiler)
        {
            try
            {
                Log.trace($"Doing transpiler patch {orig}:{transpiler}...");
                harmony.Patch(orig, null, null, new HarmonyMethod(transpiler));
            }
            catch (Exception e)
            {
                Log.error($"Exception doing transpiler patch {orig}:{transpiler}: {e}");
            }
        }

        private int prevLoaderNum = 0;
        private void onUpdate( object sender, EventArgs args )
        {
            if (Game1.currentLoader != null)
            {
                if (Game1.currentLoader.Current == 25 && prevLoaderNum != 25)
                {
                    SpaceEvents.InvokeOnBlankSave();
                }
                prevLoaderNum = Game1.currentLoader.Current;
            }
            //Log.debug("L:" + (Game1.currentLoader != null ? Game1.currentLoader.Current:-1));
        }

        private void onLoad(object sender, EventArgs args)
        {
            var data = Helper.ReadJsonFile<Sleep.Data>(Path.Combine(Constants.CurrentSavePath, "sleepy-eye.json"));
            if (data == null || data.Year != Game1.year || data.Season != Game1.currentSeason || data.Day != Game1.dayOfMonth)
                return;

            Log.debug("Previously slept in a tent, replacing player position.");

            var loc = Game1.getLocationFromName(data.Location);
            if (loc == null || loc.Name == festivalLocation())
            {
                Game1.addHUDMessage(new HUDMessage("You camped out where the festival was, so you have returned home."));
                return;
            }

            if (loc is MineShaft)
            {
                // [DefenTheNation]
                // The MineShaft class no longer has .enterMine method
                // so I replaced it with loading of the mine level and
                // getting the entrance position, which I assume is
                // what enterMine returned

                MineShaft mine = loc as MineShaft;
                Log.trace("Slept in a mine.");

                mine.loadLevel(data.MineLevel);
                Vector2 pos = mine.mineEntrancePosition(Game1.player);

                // enterMine function
                //var pos = (loc as MineShaft).enterMine(Game1.player, data.MineLevel, false);

                data.X = pos.X * Game1.tileSize;
                data.Y = pos.Y * Game1.tileSize;
            }

            Game1.player.currentLocation = Game1.currentLocation = loc;
            Game1.player.Position = new Vector2(data.X, data.Y);
        }

        private void onSave(object sender, EventArgs args)
        {
            if (!Sleep.SaveLocation)
                return;

            Log.debug("Saving tent sleep data");

            if (Game1.player.currentLocation.Name == festivalLocation())
            {
                Log.trace("There'll be a festival here tomorrow, canceling");
                Game1.addHUDMessage(new HUDMessage("You camped out where the festival was, so you have returned home."));

                var house = Game1.getLocationFromName("FarmHouse") as FarmHouse;
                Game1.player.currentLocation = Game1.currentLocation = house;
                Game1.player.Position = new Vector2(house.getBedSpot().X * Game1.tileSize, house.getBedSpot().Y * Game1.tileSize);
                Sleep.SaveLocation = false;
                return;
            }

            var data = new Sleep.Data();
            data.Location = Game1.currentLocation.Name;
            data.X = Game1.player.position.X;
            data.Y = Game1.player.position.Y;

            data.Year = Game1.year;
            data.Season = Game1.currentSeason;
            data.Day = Game1.dayOfMonth;

            if (Game1.currentLocation is MineShaft)
            {
                data.MineLevel = (Game1.currentLocation as MineShaft).mineLevel;
            }

            Helper.WriteJsonFile<Sleep.Data>(Path.Combine(Constants.CurrentSavePath, "sleepy-eye.json"), data);
            Sleep.SaveLocation = false;
        }

        // TODO: Move somewhere more sensible (and make public)?
        internal string festivalLocation()
        {
            try
            {
                return Game1.temporaryContent.Load<Dictionary<string, string>>("Data\\Festivals\\" + Game1.currentSeason + (object)Game1.dayOfMonth)["conditions"].Split('/')[0];
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
