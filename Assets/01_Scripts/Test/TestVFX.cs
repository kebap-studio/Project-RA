using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class TestVFX : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> visualEffectAssets;

    void Start()
    {
        foreach (var vfxObject in visualEffectAssets)
        {
            if (vfxObject)
            {
                VisualEffect vfx = vfxObject.GetComponent<VisualEffect>();
                if (vfx)
                {
                    vfx.Play();
                }
            }
        }
    }

    void Update()
    {
        // 무한재생
        foreach (var vfxObject in visualEffectAssets)
        {
            VisualEffect vfx = vfxObject.GetComponent<VisualEffect>();
            if (vfx && !vfx.HasAnySystemAwake())
            {
                vfx.Play();
            }
        }
    }
}
