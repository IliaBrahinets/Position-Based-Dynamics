using UnityEngine;
using System;
using System.Collections.Generic;

using Common.Mathematics.LinearAlgebra;
using Common.Geometry.Shapes;
using Common.Unity.Drawing;
using Common.Unity.Mathematics;

using PositionBasedDynamics.Solvers;
using PositionBasedDynamics.Bodies.Fluids;
using PositionBasedDynamics.Sources;
using PositionBasedDynamics.Forces;
using PositionBasedDynamics.Bodies;

namespace PositionBasedDynamics
{
    public class FluidBodyDemo : MonoBehaviour
    {

        private const float timeStep = 1.0f / 60.0f;

        private const int GRID_SIZE = 10;
        
        public Material sphereMaterial;

        public Material boundaryMaterial;

        public bool drawLines = true;

        public bool drawBoundary = true;

        private GameObject[] FluidSpheres { get; set; }

        private GameObject[] BoundarySpheres { get; set; }

        private FluidBody3d Body { get; set; }

        private FluidBoundary3d Boundary { get; set; }

        private GPUFluidSolver3d Solver { get; set; }


        private Box3f FluidBounds, OuterBounds, InnerBounds;

        void Start()
        {

            float radius = 0.25f;
            float density = 1000f;

            CreateBoundary(radius, density);
            CreateFluid(radius, density);

            Solver = new GPUFluidSolver3d(Body);
            Solver.AddForce(new GravitationalForce3d());
            
        }

        void Update()
        { 
            Solver.StepPhysics(timeStep);

            VisualizeUpdate();
        }

        void OnDestroy()
        {

            if (FluidSpheres != null)
            {
                for (int i = 0; i < FluidSpheres.Length; i++)
                {
                    DestroyImmediate(FluidSpheres[i]);
                    FluidSpheres[i] = null;
                }
            }

            if (BoundarySpheres != null)
            {
                for (int i = 0; i < BoundarySpheres.Length; i++)
                {
                    DestroyImmediate(BoundarySpheres[i]);
                    BoundarySpheres[i] = null;
                }
            }

        }

        private void OnRenderObject()
        {
            if (drawLines)
            {
                Camera camera = Camera.current;

                Matrix4x4f m = MathConverter.ToMatrix4x4f(transform.localToWorldMatrix);
                DrawLines.DrawBounds(camera, Color.red, OuterBounds, m);
                DrawLines.DrawBounds(camera, Color.red, InnerBounds, m);
                DrawLines.DrawBounds(camera, Color.blue, FluidBounds, m);

                Vector3 min = new Vector3(-GRID_SIZE, 0, -GRID_SIZE);
                Vector3 max = new Vector3(GRID_SIZE, 0, GRID_SIZE);

                DrawLines.DrawGrid(camera, Color.white, min, max, 1, transform.localToWorldMatrix);
            }
        }

        public void CreateBoundary(float radius, float density)
        {

            InnerBounds = new Box3f(-8, 4, 0, 10, -2,4);
            //new Box3f(-8, 8, 0, 10, -2, 4);
            OuterBounds = InnerBounds;

            float thickness = 1.2f;
            OuterBounds.Min -= new Vector3f(radius * 2 * thickness);
            OuterBounds.Max += new Vector3f(radius * 2 * thickness);

            ParticleSource source = new ParticlesFromBounds(radius, OuterBounds, InnerBounds);

            Boundary = new FluidBoundary3d(source, radius, density, Matrix4x4f.Identity);

            CreateBoundaryVisualize();
        }


        public void CreateFluid( float radius, float density)
        {
            //To make less particles decrease the size of the bounds or increase the radius.
            //Make sure fluid bounds fits inside the boundrys bounds.
            FluidBounds = new Box3f(-8, 0, 0, 8, -2, 2);//new Box3f(-7, 6, 1, 7, -2, 4);
           

            FluidParticlesWithConstraint source = new FluidParticlesWithConstraint(radius, FluidBounds,Boundary.NumParticles);

            System.Random rnd = new System.Random(0);

            Body = new FluidBody3d(source, radius, density, Matrix4x4f.Identity);
            Body.Dampning = 0.0f;
            Body.AddBoundry(Boundary);
            Body.RandomizePositions(rnd, radius * 0.01f);
            Body.RandomizePositionOrder(rnd);

            Debug.Log(Body.NumParticles);

            
            CreateFluidVisualize();
        }


        public void CreateBoundaryVisualize()
        {
            
            BoundarySpheres = new GameObject[Boundary.NumParticles];
            float diam = (float)Boundary.ParticleDiameter;

            for (int i = 0; i < BoundarySpheres.Length; i++)
            {
                Vector3f pos = Boundary.Positions[i];

                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);

                sphere.SetActive(drawBoundary);
                sphere.transform.parent = transform;
                sphere.transform.position = new Vector3((float)pos.x, (float)pos.y, (float)pos.z);
                sphere.transform.localScale = new Vector3(diam, diam, diam);
                sphere.GetComponent<Collider>().enabled = false;

                sphere.GetComponent<MeshRenderer>().material = boundaryMaterial;

                BoundarySpheres[i] = sphere;
            }

        }
        public void CreateFluidVisualize()
        {
     
            FluidSpheres = new GameObject[Body.NumParticles];

            float diam = (float)Body.ParticleDiameter;

            for (int i = 0; i < FluidSpheres.Length; i++)
            {
                Vector3f pos = Body.Positions[i];

                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.parent = transform;
                sphere.transform.position = new Vector3((float)pos.x, (float)pos.y, (float)pos.z);
                sphere.transform.localScale = new Vector3(diam, diam, diam);
                sphere.GetComponent<Collider>().enabled = false;

                sphere.GetComponent<MeshRenderer>().material = sphereMaterial;

                FluidSpheres[i] = sphere;
            }
        }
        public void VisualizeUpdate()
        {

            if (FluidSpheres != null)
            {
                for (int i = 0; i < FluidSpheres.Length; i++)
                {
                    Vector3f pos = Body.Positions[i];
                    FluidSpheres[i].transform.position = new Vector3((float)pos.x, (float)pos.y, (float)pos.z);
                }
            }

            if (BoundarySpheres != null)
            {
                for (int i = 0; i < BoundarySpheres.Length; i++)
                {
                    BoundarySpheres[i].SetActive(drawBoundary);

                    Vector3f pos = Boundary.Positions[i];
                    BoundarySpheres[i].transform.position = new Vector3((float)pos.x, (float)pos.y, (float)pos.z);
                }
            }

        }

    }

}
