using HarmonyLib;
using ResoniteModLoader;
using Elements.Core;
using FrooxEngine;
using Elements.Assets;
using System.Reflection;

namespace AnimatorAdditions
{
    public class AnimatorAdditions : ResoniteMod
    {
        public static ModConfiguration Config;


        public override string Author => "989onan";

        public override string Link => "https://github.com/989onan/AnimatorAdditions";

        public override string Name => "AnimatorAdditions";

        public override string Version => "2.0.0";


        public override void OnEngineInit()
        {
            Harmony harmony = new Harmony($"{Author}.{Name}");
            Config = GetConfiguration();
            Config.Save(true);
            Msg("patching method for audio buffers from buffer reduction");
            MethodInfo patchmethod = AccessTools.Method(typeof(AudioClamper), "BufferIntercept");
            harmony.Patch(AccessTools.Method(typeof(AudioOutputDriver), "Read"), postfix: patchmethod);
            harmony.Patch(AccessTools.Method(typeof(AudioOutputDriver), "Input_NewRawSamples"), postfix: patchmethod);
            Msg("patched method for audio buffers from buffer reduction");
        }
        


        public class AudioClamper
        {
            public static void BufferIntercept(System.Span<StereoSample> buffer)
            {



                //Msg("reading buffer");
                float num = 0f;
                for (int i = 0; i < buffer.Length; i += 2)
                {
                    num = MathX.Max(num, buffer[i].AbsoluteAmplitude);
                }
                if (num > Config.GetValue(audioclamp))
                {
                    //Msg("clamping buffer volume");
                    float max_vol = num/Config.GetValue(audioclamp);
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        buffer[i] = new StereoSample(buffer[i].left/ max_vol, buffer[i].right / max_vol);
                    }
                }


            }

        }
    }
}