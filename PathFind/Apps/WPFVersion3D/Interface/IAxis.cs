﻿using WPFVersion3D.Model;

namespace WPFVersion3D.Interface
{
    internal interface IAxis
    {
        void Locate(GraphField3D field, double distanceBetweenVertices);
    }
}