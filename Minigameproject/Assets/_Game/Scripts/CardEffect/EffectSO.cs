using UnityEngine;

public abstract class EffectSO : ScriptableObject
{
    public abstract void Apply(EffectContext context);

    public virtual void Preview(EffectContext context, EffectPreviewResult result)
    {
    }
}