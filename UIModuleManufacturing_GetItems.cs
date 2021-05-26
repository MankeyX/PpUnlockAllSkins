using System;
using System.Linq;
using Base.Core;
using Harmony;
using PhoenixPoint.Common.Core;
using PhoenixPoint.Common.Entities.Items;
using PhoenixPoint.Common.Entities.RedeemableCodes;
using PhoenixPoint.Geoscape.Levels;
using PhoenixPoint.Geoscape.View.ViewModules;

namespace PpUnlockAllSkins
{
    public class Main
    {
        public static void Init () => GeoscapeMod();
        public static void MainMod ( Func<string,object,object> api ) => GeoscapeMod( api );
   
        public static void GeoscapeMod ( Func<string,object,object> api = null ) {
            HarmonyInstance.Create( $"{nameof(PpUnlockAllSkins)}.1" ).PatchAll();
        }
    }
    
    [HarmonyPatch(typeof(UIModuleManufacturing), "GetItems")]
    internal static class UIModuleManufacturing_GetItems
    {
        private static bool Prefix()
        {
            var optionsManager = GameUtl.GameComponent<OptionsManager>();

            if (optionsManager != null)
            {
                var geoLevelController = GameUtl.CurrentLevel().GetComponent<GeoLevelController>();

                if (geoLevelController != null)
                {
                    var faction = geoLevelController.ViewerFaction;

                    var items = optionsManager.DefsRepo.GetAllDefs<RedeemableCodeDef>().SelectMany(x => x.GiftedItems);
                    var manufacturableTag = GameUtl.GameComponent<SharedData>().SharedGameTags.ManufacturableTag;

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
            }

            return true;
        }
    }
}