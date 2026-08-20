using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Airfoil : MonoBehaviour
{
    public float thicknessRatio = 0.12f;
    public ReynoldsPolar[] polars;

    public AeroCoefficients Sample(float alpha, float reynolds)
    {
        if (polars != null && polars.Length > 0)
        {
            ReynoldsPolar upper = polars[polars.Length - 1];
            ReynoldsPolar lower = polars[0];

            if (polars.Length == 1)
            {
                return polars[0].SampleAlpha(alpha);
            }
            else if (reynolds >= upper.reynolds)
            {
                return upper.SampleAlpha(alpha);
            }
            else if (reynolds <= lower.reynolds)
            {
                return lower.SampleAlpha(alpha);
            }
            else
            {
                for (int i = 0; i < polars.Length - 1; i++)
                {
                    upper = polars[i + 1];
                    lower = polars[i];

                    if (upper.reynolds >= reynolds && reynolds >= lower.reynolds)
                    {
                        break;
                    }
                }

                AeroCoefficients upperCoeffs = upper.SampleAlpha(alpha);
                AeroCoefficients lowerCoeffs = lower.SampleAlpha(alpha);

                float t = Mathf.InverseLerp(lower.reynolds, upper.reynolds, reynolds);
                return AeroCoefficients.Lerp(lowerCoeffs, upperCoeffs, t);
            }
        }
        else
        {
            Debug.LogWarning("messed up again big nerd");
            return default;
        }
    }


    [Serializable]
    public struct AeroCoefficients
    {
        public float cl;
        public float cd;
        public float cm;

        public AeroCoefficients(float cl, float cd, float cm)
        {
            this.cl = cl;
            this.cd = cd;
            this.cm = cm;
        }

        public static AeroCoefficients Lerp(AeroCoefficients a, AeroCoefficients b, float t)
        {
            float returnCl = Mathf.Lerp(a.cl, b.cl, t);
            float returnCd = Mathf.Lerp(a.cd, b.cd, t);
            float returnCm = Mathf.Lerp(a.cm, b.cm, t);
            return new AeroCoefficients(returnCl, returnCd, returnCm);
        }
    }

    [Serializable]
    public struct AirfoilSample
    {
        public float alpha;
        public float cl;
        public float cd;
        public float cm;

        public AirfoilSample(float alpha, float cl, float cd, float cm)
        {
            this.alpha = alpha;
            this.cl = cl;
            this.cd = cd;
            this.cm = cm;
        }

        public AeroCoefficients ToCoefficients()
        {
            return new AeroCoefficients(cl, cd, cm);
        }
    }

    [Serializable]
    public class ReynoldsPolar
    {
        public float reynolds;
        public AirfoilSample[] samples;
        
        public AeroCoefficients SampleAlpha(float alpha)
        {
            if (samples != null && samples.Length > 0)
            {
                alpha = Mathf.Clamp(alpha, samples[0].alpha, samples[samples.Length - 1].alpha);
                for (int i = 0; i < samples.Length - 1; i++)
                {
                    AirfoilSample upper = samples[i + 1];
                    AirfoilSample lower = samples[i];

                    if (alpha >= lower.alpha && alpha <= upper.alpha)
                    {
                        float t = Mathf.InverseLerp(lower.alpha, upper.alpha, alpha);
                        float returnCl = Mathf.Lerp(lower.cl, upper.cl, t);
                        float returnCd = Mathf.Lerp(lower.cd, upper.cd, t);
                        float returnCm = Mathf.Lerp(lower.cm, upper.cm, t);
                        return new AeroCoefficients(returnCl, returnCd, returnCm);
                    }
                }
                return samples[samples.Length - 1].ToCoefficients();
            }
            else
            {
                Debug.LogError("sample not found big nerd");
                return default;
            }
        }
    }
}
