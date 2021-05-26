using System;
using System.Collections.Generic;
using System.Linq;
using Base.Core;
using Base.UI.MessageBox;
using Harmony;
using PhoenixPoint.Common.Core;
using PhoenixPoint.Common.Entities.Items;
using PhoenixPoint.Common.Entities.RedeemableCodes;
using PhoenixPoint.Geoscape.Levels;
using PhoenixPoint.Tactical.Entities.Equipments;

namespace PpUnlockAllSkins
{
    public class Main
    {
        public static void Init() => GameMod();
        public static void MainMod(Func<string, object, object> api) => GameMod(api);

        public static void GameMod(Func<string, object, object> api = null)
        {
            HarmonyInstance.Create($"{nameof(PpUnlockAllSkins)}.1001").PatchAll();
        }
    }

    [HarmonyPatch(typeof(PhoenixStatisticsManager), "OnGeoscapeLevelStart")]
    internal static class GeoLevelController_LevelCrt
    {
        private static void Postfix()
        {
            var optionsManager = GameUtl.GameComponent<OptionsManager>();

            if (optionsManager != null)
            {
                var geoLevelController = GameUtl.CurrentLevel().GetComponent<GeoLevelController>();

                if (geoLevelController != null)
                {
                    try
                    {
                        var faction = geoLevelController.PhoenixFaction;
                        var sharedData = GameUtl.GameComponent<SharedData>().SharedGameTags;
                        
                        var items = new List<TacticalItemDef>();
                        
                        items.AddRange(optionsManager.DefsRepo.GetAllDefs<TacticalItemDef>()
                            .Where(x => x.Tags.Contains(geoLevelController.NeutralFactionDef.Tag)));
                        items.AddRange(optionsManager.DefsRepo.GetAllDefs<RedeemableCodeDef>().SelectMany(x => x.GiftedItems));
                        
                        var manufacturableTag = sharedData.ManufacturableTag;
                        
                        foreach (var item in items)
                        {
                            if (faction.Manufacture.ManufacturableItems.Any(x => x.RelatedItemDef.name == item.name))
                                continue;
                        
                            if (!item.Tags.Contains(manufacturableTag))
                                item.Tags.Add(manufacturableTag);
                        
                            var manufacturableItem = new ManufacturableItem(item);
                        
                            faction.Manufacture.ManufacturableItems.Add(manufacturableItem);
                        }
                    }
                    catch (Exception e)
                    {
                        GameUtl.GetMessageBox().ShowSimplePrompt($"2{e.Message}", MessageBoxIcon.Error, MessageBoxButtons.OK, null);
                    }
                }
            }
        }
    }
}