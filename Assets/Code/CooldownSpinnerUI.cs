using UnityEngine;
using UnityEngine.UI;

public class CooldownSpinnerUI : MonoBehaviour
{
    [SerializeField] FlashlightBeam attack;
    [SerializeField] Image fillImage;

    void Update()
    {
        if (!attack || !fillImage) return;
        float remain = attack.CooldownRemaining;
        float dur = Mathf.Max(attack.CooldownDuration, 0.0001f);
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Radial360;
        if (remain > 0f)
            fillImage.fillAmount = 1f - (remain / dur);
        else
            fillImage.fillAmount = 0f;
    }
}
