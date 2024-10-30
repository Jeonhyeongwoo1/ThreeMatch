using DG.Tweening;
using System;

public static class DOTweenExtensions
{
    public static Tweener DOLong(Func<long> getter, Action<long> setter, long endValue, float duration)
    {
        // Convert long values to float for tweening
        return DOVirtual.Float(getter(), endValue, duration, value =>
        {
            setter((long)value);
        });
    }
}