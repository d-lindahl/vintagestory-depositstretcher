using HarmonyLib;
using System;
using System.Reflection;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.ServerMods;

namespace depositstretcher
{
    public class Core : ModSystem
    {
        private const string ConfigFileName = "depositstretcherConfig.json";
        private Harmony harmony;

        public override void Start(ICoreAPI api)
        {
            try
            {
                DepositStretcherConfig depositStretcherConfig = api.LoadModConfig<DepositStretcherConfig>(ConfigFileName);
                if (depositStretcherConfig != null)
                {
                    api.Logger.Notification("Mod Config successfully loaded.");
                    DepositStretcherConfig.Current = depositStretcherConfig;
                }
                else
                {
                    api.Logger.Notification("No Mod Config specified. Falling back to default settings");
                    DepositStretcherConfig.Current = DepositStretcherConfig.GetDefault();
                }
            }
            catch
            {
                DepositStretcherConfig.Current = DepositStretcherConfig.GetDefault();
                api.Logger.Error("Failed to load custom mod configuration. Falling back to default settings!");
            }
            finally
            {
                api.StoreModConfig<DepositStretcherConfig>(DepositStretcherConfig.Current, ConfigFileName);
            }

            this.harmony = new Harmony("quaan.depositstretcher");
            this.harmony.PatchAll(Assembly.GetExecutingAssembly());
            
        }

        public override void Dispose()
        {
            this.harmony.UnpatchAll(this.harmony.Id);
            base.Dispose();
        }
    }

    public class DepositStretcherConfig
    {
        public static DepositStretcherConfig Current { get; set; }

        public int baseHeight;

        public static DepositStretcherConfig GetDefault()
        {
            return new DepositStretcherConfig()
            {
                baseHeight = 256
            };
        }
    }

    public class DepositStretcherCalculation
    {
        public static void PatchMethod(DiscDepositGenerator __instance)
        {
            FieldInfo hereThicknessField = __instance.GetType().GetField("hereThickness", BindingFlags.Instance | BindingFlags.NonPublic);
            if ((int)hereThicknessField.GetValue(__instance) > 0)
                hereThicknessField.SetValue(__instance, ((int)(int)hereThicknessField.GetValue(__instance) * (int)Math.Round(Math.Max(1, __instance.Api.WorldManager.MapSizeY / (float)DepositStretcherConfig.Current.baseHeight))));
        }
    }

    [HarmonyPatch(typeof(FollowSealevelDiscGenerator))]
    public class FollowSealevelDiscGeneratorPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("loadYPosAndThickness", new Type[] { typeof(IMapChunk), typeof(int), typeof(int), typeof(BlockPos), typeof(double) })]
        public static void StretchDepositHeight(DiscDepositGenerator __instance, IMapChunk heremapchunk, int lx, int lz, BlockPos pos, double distanceToEdge)
        {
            DepositStretcherCalculation.PatchMethod(__instance);
        }
    }

    [HarmonyPatch(typeof(FollowSurfaceDiscGenerator))]
    public class FollowSurfaceDiscGeneratorPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("loadYPosAndThickness", new Type[] { typeof(IMapChunk), typeof(int), typeof(int), typeof(BlockPos), typeof(double) })]
        public static void StretchDepositHeight(DiscDepositGenerator __instance, IMapChunk heremapchunk, int lx, int lz, BlockPos pos, double distanceToEdge)
        {
            DepositStretcherCalculation.PatchMethod(__instance);
        }
    }

    [HarmonyPatch(typeof(FollowSurfaceBelowDiscGenerator))]
    public class FollowSurfaceBelowDiscGeneratorPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("loadYPosAndThickness", new Type[] { typeof(IMapChunk), typeof(int), typeof(int), typeof(BlockPos), typeof(double) })]
        public static void StretchDepositHeight(DiscDepositGenerator __instance, IMapChunk heremapchunk, int lx, int lz, BlockPos targetPos, double distanceToEdge)
        {
            DepositStretcherCalculation.PatchMethod(__instance);
        }
    }

    [HarmonyPatch(typeof(AnywhereDiscGenerator))]
    public class AnywhereDiscGeneratorPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("loadYPosAndThickness", new Type[] { typeof(IMapChunk), typeof(int), typeof(int), typeof(BlockPos), typeof(double) })]
        public static void StretchDepositHeight(DiscDepositGenerator __instance, IMapChunk heremapchunk, int lx, int lz, BlockPos targetPos, double distanceToEdge)
        {
            DepositStretcherCalculation.PatchMethod(__instance);
        }
    }
}
