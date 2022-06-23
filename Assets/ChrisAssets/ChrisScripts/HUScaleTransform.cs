public static class HUScaleTransform 
{
    /// <summary>
    /// Takes a known value to a specific scale, and places it in a [0,1] distribution depending on the min and max values of that scale
    /// </summary>
    public static float NormalizedValue(float value_to_normalize, float minimum, float maximum)
    {
        return (value_to_normalize - minimum) / (maximum-minimum);
    }

    /// <summary>
    /// Takes a known value to a specific scale, and places it in a [0,1] distribution depending on the min and max values of that scale
    /// </summary>
    public static float NormalizedValue(int value_to_normalize, int minimum, int maximum)
    {
        return (float)(value_to_normalize - minimum) / (maximum - minimum);
    }

    /// <summary>
    /// Takes a known value to a specific scale, and places it in a [0,1] distribution depending on the min and max values of that scale
    /// </summary>
    public static float NormalizedValue(float value_to_normalize, int minimum, int maximum)
    {
        return (value_to_normalize - minimum) / (maximum - minimum);
    }

    /// <summary>
    /// Turns a [0,1] normalized number into its original scale depending the scale's max and min values
    /// </summary>
    public static float ReverseNormalization(float value_to_denormalize, float minimum, float maximum)
    {
        
        return (value_to_denormalize*(maximum - minimum)) + minimum;
    }
}
