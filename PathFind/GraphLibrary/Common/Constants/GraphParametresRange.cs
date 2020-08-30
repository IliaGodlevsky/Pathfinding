﻿namespace GraphLibrary.Common.Constants
{
    public static class GraphParametresRange
    {
        public static int UpperWidthValue { get; }
        public static int LowerWidthValue { get; }
        public static int UpperHeightValue { get; }
        public static int LowerHeightValue { get; }
        public static int UpperObstacleValue { get; }
        public static int LowerObstacleValue { get; }

        static GraphParametresRange()
        {
            UpperWidthValue = 100;
            LowerWidthValue = 5;
            UpperHeightValue = 100;
            LowerHeightValue = 5;
            UpperObstacleValue = 99;
            LowerObstacleValue = 0;
        }

        public static bool IsInWidthRange(int width)
        {
            return width <= UpperWidthValue 
                && width >= LowerWidthValue;
        }

        public static bool IsInHeightRange(int height)
        {
            return height <= UpperHeightValue 
                && height >= LowerHeightValue;
        }

        public static bool IsInObstacleRange(int obstaclePercent)
        {
            return obstaclePercent <= UpperObstacleValue 
                && obstaclePercent >= LowerObstacleValue;
        }

        public static bool IsRightGraphParamters(int width, int height, int obstaclePercent)
        {
            return IsInWidthRange(width) 
                && IsInHeightRange(height) 
                && IsInObstacleRange(obstaclePercent);
        }
    }
}
