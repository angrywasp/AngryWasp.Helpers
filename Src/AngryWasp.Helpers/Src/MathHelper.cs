using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace AngryWasp.Helpers
{
    public static class MathHelper
    {
        public const float Pi = MathF.PI;
        public const float TwoPi = MathF.PI * 2.0f;
        public const float PiOver2 = MathF.PI / 2.0f;
        public const float PiOver4 = MathF.PI / 4.0f;
        public const float ToRadCoefficient = MathF.PI / 180.0f;
        public const float ToDegCoefficient = 180.0f / MathF.PI;

        public static float Average(params float[] numbers)
        {
            float total = 0;

            if (numbers.Length == 0)
                return float.NaN;

            foreach (float f in numbers)
                total += f;

            return total / numbers.Length;
        }

        /// <summary>
        /// interpolates a set of values between min and max of pointCount length
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="pointCount"></param>
        /// <returns></returns>
        public static float[] GetInterpolationPoints(float min, float max, int pointCount)
        {
            float avg = Average(max - min);
            float div = avg / pointCount;

            float[] result = new float[pointCount];

            for (int i = 0; i < pointCount; i++)
                result[i] = min + (div * i);

            return result;
        }

        /// <summary>Linearly interpolates between two values.</summary>
        /// <param name="value1">Source value.</param>
        /// <param name="value2">Source value.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of value2.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Lerp(float value1, float value2, float amount) => value1 + (value2 - value1) * amount;

        /// <summary>
        /// Interpolates between two values using a cubic equation.
        /// </summary>
        /// <param name="value1">Source value.</param>
        /// <param name="value2">Source value.</param>
        /// <param name="amount">Weighting value.</param>
        /// <returns>Interpolated value.</returns>
        public static float SmoothStep(float value1, float value2, float amount)
        {
            // It is expected that 0 < amount < 1
            // If amount < 0, return value1
            // If amount > 1, return value2
            float result = Clamp(amount, 0f, 1f);
            result = Hermite(value1, 0f, value2, 0f, result);

            return result;
        }

        public static float[] GetRelativeInterpolationPoints(float min, float max, int pointCount)
        {
            float avg = Average(max - min);
            float div = avg / pointCount;

            float[] result = new float[pointCount];

            for (int i = 0; i < pointCount; i++)
                result[i] = div;

            return result;
        }

        #region Clamp

        public static void Clamp(ref int value, int min, int max)
        {
            if (value < min)
                value = min;
            else if (value > max)
                value = max;
        }

        public static void Clamp(ref float value, float min, float max)
        {
            if (value < min)
                value = min;
            else if (value > max)
                value = max;
        }

        public static float Clamp(float value, float min, float max)
        {
            if (value < min)
                return min;
            else if (value > max)
                return max;

            return value;
        }

        public static int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;
            else if (value > max)
                return max;

            return value;
        }

        #endregion

        #region Invert

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Invert(Vector2 v) => new Vector2(1.0f / v.X, 1.0f / v.Y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Invert(Vector3 v) => new Vector3(1.0f / v.X, 1.0f / v.Y, 1.0f / v.Z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Invert(Vector4 v) => new Vector4(1.0f / v.X, 1.0f / v.Y, 1.0f / v.Z, 1.0f / v.W);

        #endregion

        /// <summary>
        /// Calculates the weight of a given value between the min and max specified
        /// </summary>
        /// <param name="fValue"></param>
        /// <param name="fMin"></param>
        /// <param name="fMax"></param>
        /// <returns></returns>
        public static float ComputeWeight(float fValue, float fMin, float fMax)
        {
            float fWeight = 0.0f;

            if (fValue >= fMin && fValue <= fMax)
            {
                float fSpan = fMax - fMin;
                fWeight = fValue - fMin;

                // convert to a -1 to 1 range between min and max
                fWeight /= fSpan;
                fWeight -= 0.5f;
                fWeight *= 2.0f;

                // square result for non-linear falloff
                fWeight *= fWeight;

                // invert result
                fWeight = 1.0f - fWeight;
            }

            return fWeight;
        }

        /// <summary>
        /// calculates whether a number is above the threshold value
        /// </summary>
        /// <param name="value">the value to test</param>
        /// <param name="thresh">the threshold to test it against</param>
        /// <returns>0 if value is less that theshold, otherwise value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Threshold(float value, float thresh) => value < thresh ? 0 : value;

        /// <summary>
        /// Determines if value is powered by two.
        /// </summary>
        /// <param name="value">A value.</param>
        /// <returns><c>true</c> if <c>value</c> is powered by two; otherwise <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPowerOfTwo(int value) => (value > 0) && ((value & (value - 1)) == 0);

        /// <summary>
        /// Returns the Cartesian coordinate for one axis of a point that is defined by a given triangle and two normalized barycentric (areal) coordinates.
        /// </summary>
        /// <param name="value1">The coordinate on one axis of vertex 1 of the defining triangle.</param>
        /// <param name="value2">The coordinate on the same axis of vertex 2 of the defining triangle.</param>
        /// <param name="value3">The coordinate on the same axis of vertex 3 of the defining triangle.</param>
        /// <param name="amount1">The normalized barycentric (areal) coordinate b2, equal to the weighting factor for vertex 2, the coordinate of which is specified in value2.</param>
        /// <param name="amount2">The normalized barycentric (areal) coordinate b3, equal to the weighting factor for vertex 3, the coordinate of which is specified in value3.</param>
        /// <returns>Cartesian coordinate of the specified point with respect to the axis being used.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Barycentric(float value1, float value2, float value3, float amount1, float amount2) => value1 + (value2 - value1) * amount1 + (value3 - value1) * amount2;

        /// <summary>
        /// Performs a Catmull-Rom interpolation using the specified positions.
        /// </summary>
        /// <param name="value1">The first position in the interpolation.</param>
        /// <param name="value2">The second position in the interpolation.</param>
        /// <param name="value3">The third position in the interpolation.</param>
        /// <param name="value4">The fourth position in the interpolation.</param>
        /// <param name="amount">Weighting factor.</param>
        /// <returns>A position that is the result of the Catmull-Rom interpolation.</returns>
        public static float CatmullRom(float value1, float value2, float value3, float value4, float amount)
        {
            // Using formula from http://www.mvps.org/directx/articles/catmull/
            // Internally using doubles not to lose precission
            double amountSquared = amount * amount;
            double amountCubed = amountSquared * amount;
            return (float)(0.5 * (2.0 * value2 +
                (value3 - value1) * amount +
                (2.0 * value1 - 5.0 * value2 + 4.0 * value3 - value4) * amountSquared +
                (3.0 * value2 - value1 - 3.0 * value3 + value4) * amountCubed));
        }

        /// <summary>
        /// Calculates the absolute value of the difference of two values.
        /// </summary>
        /// <param name="value1">Source value.</param>
        /// <param name="value2">Source value.</param>
        /// <returns>Distance between the two values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance(float value1, float value2) => MathF.Abs(value1 - value2);

        /// <summary>
        /// Performs a Hermite spline interpolation.
        /// </summary>
        /// <param name="value1">Source position.</param>
        /// <param name="tangent1">Source tangent.</param>
        /// <param name="value2">Source position.</param>
        /// <param name="tangent2">Source tangent.</param>
        /// <param name="amount">Weighting factor.</param>
        /// <returns>The result of the Hermite spline interpolation.</returns>
        public static float Hermite(float value1, float tangent1, float value2, float tangent2, float amount)
        {
            // All transformed to double not to lose precission
            // Otherwise, for high numbers of param:amount the result is NaN instead of Infinity
            double v1 = value1, v2 = value2, t1 = tangent1, t2 = tangent2, s = amount, result;
            double sCubed = s * s * s;
            double sSquared = s * s;

            if (amount == 0f)
                result = value1;
            else if (amount == 1f)
                result = value2;
            else
                result = (2 * v1 - 2 * v2 + t2 + t1) * sCubed +
                    (3 * v2 - 3 * v1 - 2 * t1 - t2) * sSquared +
                    t1 * s +
                    v1;
            return (float)result;
        }
        
	 
        /// <summary>
        /// Reduces a given angle to a value between π and -π.
        /// </summary>
        /// <param name="angle">The angle to reduce, in radians.</param>
        /// <returns>The new angle, in radians.</returns>
        public static float WrapAngle(float angle)
        {
            if ((angle > -Pi) && (angle <= Pi))
                return angle;
            angle %= TwoPi;
            if (angle <= -Pi)
                return angle + TwoPi;
            if (angle > Pi)
                return angle - TwoPi;
            return angle;
        }

        #region CalculateCentroid

        public static Vector2 CalculateCentroid(Vector2[] vertices)
        {
            float xCount = 0, yCount = 0;

            foreach (Vector2 v in vertices)
            {
                xCount += v.X;
                yCount += v.Y;
            }

            return new Vector2(xCount / vertices.Length, yCount / vertices.Length);
        }

        public static Vector3 CalculateCentroid(Vector3[] vertices)
        {
            float xCount = 0, yCount = 0, zCount = 0;

            foreach (Vector3 v in vertices)
            {
                xCount += v.X;
                yCount += v.Y;
                zCount += v.Z;
            }

            return new Vector3(xCount / vertices.Length, yCount / vertices.Length, zCount / vertices.Length);
        }

        #endregion

        #region Magnitude

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Magnitude(Vector2 v)
        {
            return MathF.Sqrt(
                (v.X * v.X) +
                (v.Y * v.Y));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Magnitude(Vector3 v)
        {
            return MathF.Sqrt(
                (v.X * v.X) +
                (v.Y * v.Y) +
                (v.Z * v.Z));
        }

        #endregion

        /// <summary>
        /// Calculates the 2d coordinates where a line ends at an  angle to the origin. 0 degreees is to the right
        /// </summary>
        public static Vector2 DegreesToXY(float degrees, float radius, Vector2 origin)
        {
            Vector2 xy = new Vector2();
            var radians = degrees * MathF.PI / 180.0f;

            xy.X = MathF.Cos(radians) * radius + origin.X;
            xy.Y = MathF.Sin(-radians) * radius + origin.Y;

            return xy;
        }

        /// <summary>
        /// Calculates the angle between xy and origin (0 is to the right)
        /// </summary>
        public static float XYToDegrees(Vector2 xy, Vector2 origin)
        {
            float deltaX = origin.X - xy.X;
            float deltaY = origin.Y - xy.Y;

            float radAngle = MathF.Atan2(deltaY, deltaX);
            float degreeAngle = radAngle * 180.0f / MathF.PI;

            return 180.0f - degreeAngle;
        }

        public static float GetAngleOfLineBetweenTwoPoints(Vector2 p1, Vector2 p2)
        {
            float xDiff = p2.X - p1.X;
            float yDiff = p2.Y - p1.Y;
            return MathF.Atan2(yDiff, xDiff);
        }

        /// <summary>
        /// Calculates the position of vertices for creating regular polygons with an arbitrary number of faces
        /// </summary>
        /// <param name="sides">number of sides the polygon should have</param>
        /// <param name="radius">the radius of the polygon. This is the distance from the centre to the vertices</param>
        /// <param name="startingAngle">The angle from vertical that the first vertex is calculated</param>
        /// <param name="center">The point that will for the centroid of the polygon.</param>
        /// <returns>a list of points that make the polygon</returns>
        /// <remarks>The list of vertices returned from this method are not useful for drawing as a vertex buffer. To draw these vertices to scrren, they must first be sent to the <see cref="triangulate"/> method</remarks>
        public static List<Vector2> CalculateVertices(int sides, int radius, int startingAngle, Vector2 center)
        {
            if (sides < 3)
                throw new ArgumentException("Polygon must have 3 sides or more.");

            List<Vector2> points = new List<Vector2>();
            float step = 360.0f / sides;

            float angle = startingAngle; //starting angle
            for (double i = startingAngle; i < startingAngle + 360.0; i += step) //go in a full circle
            {
                points.Add(DegreesToXY(angle, radius, center));
                angle += step;
            }

            return points;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int NextPowerOfTwo(int value) => (int)MathF.Pow(2, MathF.Floor(MathF.Log(value, 2)) + 1);

        public static Vector2 Rotate(Vector2 point, float radians, Vector2 pivot)
        {
            float cosRadians = MathF.Cos(radians);
            float sinRadians = MathF.Sin(radians);

            Vector2 translatedPoint = new Vector2();
            translatedPoint.X = point.X - pivot.X;
            translatedPoint.Y = point.Y - pivot.Y;

            Vector2 rotatedPoint = new Vector2();
            rotatedPoint.X = translatedPoint.X * cosRadians - translatedPoint.Y * sinRadians + pivot.X;
            rotatedPoint.Y = translatedPoint.X * sinRadians + translatedPoint.Y * cosRadians + pivot.Y;

            return rotatedPoint;
        }

        public static bool IsInPolygon(Vector2[] poly, Vector2 p)
        {
            Vector2 p1, p2;

            bool inside = false;

            if (poly.Length < 3)
                return inside;

            var oldPoint = new Vector2(poly[poly.Length - 1].X, poly[poly.Length - 1].Y);

            for (int i = 0; i < poly.Length; i++)
            {
                var newPoint = new Vector2(poly[i].X, poly[i].Y);

                if (newPoint.X > oldPoint.X)
                {
                    p1 = oldPoint;
                    p2 = newPoint;
                }
                else
                {
                    p1 = newPoint;
                    p2 = oldPoint;
                }

                if ((newPoint.X < p.X) == (p.X <= oldPoint.X)
                    && (p.Y - (long)p1.Y) * (p2.X - p1.X)
                    < (p2.Y - (long)p1.Y) * (p.X - p1.X))
                {
                    inside = !inside;
                }

                oldPoint = newPoint;
            }

            return inside;
        }

        public static Vector3 AngleTo(Vector3 from, Vector3 location)
        {
            Vector3 angle = new Vector3();
            Vector3 v3 = Vector3.Normalize(location - from);

            angle.X = MathF.Asin(v3.Y);
            angle.Y = MathF.Atan2(-v3.X, -v3.Z);

            return angle;
        }

        /// <summary>
        /// returns false is any component (x, y, z) is either NaN of +/- Infinity
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static bool IsValid(Vector3 v)
        {
            if (float.IsNaN(v.X) || float.IsInfinity(v.X))
                return false;

            if (float.IsNaN(v.Y) || float.IsInfinity(v.Y))
                return false;

            if (float.IsNaN(v.Z) || float.IsInfinity(v.Z))
                return false;

            return true;
        }

        /// <summary>
        /// Converts a Quaternion to is Euler angles. X: Yaw, Y: Pitch, Z: Roll
        /// </summary>
        /// <param name="rot">The Quaternion to convert to angles</param>
        /// <param name="inDegrees">if "True" the result of the conversion is returned in Degrees. Otherwise it is returned in radians</param>
        /// <returns></returns>
        public static Vector3 QuaternionToEuler(Quaternion rot, bool inDegrees)
        {
            float q0 = rot.W;
            float q1 = rot.Y;
            float q2 = rot.X;
            float q3 = rot.Z;

            Vector3 radAngles = new Vector3();
            radAngles.X = MathF.Atan2(2 * (q0 * q1 + q2 * q3), 1 - 2 * (MathF.Pow(q1, 2) + MathF.Pow(q2, 2)));
            radAngles.Y = MathF.Asin(2 * (q0 * q2 - q3 * q1));
            radAngles.Z = MathF.Atan2(2 * (q0 * q3 + q1 * q2), 1 - 2 * (MathF.Pow(q2, 2) + MathF.Pow(q3, 2)));

            if (!inDegrees)
                return radAngles;

            Vector3 angles = new Vector3();
            angles.X = radAngles.X * ToDegCoefficient;
            angles.Y = radAngles.Y * ToDegCoefficient;
            angles.Z = radAngles.Z * ToDegCoefficient;

            return angles;
        }
    }
}

