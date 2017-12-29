using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Helper // contains functions that can't added in other script for some reason
{
    /// <summary>
    /// Determining equality by hashes of elements
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static bool ListEquals<T>(List<T> a, List<T> b)
    {
        if (a != null && b != null)
        {
            if (a.Count != b.Count)
                return false;
            else
                for (int i = 0; i < a.Count; i++)
                {
                    if (a[i].GetHashCode() != b[i].GetHashCode())
                        return false;
                }

            return true;
        }
        else if (a == null && b == null)
            return true;
        else
            return false;
    }
}