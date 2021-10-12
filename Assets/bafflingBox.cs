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
    private int[] shapeOrder = new int[3];
    private int[] colorOrder = new int[3];
    private int[] axisOrder = new int[3];
    private int solution;

    public static readonly string[] shapeNames = new string[] { "cube", "sphere", "cylinder" };
    public static readonly string[] colorNames = new string[] { "green", "orange", "red" };
    private static readonly int[][] table1 = new int[][]
    {
        new int[] { 2, 0, 1 },
        new int[] { 1, 2, 0 },
        new int[] { 0, 1, 2 },
        new int[] { 0, 2, 1 },
        new int[] { 2, 1, 0 },
        new int[] { 1, 0, 2 },
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
        colorOrder = Enumerable.Range(0, 3).ToList().Shuffle().ToArray();
        shapeOrder = Enumerable.Range(0, 3).ToList().Shuffle().ToArray();
        for (int i = 0; i < 3; i++)
        {
            if (shapeOrder[i] != 0)
                cubes[i].gameObject.SetActive(false);
            if (shapeOrder[i] != 1)
                spheres[i].gameObject.SetActive(false);
            if (shapeOrder[i] != 2)
                cylinders[i].gameObject.SetActive(false);
        }
        Debug.LogFormat("[Baffling Box #{0}] Order of shapes: {1}.", moduleId, shapeOrder.Select(x => shapeNames[x]).Join(", "));
        int step1Index;
        var lits = bomb.GetOnIndicators().Count();
        var unlits = bomb.GetOffIndicators().Count();
        if (lits < unlits)
            step1Index = 0;
        else if (lits > unlits)
            step1Index = 1;
        else
            step1Index = 2;
        shapeChosen = shapeOrder[step1Index];
        Debug.LogFormat("[Baffling Box #{0}] Consider the shape in the {1} direction, which is a {2}.", moduleId, "YZX"[step1Index], shapeNames[shapeChosen]);
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
        var add = bomb.GetSerialNumberNumbers().Last() % 2 == 0 ? 0 : 3;
        axisOrder = table1[shapeChosen + add].ToArray();
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
