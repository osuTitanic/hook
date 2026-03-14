using System.Collections.Generic;

namespace TitanicHookManaged.Framework;

/// <summary>
/// Manager for patches
/// </summary>
public static class PatchManager
{
    /// <summary>
    /// Applies a patch
    /// </summary>
    /// <param name="patch">Instance of the patch</param>
    /// <returns>Whether the patch was applied successfully</returns>
    public static bool Apply(TitanicPatch patch)
    {
        // TODO: Add checks to not apply the same patch 2 times
        patch.Patch();
        AppliedPatches.Add(patch);
        return true;
    }

    public static List<TitanicPatch> AppliedPatches = [];
}
