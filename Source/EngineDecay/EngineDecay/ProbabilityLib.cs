using System;

public static class ProbabilityLib
{
    public static float UltraExponentialRandom(float a, float argOf_1)                                  //see "Some_Thoughts_On_Engine_Failures.docx"
    {
        float r = (float)Math.Log(1 / a * Math.Log(100 * Math.Exp(a) - 99))/(float)Math.Log(argOf_1);
        float k = 0.01f / ((float)Math.Exp(a) - 1);
        float F = UnityEngine.Random.Range(0f, 1f);                                                     //everything looks good for the initial implementation but I would like to know more about behaviour of this function
        return (float)Math.Pow((1 / a * Math.Log(F / k + 1)), 1 / r);
    }
}