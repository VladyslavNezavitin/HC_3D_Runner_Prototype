public static class Utils
{
    public static float RowToXPosition(int row)
    {
        switch (row)
        {
            case 0: return -3f;
            default: 
            case 1: return 0f;
            case 2: return 3f;
        }
    }

    public static float ClosestRowFromXPosition(float xPosition)
    {
        if (xPosition > float.MinValue && xPosition <= -1.5f)
            return 0;
        if (xPosition > -1.5f && xPosition <= 1.5f)
            return 1;
        if (xPosition > 1.5 && xPosition <= float.MaxValue)
            return 2;

        return 0;
    }
}