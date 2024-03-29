﻿#region File Description
//-----------------------------------------------------------------------------
// Triangle.cs
//
// Author: JCBDigger
// URL: http://Games.DiscoverThat.co.uk
//-----------------------------------------------------------------------------
// Used for collision
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using AssetData;
#endregion

namespace Engine
{
    // Have a guess what this is.
    // It is used for collision testing.
    public class Triangle
    {
        public Vector3[] Vertex;

        public Triangle()
        {
            Vertex = new Vector3[3];
            Vertex[0] = Vector3.Zero;
            Vertex[1] = Vector3.Zero;
            Vertex[2] = Vector3.Zero;
        }

        public Triangle(Vector3 a, Vector3 b, Vector3 c)
        {
            Vertex = new Vector3[3];
            Vertex[0] = a;
            Vertex[1] = b;
            Vertex[2] = c;
        }

        public Vector3 A
        {
            get { return Vertex[0]; }
        }

        public Vector3 B
        {
            get { return Vertex[1]; }
        }

        public Vector3 C
        {
            get { return Vertex[2]; }
        }

        public Vector3[] GetCorners()
        {
            return Vertex;
        }

        public void Resize(Vector3 a, Vector3 b, Vector3 c)
        {
            Vertex[0] = a;
            Vertex[1] = b;
            Vertex[2] = c;
        }

        public BoundingBox CalculateBounds()
        {
            Vector3 min = Vector3.One * float.MaxValue;
            Vector3 max = Vector3.One * float.MinValue;

            for (int i = 0; i < 3; i++)
            {
                // Get minimum of each vertex
                Vector3.Min(ref min, ref Vertex[i], out min);
                // Get maximum of each vertex
                Vector3.Max(ref max, ref Vertex[i], out max);
            }

            return new BoundingBox(min, max);
        }

        /// <summary>
        /// How many of the triangles corners are inside the bounding box
        /// </summary>
        public int PointsInside(BoundingBox box)
        {
            int count = 0;
            for (int i = 0; i < Vertex.Length; i++)
            {
                if (box.Contains(Vertex[i]) != ContainmentType.Disjoint)
                {
                    count++;
                }
            }
            return count;
        }

        // Return the position of the corner nearest to the point specified
        public Vector3 NearestCorner(Vector3 point)
        {
            float closest = -1.0f;
            int nearest = 0;
            for (int j = 0; j < Vertex.Length; j++)
            {
                // Which corner of the triangle is closest to position
                float distSq = Vector3.DistanceSquared(Vertex[j], point);
                if (closest < 0 ||
                    distSq < closest)
                {
                    // We are the first or the closest corner tested
                    closest = distSq;
                    nearest = j;
                }
            }
            return Vertex[nearest];
        }

        /// <summary>
        /// Return the start and end indices of the longest edge
        /// </summary>
        public void GetLongestEdge(out int Start, out int End)
        {
            Start = 2;
            End = 0;
            float length = (Vertex[End] - Vertex[Start]).LengthSquared();
            for (int i = 0; i < Vertex.Length - 1; i++)
            {
                if ((Vertex[i + 1] - Vertex[i]).LengthSquared() > length)
                {
                    Start = i + 1;
                    End = i;
                    length = (Vertex[End] - Vertex[Start]).LengthSquared();
                }
            }
        }

