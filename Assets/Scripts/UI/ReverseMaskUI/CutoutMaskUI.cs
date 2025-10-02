using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.UI.Extensions; // not required; just UI namespace

/// Punches a hole in an overlay by rendering where the stencil != mask.
[RequireComponent(typeof(Image))]
public class CutoutMaskUI : Image
{
    protected override void OnEnable()
    {
        base.OnEnable();
        // force a rebuild so the stencil chain is recomputed every time
        SetMaterialDirty();
        SetVerticesDirty();
        Canvas.ForceUpdateCanvases();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        // Unity will clean up StencilMaterial entries automatically when material changes,
        // but SetMaterialDirty helps de-ref the previous modified material sooner.
        SetMaterialDirty();
    }

    // This is the recommended hook for UI stencil work (not materialForRendering).
    public override Material GetModifiedMaterial(Material baseMaterial)
    {
        var toReturn = base.GetModifiedMaterial(baseMaterial);
        // Determine the stencil depth for this element
        int stencilDepth = MaskUtilities.GetStencilDepth(transform, MaskUtilities.FindRootSortOverrideCanvas(transform));
        if (stencilDepth <= 0)
            return toReturn;

        // Add stencil ops that make this graphic draw where stencil != ref
        var mat = StencilMaterial.Add(
            toReturn,
            (1 << stencilDepth) - 1,              // stencilRef
            StencilOp.Keep,                       // pass
            CompareFunction.NotEqual,             // *** the key: inverse!
            ColorWriteMask.All,                   // color mask
            (1 << stencilDepth) - 1,              // readMask
            0                                     // writeMask
        );
        return mat;
    }
}
