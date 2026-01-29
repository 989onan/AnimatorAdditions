using HarmonyLib;
using ResoniteModLoader;
using Elements.Core;
using FrooxEngine;
using Elements.Assets;
using System.Reflection;
using System.Collections.Generic;

namespace AnimatorAdditions
{
    public class AnimatorAdditions : ResoniteMod
    {
        public static ModConfiguration Config;


        public override string Author => "989onan";

        public override string Link => "https://github.com/989onan/AnimatorAdditions";

        public override string Name => "AnimatorAdditions";

        public override string Version => "0.0.0";


        public override void OnEngineInit()
        {
            Harmony harmony = new Harmony($"{Author}.{Name}");
            Config = GetConfiguration();
            Config.Save(true);
            Msg("patching method for animator ui");

            harmony.PatchAll();

        }


        [HarmonyPatch]
        public class AnimatorAdditions
        {
            [HarmonyPostfix(typeof(FrooxEngine.Animator), "BuildInspectorUI")]
            public static bool PostFix(Animator __instance, UIBuilder ui)
            {
                ui.Header("Animator Additions");
                ui.Button("Wipe Unneeded Fields", () =>
                {
                    WipeUnneededFields(__instance);
                });
                return true; // Continue with the original method after our additions
            }

            public static void WipeUnneededFields(Animator __instance)
            {
                List<int> unneeded_fields = new List<int>();
                for (int i = 0; i < __instance.Fields.Count; i++)
                {

                    if (__instance.Fields[i].Target == null)
                    {
                        unneeded_fields.Add(i);
                    }
                }

                unneeded_fields.Reverse()// Remove from the end to avoid index shifting
                //yeet the unused fields since we don't need them
                AnimX new_Anim = new AnimX();
                __instance.Clip.Asset.Data
                foreach (int i in )
                {
                    
                    .RemoveTrackAt(i);
                }
                for (int i = 0; i < __instance.Clip.Asset.Data.Count; i++)
                {
                    if(!unneeded_fields.Contains(i)){
                        new_Anim.AddTrack(__instance.Clip.Asset.Data[i]); //copy over needed tracks
                    }
                    else
                    {
                        __instance.Fields.RemoveAt(i);
                    }
                }

                __instance.Clip.Asset.SetFromAnimX(new_Anim); // Update the clip's data

                //Hope it works!
            }
        }


    }
}