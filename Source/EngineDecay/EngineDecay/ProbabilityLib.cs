using System;

public static class ProbabilityLib
{
    public static float PolynomialRandom(float exponent)                 //generates random value with distribution function = random^exponent, [0.0; 1)
    {
        return (float)Math.Pow(new Random().NextDouble(), 1 / exponent);
    }
}