using BepInEx;
using HarmonyLib;
using ExitGames.Client.Photon;
using Photon.Voice.Unity;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[BepInPlugin("com.gil.photonvoicefix", "Photon Voice Fix", "1.0.0")]
public class VoiceFixPlugin : BaseUnityPlugin
{
    void Awake()
    {
        try
        {
            var harmony = new Harmony("com.gil.photonvoicefix");
            var sendOp = AccessTools.Method(typeof(PhotonPeer), "SendOperation", 
                new Type[] { typeof(byte), typeof(ParameterDictionary), typeof(SendOptions) });
            var prefix = AccessTools.Method(typeof(PhotonFixPatch), "DeepScrubPrefix");
            harmony.Patch(sendOp, new HarmonyMethod(prefix));

            // lmao
            var enqueue = AccessTools.Method(typeof(PhotonPeer), "EnqueueOperation");
            var prefixEnqueue = AccessTools.Method(typeof(PhotonFixPatch), "UniversalPrefix");
            harmony.Patch(enqueue, new HarmonyMethod(prefixEnqueue));

            Logger.LogInfo("VoiceFix: Enumerator Scrub v1.1.9 Active.");
        }
        catch (Exception e)
        {
            Logger.LogError("VoiceFix: Patch Failed: " + e.Message);
        }
    }

    void Update()
    {
        if (!PhotonNetwork.InRoom) return;
        if (PhotonNetwork.CloudRegion == null || PhotonNetwork.CloudRegion.ToLower().Contains("custom"))
        {
            PhotonNetwork.NetworkingClient.LoadBalancingPeer.SerializationProtocolType = (SerializationProtocol)0;
        }
    }
}

public static class PhotonFixPatch
{
    public static void DeepScrubPrefix(byte operationCode, ParameterDictionary operationParameters, ref SendOptions sendOptions)
    {
        // FORCE RELIABILITY
        sendOptions.DeliveryMode = DeliveryMode.Reliable;
        sendOptions.Encrypt = false;

        if (operationParameters == null) return;

        var enumerator = operationParameters.GetEnumerator();
        while (enumerator.MoveNext())
        {
            var current = enumerator.Current;
            object scrubbed = ScrubObject(current.Value);
            if (scrubbed != current.Value)
            {
                operationParameters[current.Key] = scrubbed;
            }
        }
    }

    private static object ScrubObject(object obj)
    {
        if (obj == null) return null;

        ByteArraySlice slice = obj as ByteArraySlice;
        if (slice != null)
        {
            byte[] clean = new byte[slice.Count];
            Buffer.BlockCopy(slice.Buffer, slice.Offset, clean, 0, slice.Count);
            return clean;
        }

        object[] array = obj as object[];
        if (array != null)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = ScrubObject(array[i]);
            }
            return array;
        }

        return obj;
    }

    // fallback
    public static void UniversalPrefix(ref SendOptions sendOptions)
    {
        sendOptions.Encrypt = false;
        sendOptions.DeliveryMode = DeliveryMode.Reliable;
    }

}
