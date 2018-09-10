using System;
using System.Collections.Generic;

using Common.Mathematics.LinearAlgebra;

using PositionBasedDynamics.Bodies;

namespace PositionBasedDynamics.Collisions
{

    internal class BodyPlaneContact3d : CollisionContact3d
    {

        private Body3d Body0;

        private int i0;

        private Vector3f Normal;

        private float Distance;

        internal BodyPlaneContact3d(Body3d body0, int i0, Vector3f normal, float dist)
        {
            Body0 = body0;
            this.i0 = i0;

            Normal = normal;
            Distance = dist;
        }

        internal override void ResolveContact(float di)
        {
            float d = Vector3f.Dot(Normal, Body0.Predicted[i0]) + Distance - Body0.ParticleRadius;

            if (d < 0.0)
            {
                Vector3f delta = Normal * -d * di;
                Body0.Positions[i0] += delta;
                Body0.Predicted[i0] += delta;
            }
        }

    }

}