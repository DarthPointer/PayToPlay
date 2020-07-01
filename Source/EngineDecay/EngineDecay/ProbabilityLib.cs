using System;


namespace EngineDecay
{
    public static class ProbabilityLib
    {
        public static float UltraExponentialRandom(float a, float argOf_1)                                  //see "Some_Thoughts_On_Engine_Failures.docx"
        {
            float r = (float)Math.Log(1 / a * Math.Log(100 * Math.Exp(a) - 99)) / (float)Math.Log(argOf_1);
            float k = 0.01f / ((float)Math.Exp(a) - 1);
            float F = UnityEngine.Random.Range(0f, 1f);                                                     //everything looks good for the initial implementation but I would like to know more about behaviour of this function
            return (float)Math.Pow((1 / a * Math.Log(F / k + 1)), 1 / r);
        }

        public static float ATangentRandom(float r, float t1)                                               //see "Some_Thoughts_On_Engine_Failures.docx"
        {
            float k = (float)(Math.Pow(Math.Tan(0.005 * Math.PI), 1f/8f) * Math.Pow(10f, 5f/8f) / t1);
            
            return (float)(Math.Pow(Math.Tan(UnityEngine.Random.Range(0f, 1f) * Math.PI/2f), 1f/r) * Math.Pow(10f, 1f - 3f/r) / k);
        }

        public static float ATangentCumulativePercentArg(float r, float t1)                                 //tells t(Fr = 0.01) assuming t(F8 = 0.01) = t1
        {
            float k = (float)(Math.Pow(Math.Tan(0.005 * Math.PI), 1f / 8f) * Math.Pow(10f, 5f / 8f) / t1);

            return (float)(Math.Pow(Math.Tan(0.005 * Math.PI), 1f / r) * Math.Pow(10f, 1f - 3f / r) / k);
        }
    }
}