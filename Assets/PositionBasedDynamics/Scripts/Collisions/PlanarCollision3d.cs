using System;
using System.Collections.Generic;

using Common.Mathematics.LinearAlgebra;

using PositionBasedDynamics.Bodies;

namespace PositionBasedDynamics.Collisions
{
    public class PlanarCollision3d : Collision3d
    {

        private Vector3f Normal { get; set; }

        private float Distance { get; set; }

        public PlanarCollision3d(Vector3f normal, float distance)
        {
            Normal = normal.Normalized;

            Distance = distance;
        }

        internal override void FindContacts(IList<Body3d> bodies, List<CollisionContact3d> contacts)
        {

            for (int j = 0; j < bodies.Count; j++)
            {
                Body3d body = bodies[j];

                int numParticles = body.NumParticles;
                float radius = body.ParticleRadius;

                for (int i = 0; i < numParticles; i++)
                {
                    float d = Vector3f.Dot(Normal, body.Predicted[i]) + Distance - radius;

                    if (d < 0.0)
                        contacts.Add(new BodyPlaneContact3d(body, i, Normal, Distance));
                }
            }

        }

    }
}