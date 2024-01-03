using System;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;

namespace Haru.Reflection
{
    public abstract class APatch
    {
        public Harmony Harmony { get; private set; }
        public EPatchType Type { get; protected set; }
        public string Id { get; protected set; }
        private static ManualLogSource _logger;

        protected APatch()
        {
            _logger = Logger.CreateLogSource(nameof(APatch));
        }

        protected abstract MethodBase GetOriginalMethod();

        private HarmonyMethod GetPatchMethod()
        {
            var mi = GetType().GetMethod("Patch", BindingFlags.NonPublic | BindingFlags.Static);
            return new HarmonyMethod(mi);
        }

        public void Enable()
        {
            _logger.LogInfo($"Enabling: {Id}");

            var patch = GetPatchMethod();
            var target = GetOriginalMethod();

            Harmony = new Harmony(Id);

            if (target == null)
            {
                throw new InvalidOperationException($"{Id}: GetOriginalMethod returns null");
            }

            switch (Type)
            {
                case EPatchType.Prefix:
                    Harmony.Patch(target, prefix: patch);
                    return;

                case EPatchType.Postfix:
                    Harmony.Patch(target, postfix: patch);
                    return;

                case EPatchType.Transpile:
                    Harmony.Patch(target, transpiler: patch);
                    return;

                default:
                    throw new NotImplementedException("Patch type");
            }
        }

        public void Disable()
        {
            _logger.LogInfo($"Disabling: {Id}");
            Harmony?.Dispose();
        }
    }
}