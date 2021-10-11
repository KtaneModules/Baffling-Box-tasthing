using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;
using rnd = UnityEngine.Random;

public class bafflingBox : MonoBehaviour
{
    public new KMAudio audio;
    public KMBombInfo bomb;
    public KMBombModule module;

    public KMSelectable button;
    public Material[] readMats;
    public Material[] writeMats;
    public Renderer[] YFaces;
    public Renderer[] XFaces;
    public Renderer[] ZFaces;
    public Renderer[] cubes;
    public Renderer[] spheres;
    public Renderer[] cylinders;
    public GameObject hidable;

    private int shapeChosen;
    private int[] colorOrder = new int[3];
    private int[] axisOrder = new int[3];
    private int solution;

    public static readonly string[] shapeNames = new string[] { "cube", "sphere", "cylinder" };
    public static readonly string[] colorNames = new string[] { "green", "orange", "red" };
    private static readonly int[][] table1 = new int[][]
    {
        new int[] { 1, 0, 2 },      
        new int[] { 2, 1, 0 },        
        new int[] { 0, 2, 1 },        
        new int[] { 0, 1, 2 },        
        new int[] { 1, 2, 0 },        
        new int[] { 2, 0, 1 },        
    };
    private static readonly string[] table2 = new string[] { "ROG", "GRO", "OGR", "ORG", "RGO", "GOR" };

    private static int moduleIdCounter = 1;
    private int moduleId;
    private bool moduleSolved;

    private void Awake()
    {
        moduleId = moduleIdCounter++;
        button.OnInteract += delegate () { PressButton(); return false; };
    }

    private void Start()
    {
        shapeChosen = rnd.Range(0, 3);
        colorOrder = Enumerable.Range(0, 3).ToList().Shuffle().ToArray();
        switch (shapeChosen)
        {
            case 0:
                foreach (Renderer sphere in spheres)
                    sphere.gameObject.SetActive(false);
                foreach (Renderer cylinder in cylinders)
                    cylinder.gameObject.SetActive(false);
                break;
            case 1:
                foreach (Renderer cube in cubes)
                    cube.gameObject.SetActive(false);
                foreach (Renderer cylinder in cylinders)
                    cylinder.gameObject.SetActive(false);
                break;
            case 2:
                foreach (Renderer cube in cubes)
                    cube.gameObject.SetActive(false);
                foreach (Renderer sphere in spheres)
                    sphere.gameObject.SetActive(false);
                break;
        }
        Debug.LogFormat("[Baffling Box #{0}] The box contains a {1}.", moduleId, shapeNames[shapeChosen]);
        for (int i = 0; i < 3; i++)
        {
            cubes[i].material = readMats[colorOrder[i]];
            spheres[i].material = readMats[colorOrder[i]];
            cylinders[i].material = readMats[colorOrder[i]];
            switch (i)
            {
                case 0:
                    YFaces[0].material = writeMats[colorOrder[i]];
                    YFaces[1].material = writeMats[colorOrder[i]];
                    break;
                case 1:
                    XFaces[0].material = writeMats[colorOrder[i]];
                    XFaces[1].material = writeMats[colorOrder[i]];
                    break;
                case 2:
                    ZFaces[0].material = writeMats[colorOrder[i]];
                    ZFaces[1].material = writeMats[colorOrder[i]];
                    break;
            }
        }
        Debug.LogFormat("[Baffling Box #{0}] Displayed colors (YZX): {1}.", moduleId, colorOrder.Select(x => colorNames[x]).Join(", "));
        axisOrder = table1[shapeChosen + bomb.GetSerialNumberNumbers().Last() % 2 == 0 ? 0 : 3].ToArray();
        Debug.LogFormat("[Baffling Box #{0}] The axis order is {1}.", moduleId, axisOrder.Select(x => "YZX"[x]).Join(""));
        var str = "";
        for (int i = 0; i < 3; i++)
            str += "GOR"[colorOrder[axisOrder[i]]];
        solution = Array.IndexOf(table2, str);
        Debug.LogFormat("[Baffling Box #{0}] This gives us the colors {1}, and the solution digit of {2}.", moduleId, str, solution);
    }

    private void PressButton()
    {
        button.AddInteractionPunch(.25f);
        audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, button.transform);
        if (moduleSolved)
        {
            hidable.SetActive(false);
            return;
        }
        var submmittedTime = ((int)bomb.GetTime()) % 60 / 10;
        Debug.LogFormat("[Baffling Box #{0}] You submitted when the tens digit was a {1}.", moduleId, submmittedTime);
        if (submmittedTime != solution)
        {
            Debug.LogFormat("[Baffling Box #{0}] That was incorrect. Strike!", moduleId);
            module.HandleStrike();
        }
        else
        {
            Debug.LogFormat("[Baxxffling Box #{0}] That was correct. Module solved!", moduleId);
            module.HandlePass();
            moduleSolved = true;
            audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
        }
    }

    // Twitch Plays
#pragma warning disable 414
    private readonly string TwitchHelpMessage = "!{0} <##> [Press the button when the two seconds digits are ##. Must be a two digit number.]";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string input)
    {
        input = input.Trim();
        var validTimes = Enumerable.Range(0, 60).Select(x => x.ToString("00")).ToArray();
        if (validTimes.Contains(input))
        {
            while (((int)bomb.GetTime()) % 60 != Array.IndexOf(validTimes, input))
                yield return "trycancel";
            yield return null;
            button.OnInteract();
        }
        else
            yield break;
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        while (((int)bomb.GetTime()) % 60 / 10 != solution)
        {
            yield return true;
            yield return null;
        }
        yield return null;
        button.OnInteract();
    }
}
