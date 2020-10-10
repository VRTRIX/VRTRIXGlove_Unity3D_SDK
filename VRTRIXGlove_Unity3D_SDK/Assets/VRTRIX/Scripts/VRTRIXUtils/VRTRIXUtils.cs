//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Utilities for working with SteamVR
//
//=============================================================================

using UnityEngine;
using System.Collections;
using Valve.VRAPI;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VRTRIX
{
    public static class VRTRIXUtils
    {
        public class Event
        {
            public delegate void Handler(params object[] args);

            public static void Listen(string message, Handler action)
            {
                var actions = listeners[message] as Handler;
                if (actions != null)
                {
                    listeners[message] = actions + action;
                }
                else
                {
                    listeners[message] = action;
                }
            }

            public static void Remove(string message, Handler action)
            {
                var actions = listeners[message] as Handler;
                if (actions != null)
                {
                    listeners[message] = actions - action;
                }
            }

            public static void Send(string message, params object[] args)
            {
                var actions = listeners[message] as Handler;
                if (actions != null)
                {
                    actions(args);
                }
            }

            private static Hashtable listeners = new Hashtable();
        }

        public const float FeetToMeters = 0.3048f;
        public const float FeetToCentimeters = 30.48f;
        public const float InchesToMeters = 0.0254f;
        public const float InchesToCentimeters = 2.54f;
        public const float MetersToFeet = 3.28084f;
        public const float MetersToInches = 39.3701f;
        public const float CentimetersToFeet = 0.0328084f;
        public const float CentimetersToInches = 0.393701f;
        public const float KilometersToMiles = 0.621371f;
        public const float MilesToKilometers = 1.60934f;

        //-------------------------------------------------
        // Remap num from range 1 to range 2
        //-------------------------------------------------
        public static float RemapNumber(float num, float low1, float high1, float low2, float high2)
        {
            return low2 + (num - low1) * (high2 - low2) / (high1 - low1);
        }


        //-------------------------------------------------
        public static float RemapNumberClamped(float num, float low1, float high1, float low2, float high2)
        {
            return Mathf.Clamp(RemapNumber(num, low1, high1, low2, high2), Mathf.Min(low2, high2), Mathf.Max(low2, high2));
        }


        //-------------------------------------------------
        public static float Approach(float target, float value, float speed)
        {
            float delta = target - value;

            if (delta > speed)
                value += speed;
            else if (delta < -speed)
                value -= speed;
            else
                value = target;

            return value;
        }


        //-------------------------------------------------
        public static Vector3 BezierInterpolate3(Vector3 p0, Vector3 c0, Vector3 p1, float t)
        {
            Vector3 p0c0 = Vector3.Lerp(p0, c0, t);
            Vector3 c0p1 = Vector3.Lerp(c0, p1, t);

            return Vector3.Lerp(p0c0, c0p1, t);
        }


        //-------------------------------------------------
        public static Vector3 BezierInterpolate4(Vector3 p0, Vector3 c0, Vector3 c1, Vector3 p1, float t)
        {
            Vector3 p0c0 = Vector3.Lerp(p0, c0, t);
            Vector3 c0c1 = Vector3.Lerp(c0, c1, t);
            Vector3 c1p1 = Vector3.Lerp(c1, p1, t);

            Vector3 x = Vector3.Lerp(p0c0, c0c1, t);
            Vector3 y = Vector3.Lerp(c0c1, c1p1, t);

            //Debug.DrawRay(p0, Vector3.forward);
            //Debug.DrawRay(c0, Vector3.forward);
            //Debug.DrawRay(c1, Vector3.forward);
            //Debug.DrawRay(p1, Vector3.forward);

            //Gizmos.DrawSphere(p0c0, 0.5F);
            //Gizmos.DrawSphere(c0c1, 0.5F);
            //Gizmos.DrawSphere(c1p1, 0.5F);
            //Gizmos.DrawSphere(x, 0.5F);
            //Gizmos.DrawSphere(y, 0.5F);

            return Vector3.Lerp(x, y, t);
        }


        //-------------------------------------------------
        public static Vector3 Vector3FromString(string szString)
        {
            string[] szParseString = szString.Substring(1, szString.Length - 1).Split(',');

            float x = float.Parse(szParseString[0]);
            float y = float.Parse(szParseString[1]);
            float z = float.Parse(szParseString[2]);

            Vector3 vReturn = new Vector3(x, y, z);

            return vReturn;
        }


        //-------------------------------------------------
        public static Vector2 Vector2FromString(string szString)
        {
            string[] szParseString = szString.Substring(1, szString.Length - 1).Split(',');

            float x = float.Parse(szParseString[0]);
            float y = float.Parse(szParseString[1]);

            Vector3 vReturn = new Vector2(x, y);

            return vReturn;
        }


        //-------------------------------------------------
        public static float Normalize(float value, float min, float max)
        {
            float normalizedValue = (value - min) / (max - min);

            return normalizedValue;
        }


        //-------------------------------------------------
        public static Vector3 Vector2AsVector3(Vector2 v)
        {
            return new Vector3(v.x, 0.0f, v.y);
        }


        //-------------------------------------------------
        public static Vector2 Vector3AsVector2(Vector3 v)
        {
            return new Vector2(v.x, v.z);
        }


        //-------------------------------------------------
        public static float AngleOf(Vector2 v)
        {
            float fDist = v.magnitude;

            if (v.y >= 0.0f)
            {
                return Mathf.Acos(v.x / fDist);
            }
            else
            {
                return Mathf.Acos(-v.x / fDist) + Mathf.PI;
            }
        }


        //-------------------------------------------------
        public static float YawOf(Vector3 v)
        {
            float fDist = v.magnitude;

            if (v.z >= 0.0f)
            {
                return Mathf.Acos(v.x / fDist);
            }
            else
            {
                return Mathf.Acos(-v.x / fDist) + Mathf.PI;
            }
        }


        //-------------------------------------------------
        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
        }


        //-------------------------------------------------
        public static void Shuffle<T>(T[] array)
        {
            for (int i = array.Length - 1; i > 0; i--)
            {
                int r = UnityEngine.Random.Range(0, i);
                Swap(ref array[i], ref array[r]);
            }
        }


        //-------------------------------------------------
        public static void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int r = UnityEngine.Random.Range(0, i);
                T temp = list[i];
                list[i] = list[r];
                list[r] = temp;
            }
        }


        //-------------------------------------------------
        public static int RandomWithLookback(int min, int max, List<int> history, int historyCount)
        {
            int index = UnityEngine.Random.Range(min, max - history.Count);

            for (int i = 0; i < history.Count; i++)
            {
                if (index >= history[i])
                {
                    index++;
                }
            }

            history.Add(index);

            if (history.Count > historyCount)
            {
                history.RemoveRange(0, history.Count - historyCount);
            }

            return index;
        }


        //-------------------------------------------------
        public static Transform FindChild(Transform parent, string name)
        {
            if (parent.name == name)
                return parent;

            foreach (Transform child in parent)
            {
                var found = FindChild(child, name);
                if (found != null)
                    return found;
            }

            return null;
        }


        //-------------------------------------------------
        public static bool IsNullOrEmpty<T>(T[] array)
        {
            if (array == null)
                return true;

            if (array.Length == 0)
                return true;

            return false;
        }


        //-------------------------------------------------
        public static bool IsValidIndex<T>(T[] array, int i)
        {
            if (array == null)
                return false;

            return (i >= 0) && (i < array.Length);
        }


        //-------------------------------------------------
        public static bool IsValidIndex<T>(List<T> list, int i)
        {
            if (list == null || list.Count == 0)
                return false;

            return (i >= 0) && (i < list.Count);
        }


        //-------------------------------------------------
        public static int FindOrAdd<T>(List<T> list, T item)
        {
            int index = list.IndexOf(item);

            if (index == -1)
            {
                list.Add(item);
                index = list.Count - 1;
            }

            return index;
        }


        //-------------------------------------------------
        public static List<T> FindAndRemove<T>(List<T> list, System.Predicate<T> match)
        {
            List<T> retVal = list.FindAll(match);
            list.RemoveAll(match);
            return retVal;
        }


        //-------------------------------------------------
        public static T FindOrAddComponent<T>(GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            if (component)
                return component;

            return gameObject.AddComponent<T>();
        }


        //-------------------------------------------------
        public static void FastRemove<T>(List<T> list, int index)
        {
            list[index] = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);
        }


        //-------------------------------------------------
        public static void ReplaceGameObject<T, U>(T replace, U replaceWith)
            where T : MonoBehaviour
            where U : MonoBehaviour
        {
            replace.gameObject.SetActive(false);
            replaceWith.gameObject.SetActive(true);
        }


        //-------------------------------------------------
        public static void SwitchLayerRecursively(Transform transform, int fromLayer, int toLayer)
        {
            if (transform.gameObject.layer == fromLayer)
                transform.gameObject.layer = toLayer;

            int childCount = transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                SwitchLayerRecursively(transform.GetChild(i), fromLayer, toLayer);
            }
        }


        //-------------------------------------------------
        public static void DrawCross(Vector3 origin, Color crossColor, float size)
        {
            Vector3 line1Start = origin + (Vector3.right * size);
            Vector3 line1End = origin - (Vector3.right * size);

            Debug.DrawLine(line1Start, line1End, crossColor);

            Vector3 line2Start = origin + (Vector3.up * size);
            Vector3 line2End = origin - (Vector3.up * size);

            Debug.DrawLine(line2Start, line2End, crossColor);

            Vector3 line3Start = origin + (Vector3.forward * size);
            Vector3 line3End = origin - (Vector3.forward * size);

            Debug.DrawLine(line3Start, line3End, crossColor);
        }


        //-------------------------------------------------
        public static void ResetTransform(Transform t, bool resetScale = true)
        {
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            if (resetScale)
            {
                t.localScale = new Vector3(1f, 1f, 1f);
            }
        }


        //-------------------------------------------------
        public static Vector3 ClosestPointOnLine(Vector3 vA, Vector3 vB, Vector3 vPoint)
        {
            var vVector1 = vPoint - vA;
            var vVector2 = (vB - vA).normalized;

            var d = Vector3.Distance(vA, vB);
            var t = Vector3.Dot(vVector2, vVector1);

            if (t <= 0)
                return vA;

            if (t >= d)
                return vB;

            var vVector3 = vVector2 * t;

            var vClosestPoint = vA + vVector3;

            return vClosestPoint;
        }


        //-------------------------------------------------
        public static void AfterTimer(GameObject go, float _time, System.Action callback, bool trigger_if_destroyed_early = false)
        {
            AfterTimer_Component afterTimer_component = go.AddComponent<AfterTimer_Component>();
            afterTimer_component.Init(_time, callback, trigger_if_destroyed_early);
        }


        //-------------------------------------------------
        public static void SendPhysicsMessage(Collider collider, string message, SendMessageOptions sendMessageOptions)
        {
            Rigidbody rb = collider.attachedRigidbody;
            if (rb && rb.gameObject != collider.gameObject)
            {
                rb.SendMessage(message, sendMessageOptions);
            }

            collider.SendMessage(message, sendMessageOptions);
        }


        //-------------------------------------------------
        public static void SendPhysicsMessage(Collider collider, string message, object arg, SendMessageOptions sendMessageOptions)
        {
            Rigidbody rb = collider.attachedRigidbody;
            if (rb && rb.gameObject != collider.gameObject)
            {
                rb.SendMessage(message, arg, sendMessageOptions);
            }

            collider.SendMessage(message, arg, sendMessageOptions);
        }


        //-------------------------------------------------
        public static void IgnoreCollisions(GameObject goA, GameObject goB)
        {
            Collider[] goA_colliders = goA.GetComponentsInChildren<Collider>();
            Collider[] goB_colliders = goB.GetComponentsInChildren<Collider>();

            if (goA_colliders.Length == 0 || goB_colliders.Length == 0)
            {
                return;
            }

            foreach (Collider cA in goA_colliders)
            {
                foreach (Collider cB in goB_colliders)
                {
                    if (cA.enabled && cB.enabled)
                    {
                        Physics.IgnoreCollision(cA, cB, true);
                    }
                }
            }
        }


        //-------------------------------------------------
        public static IEnumerator WrapCoroutine(IEnumerator coroutine, System.Action onCoroutineFinished)
        {
            while (coroutine.MoveNext())
            {
                yield return coroutine.Current;
            }

            onCoroutineFinished();
        }


        //-------------------------------------------------
        public static Color ColorWithAlpha(this Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        //-------------------------------------------------
        // Exits the application if running standalone, or stops playback if running in the editor.
        //-------------------------------------------------
        public static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        // NOTE: The recommended call for exiting a Unity app is UnityEngine.Application.Quit(), but as
        // of 5.1.0f3 this was causing the application to crash. The following works without crashing:
        System.Diagnostics.Process.GetCurrentProcess().Kill();
#endif
        }

        //-------------------------------------------------
        // Truncate floats to the specified # of decimal places when you want easier-to-read numbers without clamping to an int
        //-------------------------------------------------
        public static decimal FloatToDecimal(float value, int decimalPlaces = 2)
        {
            return Math.Round((decimal)value, decimalPlaces);
        }


        //-------------------------------------------------
        public static T Median<T>(this IEnumerable<T> source)
        {
            if (source == null)
            {
                throw new ArgumentException("Argument cannot be null.", "source");
            }

            int count = source.Count();
            if (count == 0)
            {
                throw new InvalidOperationException("Enumerable must contain at least one element.");
            }

            return source.OrderBy(x => x).ElementAt(count / 2);
        }


        //-------------------------------------------------
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null)
            {
                throw new ArgumentException("Argument cannot be null.", "source");
            }

            foreach (T value in source)
            {
                action(value);
            }
        }


        //-------------------------------------------------
        // In some cases Unity/C# don't correctly interpret the newline control character (\n).
        // This function replaces every instance of "\\n" with the actual newline control character.
        //-------------------------------------------------
        public static string FixupNewlines(string text)
        {
            bool newLinesRemaining = true;

            while (newLinesRemaining)
            {
                int CIndex = text.IndexOf("\\n");

                if (CIndex == -1)
                {
                    newLinesRemaining = false;
                }
                else
                {
                    text = text.Remove(CIndex - 1, 3);
                    text = text.Insert(CIndex - 1, "\n");
                }
            }

            return text;
        }


        //-------------------------------------------------
