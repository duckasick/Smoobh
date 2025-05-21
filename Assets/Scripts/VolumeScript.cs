using UnityEngine;
using UnityEngine.Rendering;

public class VolumeScript : MonoBehaviour
{
    public float vignetteIntensity;

    public UnityEngine.Rendering.Volume volume;         //just using this for testing
    public UnityEngine.Rendering.VolumeProfile volumeProfile;
    public UnityEngine.Rendering.Universal.Vignette vignette;

    void Start()
    {
        UnityEngine.Rendering.VolumeProfile volumeProfile = GetComponent<UnityEngine.Rendering.Volume>()?.profile;
        if (!volumeProfile) throw new System.NullReferenceException(nameof(UnityEngine.Rendering.VolumeProfile));

        if (!volumeProfile.TryGet(out vignette)) throw new System.NullReferenceException(nameof(vignette));

        vignette.intensity.Override(0.9f);
    }

    // Update is called once per frame
    void Update()
    {
        vignette.intensity.Override(vignetteIntensity);
    }
}
