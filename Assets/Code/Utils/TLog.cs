// Copyright Ioan-Bogdan Lazu. All Rights Reserved.

using UnityEngine;

public class Log : MonoBehaviour
{
#if DEBUG_MODE
    private static int DEBUG_INFO       = 1;
    private static int DEBUG_DEBUG      = 2;
    private static int DEBUG_WARNING    = 3;
    private static int DEBUG_ERROR      = 4;

    // Print all until error
    private static int DEBUG_LEVEL = DEBUG_ERROR;
#endif

    public static void i(string info)
    {
#if DEBUG_MODE
        if (DEBUG_LEVEL >= DEBUG_INFO)
            Debug.Log(info);
#endif
    }

    public static void d(string debug)
    {
#if DEBUG_MODE
        if(DEBUG_LEVEL >= DEBUG_DEBUG)
            Debug.Log(debug);
#endif
    }

    public static void w(string warning)
    {
#if DEBUG_MODE
        if (DEBUG_LEVEL >= DEBUG_WARNING)
            Debug.LogWarning(warning);
#endif
    }

    public static void e(string error)
    {
#if DEBUG_MODE
        if (DEBUG_LEVEL >= DEBUG_ERROR)
            Debug.LogError(error);
#endif
    }

    public static void SecureLog(string secure)
    {
#if DEBUG_MODE
        Debug.Log("[SECURE LOG]: " + secure);
#endif
    }
}
