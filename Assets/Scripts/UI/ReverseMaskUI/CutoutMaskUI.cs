/*
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class CutoutMaskUI : Image
{
    public override Material materialForRendering
    {
        get
        {
            Material material = new Material(base.materialForRendering);
            material.SetInt("_StencilComp", (int)CompareFunction.NotEqual);
            return material;
        }
    }
}
*/

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class CutoutMaskUI : Image
{
    // Let Unity build the masked material (with correct stencil ID)
    // then just change the comparison to invert it.
    public override Material GetModifiedMaterial(Material baseMaterial)
    {
        var mat = base.GetModifiedMaterial(baseMaterial);
        if (mat != null)
        {
            // Invert the mask: draw where stencil != reference
            mat.SetInt("_StencilComp", (int)CompareFunction.NotEqual);
        }
        return mat;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        // Force a proper rebind after scene loads/enables
        SetMaterialDirty();
        SetVerticesDirty();
    }
}
