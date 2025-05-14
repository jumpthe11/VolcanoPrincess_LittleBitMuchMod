using MelonLoader;
using System.Diagnostics;
using UnityEngine;

public class MyMod : MelonMod
{
    public override void OnApplicationStart()
    {
        Debug.Log("My mod has loaded!");
    }
}
