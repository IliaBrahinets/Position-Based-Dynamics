using System;
using System.Collections.Generic;

using Common.Mathematics.LinearAlgebra;

namespace PositionBasedDynamics.Sources
{

    public abstract class ParticleSource
    {
        public int NumParticles { get { return Positions.Count; } }

        public IList<Vector3f> Positions { get; protected set; }

        public float Spacing { get; private set; }

        public float Diameter {  get { return Spacing * 2.0f; } }

        public ParticleSource(float spacing)
        {
            Spacing = spacing;
        }

    }

}