        /// <summary>
        /// Return the index of the corner not specified
        /// </summary>
        public int GetOtherIndex(int a, int b)
        {
            if (a + b == 1)
            {
                return 2;
            }
            else if (a + b == 2)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Return the index of any corner except the one specified
        /// </summary>
        public int GetAnyOtherIndex(int a)
        {
            if (a == 0)
            {
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// Return the mid point of an edge given the indices of the start and end corners.
        /// </summary>
        public Vector3 EdgeMidPoint(int Start, int End)
        {
            float midX = Vertex[Start].X + ((Vertex[End].X - Vertex[Start].X) * 0.5f);
            float midY = Vertex[Start].Y + ((Vertex[End].Y - Vertex[Start].Y) * 0.5f);
            float midZ = Vertex[Start].Z + ((Vertex[End].Z - Vertex[Start].Z) * 0.5f);
            return new Vector3(midX, midY, midZ);
        }

        /// <summary>
        /// Return a new triangle based on two of the vertices of this triangle
        /// plus some other point.
        /// </summary>
        /// <param name="a">Index of one existing corner</param>
        /// <param name="b">Index of another existing corner</param>
        /// <param name="pointC">The new corner</param>
        public Triangle SplitTriangle(int a, int b, Vector3 pointC)
        {
            return new Triangle(Vertex[a], Vertex[b], pointC);
        }

        /// <summary>
        /// Return the mid point of the triangle
        /// </summary>
        public Vector3 CentrePoint()
        {
            Vector3 midEdge = EdgeMidPoint(0, 1);
            float midX = midEdge.X + ((Vertex[2].X - midEdge.X) * 0.5f);
            float midY = midEdge.Y + ((Vertex[2].Y - midEdge.Y) * 0.5f);
            float midZ = midEdge.Z + ((Vertex[2].Z - midEdge.Z) * 0.5f);
            return new Vector3(midX, midY, midZ);
        }

        /// <summary>
        /// Return true if any edge of the bounding box is larger than the size specified
        /// </summary>
        public bool IsLargerThan(float MaxBoundSize)
        {
            // Calculate the bounding box on the fly
            BoundingBox bounds = CalculateBounds();
            float boxSizeX = bounds.Max.X - bounds.Min.X;
            float boxSizeY = bounds.Max.Y - bounds.Min.Y;
            float boxSizeZ = bounds.Max.Z - bounds.Min.Z;
            if (boxSizeX > MaxBoundSize || boxSizeY > MaxBoundSize || boxSizeZ > MaxBoundSize)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// A unit length vector at right angles to the plane of the triangle
        /// </summary>
        public void Normal(out Vector3 normal)
        {
            normal = Vector3.Zero;
            Vector3 side1 = B - A;
            Vector3 side2 = C - A;
            // Avoid NaN errors
            normal = MoreMaths.SafeNormalize(Vector3.Cross(side1, side2));
        }

        /// <summary>
        /// Get a normal that faces towards the point specified (faces out)
        /// </summary>
        public void NormalFacing(ref Vector3 point, out Vector3 normal)
        {
            Normal(out normal);
            // The direction from the point towards any corner of the triangle
            Vector3 direction = A - point; ;
            // Roughly facing the same way
            if (Vector3.Dot(normal, direction) > 0)
            {
                // Same direction therefore invert the normal to face away from the direction
                // to face the point
                Vector3.Multiply(ref normal, -1.0f, out normal);
            }
        }

        /// <summary>
        /// Get a normal that faces away from the point specified (faces in)
        /// </summary>
        public void InverseNormal(ref Vector3 point, out Vector3 inverseNormal)
        {
            Normal(out inverseNormal);
            // The direction from any corner of the triangle to the point
            Vector3 inverseDirection = point - A; ;
            // Roughly facing the same way
            if (Vector3.Dot(inverseNormal, inverseDirection) > 0)
            {
                // Same direction therefore invert the normal to face away from the direction
                // to face the point
                Vector3.Multiply(ref inverseNormal, -1.0f, out inverseNormal);
            }
        }

        // Gets the highest Y value of any of the points
        public float GetHighestPoint()
        {
            float result = float.MinValue;
            for (int i = 0; i < Vertex.Length; i++)
            {
                if (Vertex[i].Y > result)
                {
                    result = Vertex[i].Y;
                }
            }
            return result;
        }

        // Method to check whether a ray intersects a triangle. 
        // This uses the algorithm developed by Tomas Moller and Ben Trumbore, which 
        // was published in the Journal of Graphics Tools, volume 2, "Fast, Minimum 
        // Storage Ray-Triangle Intersection".
        // 
        // From the Creators Club TrianglePicking sample RayIntersectsTriangle(...)
        // See: http://creators.xna.com/en-GB/sample/pickingtriangle

        // Returns the distance from the origin of the ray to the intersection with 
        // the triangle, null if no intersect and negative if behind.
        public void Intersects(ref Ray ray, out float? distance)
        {
            // Set the Distance to indicate no intersect
            distance = null;
            // Compute vectors along two edges of the triangle.
            Vector3 edge1, edge2;

            Vector3.Subtract(ref Vertex[2], ref Vertex[1], out edge1);
            Vector3.Subtract(ref Vertex[0], ref Vertex[1], out edge2);

            // Compute the determinant.
            Vector3 directionCrossEdge2;
            Vector3.Cross(ref ray.Direction, ref edge2, out directionCrossEdge2);

            float determinant;
            Vector3.Dot(ref edge1, ref directionCrossEdge2, out determinant);

            // If the ray is parallel to the triangle plane, there is no collision.
            if (determinant > -float.Epsilon && determinant < float.Epsilon)
            {
                return;
            }

            float inverseDeterminant = 1.0f / determinant;

            // Calculate the U parameter of the intersection point.
            Vector3 distanceVector;
            Vector3.Subtract(ref ray.Position, ref Vertex[1], out distanceVector);

            float triangleU;
            Vector3.Dot(ref distanceVector, ref directionCrossEdge2, out triangleU);
            triangleU *= inverseDeterminant;

            // Make sure it is inside the triangle.
            if (triangleU < 0 || triangleU > 1)
            {
                return;
            }

            // Calculate the V parameter of the intersection point.
            Vector3 distanceCrossEdge1;
            Vector3.Cross(ref distanceVector, ref edge1, out distanceCrossEdge1);

            float triangleV;
            Vector3.Dot(ref ray.Direction, ref distanceCrossEdge1, out triangleV);
            triangleV *= inverseDeterminant;

            // Make sure it is inside the triangle.
            if (triangleV < 0 || triangleU + triangleV > 1)
            {
                return;
            }

            // == By here the ray must be inside the triangle

            // Compute the distance along the ray to the triangle.
            float length = 0;
            Vector3.Dot(ref edge2, ref distanceCrossEdge1, out length);
            distance = length * inverseDeterminant;
        }

        // Based on existing code available on the Internet
        // See: http://realtimerendering.com/intersections.html
        // Converted to C# for XNA

        /// <summary>
        /// This is an expensive test.
        /// </summary>
        public void Intersects(ref BoundingSphere sphere, out bool result)
        {
            result = false;
            // First check if any corner point is inside the sphere
            // This is necessary because the other tests can easily miss
            // small triangles that are fully inside the sphere.
            if (sphere.Contains(A) != ContainmentType.Disjoint ||
                sphere.Contains(B) != ContainmentType.Disjoint ||
                sphere.Contains(C) != ContainmentType.Disjoint)
            {
                // A point is inside the sphere
                result = true;
                return;
            }
            // Test the edges of the triangle using a ray
            // If any hit then check the distance to the hit is less than the length of the side
            // The distance from a point of a small triangle inside the sphere coule be longer
            // than the edge of the small triangle, hence the test for points inside above.
            Vector3 side = B - A;
            // Important:  The direction of the ray MUST
            // be normalised otherwise the resulting length 
            // of any intersect is wrong!
            Ray ray = new Ray(A, Vector3.Normalize(side));
            float distSq = 0;
            float? length = null;
            sphere.Intersects(ref ray, out length);
            if (length != null)
            {
                distSq = (float)length * (float)length;
                if (length > 0 && distSq < side.LengthSquared())
                {
                    // Hit edge
                    result = true;
                    return;
                }
            }
            // Stay at A and change the direction to C
            side = C - A;
            ray.Direction = Vector3.Normalize(side);
            length = null;
            sphere.Intersects(ref ray, out length);
            if (length != null)
            {
                distSq = (float)length * (float)length;
                if (length > 0 && distSq < side.LengthSquared())
                {
                    // Hit edge
                    result = true;
                    return;
                }
            }
            // Change to corner B and edge to C
            side = C - B;
            ray.Position = B;
            ray.Direction = Vector3.Normalize(side);
            length = null;
            sphere.Intersects(ref ray, out length);
            if (length != null)
            {
                distSq = (float)length * (float)length;
                if (length > 0 && distSq < side.LengthSquared())
                {
                    // Hit edge
                    result = true;
                    return;
                }
            }
            // If we get this far we are not touching the edges of the triangle
            
            // Calculate the InverseNormal of the triangle from the centre of the sphere
            // Do a ray intersection from the centre of the sphere to the triangle.
            // If the triangle is too small the ray could miss a small triangle inside
            // the sphere hence why the points were tested above.
            ray.Position = sphere.Center;
            // This will always create a vector facing towards the triangle from the 
            // ray starting point.
            InverseNormal(ref ray.Position, out side);
            ray.Direction = side;
            Intersects(ref ray, out length);
            if (length != null && length > 0 && length < sphere.Radius)
            {
                // Hit the surface of the triangle
                result = true;
                return;
            }
            // Only if we get this far have we missed the triangle
            result = false;
        }

        // Return the centre point of the circular section formed by 
        // the intersection of a sphere with the triangle.  
        // Returns null if no intersect
        public Vector3? IntersectSectionCentre(BoundingSphere sphere)
        {
            // Calculate the InverseNormal of the triangle from the centre of the sphere
            // Do a ray intersection from the centre of the sphere to the triangle.
            // This will always create a vector facing towards the triangle from the 
            // ray starting point.
            Vector3 direction = Vector3.Zero;
            InverseNormal(ref sphere.Center, out direction);
            Ray ray = new Ray(sphere.Center, direction);
            float? length = null;
            Intersects(ref ray, out length);
            if (length != null && length > 0 && length < sphere.Radius)
            {
                // Hit the surface of the triangle
                return sphere.Center + (direction * length);
            }
            return null;
        }

        /// <summary>
        /// The nearest point on the sphere to the point specified where the 
        /// sphere and triangle intersect.
        /// Returns null if there is no intersect
        /// </summary>
        public Vector3? IntersectNearestOnPlane(BoundingSphere sphere, int point)
        {
            // Find the nearest point on the plane of the triangle that 
            // intersects the sphere
            Vector3? centre = IntersectSectionCentre(sphere);
            if (centre == null)
            {
                // No intersect
                return null;
            }
            // Test from the point to the centre
            Vector3 line = (Vector3)centre - Vertex[point];
            // Important:  The direction of the ray MUST
            // be normalised otherwise the resulting length 
            // of any intersect is wrong!
            Ray ray = new Ray(Vertex[point], Vector3.Normalize(line));
            float distSq = 0;
            float? length = null;
            sphere.Intersects(ref ray, out length);
            if (length != null)
            {
                distSq = (float)length * (float)length;
                if (length > 0 && distSq < line.LengthSquared())
                {
                    // The line hits the edge of the sphere
                    // so save the point of impact.
                    return ray.Position + (ray.Direction * (float)length);
                }
            }
            return null;
        }


        /// <summary>
        /// Returns a list of the extremities of the triangle that 
        /// are fully within the sphere.
        /// This is sufficient for collission testing but is not
        /// intended to be an accurate cross section.
        /// Triangles larger than the sphere return points that touch
        /// the edges of the bounding sphere where the triangle sides
        /// penetrates the sphere or if all three points are outside  
        /// tey return the nearest point on the sphere which is sufficient 
        /// to get an approximate section of the sphere.
        /// Returns an empty list if the triangle does not intersect the sphere.
        /// This is an expensive test.  Use at design time only.
        /// </summary>
        public List<Vector3> PointsInsideSphere(BoundingSphere sphere)
        {
            List<Vector3> result = new List<Vector3>();

            // Test which if any points are inside the sphere
            for (int p = 0; p < Vertex.Length; p++)
            {
                if (sphere.Contains(Vertex[p]) != ContainmentType.Disjoint)
                {
                    // == Inside the sphere
                    result.Add(Vertex[p]);
                }
                else
                {
                    // == Outside the sphere
                    // Test both side eminating from the point to see where they 
                    // intersect the sphere, if at all.
                    int other = GetAnyOtherIndex(p);
                    Vector3 side = Vertex[other] - Vertex[p];
                    // Important:  The direction of the ray MUST
                    // be normalised otherwise the resulting length 
                    // of any intersect is wrong!
                    Ray ray = new Ray(Vertex[p], Vector3.Normalize(side));
                    float distSq = 0;
                    float? length = null;
                    sphere.Intersects(ref ray, out length);
                    if (length != null)
                    {
                        distSq = (float)length * (float)length;
                        if (length > 0 && distSq < side.LengthSquared())
                        {
                            // The side of the triangle hits the edge of the sphere
                            // so save the point of impact.
                            result.Add(ray.Position + (ray.Direction * (float)length));
                        }
                    }
                    else
                    {
                        // Side is fully outside the sphere
                        Vector3? impact = IntersectNearestOnPlane(sphere, p);
                        if (impact != null)
                        {
                            result.Add((Vector3)impact);
                        }
                    }
                    // Get the other corner
                    other = GetOtherIndex(other, p);
                    // Same point but the other edge
                    side = Vertex[other] - Vertex[p];
                    ray.Direction = Vector3.Normalize(side);
                    length = null;
                    sphere.Intersects(ref ray, out length);
                    if (length != null)
                    {
                        distSq = (float)length * (float)length;
                        if (length > 0 && distSq < side.LengthSquared())
                        {
                            // The side of the triangle hits the edge of the sphere
                            // so save the point of impact.
                            result.Add(ray.Position + (ray.Direction * (float)length));
                        }
                    }
                    else
                    {
                        // Side is fully outside the sphere
                        Vector3? impact = IntersectNearestOnPlane(sphere, p);
                        if (impact != null)
                        {
                            result.Add((Vector3)impact);
                        }
                    }
                }
            }
            return result;
        }


    }
}
