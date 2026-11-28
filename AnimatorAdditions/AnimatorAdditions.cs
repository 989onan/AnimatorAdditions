using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using ResoniteModLoader;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Elements.Assets;

namespace AnimatorAdditions
{
    public class AnimatorAdditions : ResoniteMod
    {
        public static ModConfiguration Config;


        public override string Author => "989onan, art0007i";

        public override string Link => "https://github.com/989onan/AnimatorAdditions";

        public override string Name => "AnimatorAdditions";

        public override string Version => "0.0.0";


        public override void OnEngineInit()
        {
            Harmony harmony = new Harmony($"{Author}.{Name}");
            Msg("patching method for animator ui");
            harmony.Patch(typeof(Animator).GetMethod(nameof(Animator.BuildInspectorUI)), postfix: new HarmonyMethod(AnimatorAdditionsPatch.PostFix));
            harmony.Patch(typeof(Animator).GetMethod("SetupFieldsAsync", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.IgnoreReturn), transpiler: new HarmonyMethod(AnimatorAdditionsPatch.Transpiler_SetupFieldsAsync));

        }

        

        public class AnimatorAdditionsPatch
        {
            public static IEnumerable<CodeInstruction> Transpiler_SetupFieldsAsync(IEnumerable<CodeInstruction> instructions, ILGenerator il)
            {
                Msg("patching method setupfieldsasync");
                var codes = new List<CodeInstruction>(instructions);
                var newcodes = new List<CodeInstruction>(instructions);
                var indexes = new List<int>();

                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Brfalse && codes[i - 1].operand.ToString().Contains("IField"))
                    {
                        indexes.Add(i);
                    }
                }
                indexes.Reverse();




                for (int i = 0; i < indexes.Count; i++)
                {



                    var new_instructions = new List<CodeInstruction>
                    {
                        new CodeInstruction(OpCodes.Dup),
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Call, typeof(AnimatorAdditionsPatch).GetMethod(nameof(AnimatorAdditionsPatch.AddFieldAnyways), BindingFlags.Public | BindingFlags.Static)),
                    };
                    newcodes.InsertRange(indexes[i], new_instructions);
                }


                
                Msg("=====================================================================================");
                Msg("=====================================================================================");
                Msg("=====================================================================================");
                Msg("=====================================================================================");
                for (int i = 0; i < newcodes.Count; i++)
                {
                    Msg("New Animator code instruction" + newcodes[i].opcode + " " + newcodes[i].operand );//" Labels: " + newcodes[i].labels.Join(k=>k+"")
                }

                return newcodes.AsEnumerable();
            }

            public static void AddFieldAnyways(IField mappedField, Animator __instance)
            {
                if(mappedField == null)
                {
                    __instance.Fields.Add();
                }
            }

            public static void PostFix(Animator __instance, UIBuilder ui)
            {
                ui.Text("Animator Additions");
                var button = ui.Button("Wipe Unneeded Fields");
                button.LocalPressed += (IButton button, ButtonEventData data) =>
                {
                    WipeUnneededFieldsAsync(__instance);
                };
            }

            public static async Task WipeUnneededFieldsAsync(Animator __instance)
            {
                Msg("finding unneeded fields");
                List<int> unneeded_fields = new List<int>();
                for (int i = 0; i < __instance.Fields.Count; i++)
                {

                    if (__instance.Fields[i].Target == null)
                    {
                        unneeded_fields.Add(i);
                    }
                }

                unneeded_fields.Reverse();// Remove from the end to avoid index shifting
                Msg("creating stream of old data");
                System.IO.Stream data = await Engine.Current.LocalDB.TryOpenAsset(((StaticAnimationProvider)__instance.Clip.Target).URL);
                Msg("setting anim data to stream of old data");
                AnimX animation = new AnimX(data, false);
                Msg("cleaning animation data");
                //yeet the unused fields since we don't need them
                __instance.World.RunSynchronously(() =>
                {
                    foreach (int i in unneeded_fields)
                    {
                        __instance.Fields.RemoveAt(i);
                    }
                });
                foreach (int i in unneeded_fields)
                {
                    animation.RemoveTrackAt(i);
                }
                
                
                Msg("Saving file");
                Engine.Current.LocalDB.SaveAssetAsync(animation).ContinueWith(task =>
                {
                    
                    __instance.RunSynchronously(() =>
                    {
                        Msg("creating new static provider");
                        StaticAnimationProvider hello = (__instance.Clip.Target as FrooxEngine.Component).Slot.AttachComponent<StaticAnimationProvider>();
                        Msg("setting URI");
                        hello.URL.Value = task.Result;
                        __instance.Clip.Target = hello;
                    });
                });

                //Hope it works!
            }
        }


    }
}