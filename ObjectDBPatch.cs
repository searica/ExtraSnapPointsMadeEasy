using HarmonyLib;

namespace ExtraSnapPointsMadeEasy.Patches
{
    [HarmonyPatch]
    public class ObjectDBPatch
    {
        [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake)), HarmonyPostfix]
        public static void AfterObjectDBAwake()
        {
            if (!ZNetScene.instance)
            {
                return;
            }

            Plugin.AddExtraSnapPoints();
        }
    }


}

// itemStandSnap mod uses the following patch method instead
// Just something to keep in mind
// [HarmonyPatch]
// public class Patch
// {
// 	[HarmonyPatch(typeof(ZNetScene), "Awake"), HarmonyPostfix]
// 	public static void ZNetSceneAwakePatch()
// 	{
// 		SnapPointHelper.AddSnappoints("itemstandh", new Vector3[]
// 		{
// 			new (0f, 0f, 0f),
// 			new (0.1f, 0f, 0f),
// 			new (-0.1f, 0f, 0f),
// 			new (0.0f, 0f, 0.1f),
// 			new (0.0f, 0f, -0.1f),

// 		});
//         SnapPointHelper.AddSnappoints("itemstand", new Vector3[]
// 		{
//             new (0f, 0f, 0f),
//             new (0.22f, 0f, 0f),
//             new (-0.22f, 0f, 0f),
//             new (0.0f, 0.22f, 0f),
//             new (0.0f, -0.22f, 0f),

// 		});
//     }
// }
