using System;
using UnityEngine;
using UnityEngine.Animations;

namespace com.github.hkrn
{
    [Serializable]
    public class BlendShapeSelectProperty
    {
        [SerializeField] [NotKeyable] public bool IsAutomatic = true;
        [SerializeField] [NotKeyable] public SkinnedMeshRenderer MeshRenderer;
        [SerializeField] [NotKeyable] public string BlendShapeName;
    }
}