#if (UNITY_5_4)
		public static float PathLength( NavMeshPath path )
#else
        public static float PathLength(UnityEngine.AI.NavMeshPath path)
#endif
        {
            if (path.corners.Length < 2)
                return 0;

            Vector3 previousCorner = path.corners[0];
            float lengthSoFar = 0.0f;
            int i = 1;
            while (i < path.corners.Length)
            {
                Vector3 currentCorner = path.corners[i];
                lengthSoFar += Vector3.Distance(previousCorner, currentCorner);
                previousCorner = currentCorner;
                i++;
            }
            return lengthSoFar;
        }


        //-------------------------------------------------
        public static bool HasCommandLineArgument(string argumentName)
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals(argumentName))
                {
                    return true;
                }
            }

            return false;
        }


        //-------------------------------------------------
        public static int GetCommandLineArgValue(string argumentName, int nDefaultValue)
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals(argumentName))
                {
                    if (i == (args.Length - 1)) // Last arg, return default
                    {
                        return nDefaultValue;
                    }

                    return System.Int32.Parse(args[i + 1]);
                }
            }

            return nDefaultValue;
        }


        //-------------------------------------------------
        public static float GetCommandLineArgValue(string argumentName, float flDefaultValue)
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals(argumentName))
                {
                    if (i == (args.Length - 1)) // Last arg, return default
                    {
                        return flDefaultValue;
                    }

                    return (float)Double.Parse(args[i + 1]);
                }
            }

            return flDefaultValue;
        }


        //-------------------------------------------------
        public static void SetActive(GameObject gameObject, bool active)
        {
            if (gameObject != null)
            {
                gameObject.SetActive(active);
            }
        }


        //-------------------------------------------------
        // The version of Path.Combine() included with Unity can only combine two paths.
        // This version mimics the modern .NET version, which allows for any number of
        // paths to be combined.
        //-------------------------------------------------
        public static string CombinePaths(params string[] paths)
        {
            if (paths.Length == 0)
            {
                return "";
            }
            else
            {
                string combinedPath = paths[0];
                for (int i = 1; i < paths.Length; i++)
                {
                    combinedPath = Path.Combine(combinedPath, paths[i]);
                }

                return combinedPath;
            }
        }

        public static bool IsValid(Vector3 vector)
        {
            return (float.IsNaN(vector.x) == false && float.IsNaN(vector.y) == false && float.IsNaN(vector.z) == false);
        }
        public static bool IsValid(Quaternion rotation)
        {
            return (float.IsNaN(rotation.x) == false && float.IsNaN(rotation.y) == false && float.IsNaN(rotation.z) == false && float.IsNaN(rotation.w) == false) &&
                (rotation.x != 0 || rotation.y != 0 || rotation.z != 0 || rotation.w != 0);
        }

        // this version does not clamp [0..1]
        public static Quaternion Slerp(Quaternion A, Quaternion B, float t)
        {
            var cosom = Mathf.Clamp(A.x * B.x + A.y * B.y + A.z * B.z + A.w * B.w, -1.0f, 1.0f);
            if (cosom < 0.0f)
            {
                B = new Quaternion(-B.x, -B.y, -B.z, -B.w);
                cosom = -cosom;
            }

            float sclp, sclq;
            if ((1.0f - cosom) > 0.0001f)
            {
                var omega = Mathf.Acos(cosom);
                var sinom = Mathf.Sin(omega);
                sclp = Mathf.Sin((1.0f - t) * omega) / sinom;
                sclq = Mathf.Sin(t * omega) / sinom;
            }
            else
            {
                // "from" and "to" very close, so do linear interp
                sclp = 1.0f - t;
                sclq = t;
            }

            return new Quaternion(
                sclp * A.x + sclq * B.x,
                sclp * A.y + sclq * B.y,
                sclp * A.z + sclq * B.z,
                sclp * A.w + sclq * B.w);
        }

        public static Vector3 Lerp(Vector3 A, Vector3 B, float t)
        {
            return new Vector3(
                Lerp(A.x, B.x, t),
                Lerp(A.y, B.y, t),
                Lerp(A.z, B.z, t));
        }

        public static float Lerp(float A, float B, float t)
        {
            return A + (B - A) * t;
        }

        public static double Lerp(double A, double B, double t)
        {
            return A + (B - A) * t;
        }

        public static float InverseLerp(Vector3 A, Vector3 B, Vector3 result)
        {
            return Vector3.Dot(result - A, B - A);
        }

        public static float InverseLerp(float A, float B, float result)
        {
            return (result - A) / (B - A);
        }

        public static double InverseLerp(double A, double B, double result)
        {
            return (result - A) / (B - A);
        }

        public static float Saturate(float A)
        {
            return (A < 0) ? 0 : (A > 1) ? 1 : A;
        }

        public static Vector2 Saturate(Vector2 A)
        {
            return new Vector2(Saturate(A.x), Saturate(A.y));
        }

        public static float Abs(float A)
        {
            return (A < 0) ? -A : A;
        }

        public static Vector2 Abs(Vector2 A)
        {
            return new Vector2(Abs(A.x), Abs(A.y));
        }

        private static float _copysign(float sizeval, float signval)
        {
            return Mathf.Sign(signval) == 1 ? Mathf.Abs(sizeval) : -Mathf.Abs(sizeval);
        }

        public static Quaternion GetRotation(this Matrix4x4 matrix)
        {
            Quaternion q = new Quaternion();
            q.w = Mathf.Sqrt(Mathf.Max(0, 1 + matrix.m00 + matrix.m11 + matrix.m22)) / 2;
            q.x = Mathf.Sqrt(Mathf.Max(0, 1 + matrix.m00 - matrix.m11 - matrix.m22)) / 2;
            q.y = Mathf.Sqrt(Mathf.Max(0, 1 - matrix.m00 + matrix.m11 - matrix.m22)) / 2;
            q.z = Mathf.Sqrt(Mathf.Max(0, 1 - matrix.m00 - matrix.m11 + matrix.m22)) / 2;
            q.x = _copysign(q.x, matrix.m21 - matrix.m12);
            q.y = _copysign(q.y, matrix.m02 - matrix.m20);
            q.z = _copysign(q.z, matrix.m10 - matrix.m01);
            return q;
        }

        public static Vector3 GetPosition(this Matrix4x4 matrix)
        {
            var x = matrix.m03;
            var y = matrix.m13;
            var z = matrix.m23;

            return new Vector3(x, y, z);
        }

        public static Vector3 GetScale(this Matrix4x4 m)
        {
            var x = Mathf.Sqrt(m.m00 * m.m00 + m.m01 * m.m01 + m.m02 * m.m02);
            var y = Mathf.Sqrt(m.m10 * m.m10 + m.m11 * m.m11 + m.m12 * m.m12);
            var z = Mathf.Sqrt(m.m20 * m.m20 + m.m21 * m.m21 + m.m22 * m.m22);

            return new Vector3(x, y, z);
        }

        public static float GetLossyScale(Transform t)
        {
            return t.lossyScale.x;
        }

        private const string secretKey = "foobar";

        public static string GetBadMD5Hash(string usedString)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(usedString + secretKey);

            return GetBadMD5Hash(bytes);
        }
        public static string GetBadMD5Hash(byte[] bytes)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] hash = md5.ComputeHash(bytes);

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }

            return sb.ToString();
        }
        public static string GetBadMD5HashFromFile(string filePath)
        {
            if (File.Exists(filePath) == false)
                return null;

            string data = File.ReadAllText(filePath);
            return GetBadMD5Hash(data + secretKey);
        }

        public static string SanitizePath(string path, bool allowLeadingSlash = true)
        {
            if (path.Contains("\\\\"))
                path = path.Replace("\\\\", "\\");
            if (path.Contains("//"))
                path = path.Replace("//", "/");

            if (allowLeadingSlash == false)
            {
                if (path[0] == '/' || path[0] == '\\')
                    path = path.Substring(1);
            }

            return path;
        }

        [System.Serializable]
        public struct RigidTransform
        {
            public Vector3 pos;
            public Quaternion rot;

            public static RigidTransform identity
            {
                get { return new RigidTransform(Vector3.zero, Quaternion.identity); }
            }

            public static RigidTransform FromLocal(Transform t)
            {
                return new RigidTransform(t.localPosition, t.localRotation);
            }

            public RigidTransform(Vector3 pos, Quaternion rot)
            {
                this.pos = pos;
                this.rot = rot;
            }

            public RigidTransform(Transform t)
            {
                this.pos = t.position;
                this.rot = t.rotation;
            }

            public RigidTransform(Transform from, Transform to)
            {
                var inv = Quaternion.Inverse(from.rotation);
                rot = inv * to.rotation;
                pos = inv * (to.position - from.position);
            }

            public RigidTransform(HmdMatrix34_t pose)
            {
                var m = Matrix4x4.identity;

                m[0, 0] = pose.m0;
                m[0, 1] = pose.m1;
                m[0, 2] = -pose.m2;
                m[0, 3] = pose.m3;

                m[1, 0] = pose.m4;
                m[1, 1] = pose.m5;
                m[1, 2] = -pose.m6;
                m[1, 3] = pose.m7;

                m[2, 0] = -pose.m8;
                m[2, 1] = -pose.m9;
                m[2, 2] = pose.m10;
                m[2, 3] = -pose.m11;

                this.pos = m.GetPosition();
                this.rot = m.GetRotation();
            }

            public RigidTransform(HmdMatrix44_t pose)
            {
                var m = Matrix4x4.identity;

                m[0, 0] = pose.m0;
                m[0, 1] = pose.m1;
                m[0, 2] = -pose.m2;
                m[0, 3] = pose.m3;

                m[1, 0] = pose.m4;
                m[1, 1] = pose.m5;
                m[1, 2] = -pose.m6;
                m[1, 3] = pose.m7;

                m[2, 0] = -pose.m8;
                m[2, 1] = -pose.m9;
                m[2, 2] = pose.m10;
                m[2, 3] = -pose.m11;

                m[3, 0] = pose.m12;
                m[3, 1] = pose.m13;
                m[3, 2] = -pose.m14;
                m[3, 3] = pose.m15;

                this.pos = m.GetPosition();
                this.rot = m.GetRotation();
            }

            public HmdMatrix44_t ToHmdMatrix44()
            {
                var m = Matrix4x4.TRS(pos, rot, Vector3.one);
                var pose = new HmdMatrix44_t();

                pose.m0 = m[0, 0];
                pose.m1 = m[0, 1];
                pose.m2 = -m[0, 2];
                pose.m3 = m[0, 3];

                pose.m4 = m[1, 0];
                pose.m5 = m[1, 1];
                pose.m6 = -m[1, 2];
                pose.m7 = m[1, 3];

                pose.m8 = -m[2, 0];
                pose.m9 = -m[2, 1];
                pose.m10 = m[2, 2];
                pose.m11 = -m[2, 3];

                pose.m12 = m[3, 0];
                pose.m13 = m[3, 1];
                pose.m14 = -m[3, 2];
                pose.m15 = m[3, 3];

                return pose;
            }

            public HmdMatrix34_t ToHmdMatrix34()
            {
                var m = Matrix4x4.TRS(pos, rot, Vector3.one);
                var pose = new HmdMatrix34_t();

                pose.m0 = m[0, 0];
                pose.m1 = m[0, 1];
                pose.m2 = -m[0, 2];
                pose.m3 = m[0, 3];

                pose.m4 = m[1, 0];
                pose.m5 = m[1, 1];
                pose.m6 = -m[1, 2];
                pose.m7 = m[1, 3];

                pose.m8 = -m[2, 0];
                pose.m9 = -m[2, 1];
                pose.m10 = m[2, 2];
                pose.m11 = -m[2, 3];

                return pose;
            }

            public override bool Equals(object o)
            {
                if (o is RigidTransform)
                {
                    RigidTransform t = (RigidTransform)o;
                    return pos == t.pos && rot == t.rot;
                }
                return false;
            }



            public override int GetHashCode()
            {
                return pos.GetHashCode() ^ rot.GetHashCode();
            }

            public static bool operator ==(RigidTransform a, RigidTransform b)
            {
                return a.pos == b.pos && a.rot == b.rot;
            }

            public static bool operator !=(RigidTransform a, RigidTransform b)
            {
                return a.pos != b.pos || a.rot != b.rot;
            }

            public static RigidTransform operator *(RigidTransform a, RigidTransform b)
            {
                return new RigidTransform
                {
                    rot = a.rot * b.rot,
                    pos = a.pos + a.rot * b.pos
                };
            }

            public void Inverse()
            {
                rot = Quaternion.Inverse(rot);
                pos = -(rot * pos);
            }

            public RigidTransform GetInverse()
            {
                var t = new RigidTransform(pos, rot);
                t.Inverse();
                return t;
            }

            public void Multiply(RigidTransform a, RigidTransform b)
            {
                rot = a.rot * b.rot;
                pos = a.pos + a.rot * b.pos;
            }

            public Vector3 InverseTransformPoint(Vector3 point)
            {
                return Quaternion.Inverse(rot) * (point - pos);
            }

            public Vector3 TransformPoint(Vector3 point)
            {
                return pos + (rot * point);
            }

            public static Vector3 operator *(RigidTransform t, Vector3 v)
            {
                return t.TransformPoint(v);
            }

            public static RigidTransform Interpolate(RigidTransform a, RigidTransform b, float t)
            {
                return new RigidTransform(Vector3.Lerp(a.pos, b.pos, t), Quaternion.Slerp(a.rot, b.rot, t));
            }

            public void Interpolate(RigidTransform to, float t)
            {
                pos = VRTRIXUtils.Lerp(pos, to.pos, t);
                rot = VRTRIXUtils.Slerp(rot, to.rot, t);
            }
        }
        public delegate object SystemFn(CVRSystem system, params object[] args);
    }

    //-------------------------------------------------------------------------
    //Component used by the static AfterTimer function
    //-------------------------------------------------------------------------
    [System.Serializable]
    public class AfterTimer_Component : MonoBehaviour
    {
        private System.Action callback;
        private float triggerTime;
        private bool timerActive = false;
        private bool triggerOnEarlyDestroy = false;

        //-------------------------------------------------
        public void Init(float _time, System.Action _callback, bool earlydestroy)
        {
            triggerTime = _time;
            callback = _callback;
            triggerOnEarlyDestroy = earlydestroy;
            timerActive = true;
            StartCoroutine(Wait());
        }


        //-------------------------------------------------
        private IEnumerator Wait()
        {
            yield return new WaitForSeconds(triggerTime);
            timerActive = false;
            callback.Invoke();
            Destroy(this);
        }


        //-------------------------------------------------
        void OnDestroy()
        {
            if (timerActive)
            {
                //If the component or its GameObject get destroyed before the timer is complete, clean up
                StopCoroutine(Wait());
                timerActive = false;

                if (triggerOnEarlyDestroy)
                {
                    callback.Invoke();
                }
            }
        }
    }
}
