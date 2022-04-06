using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using rnd = UnityEngine.Random;

public class ColoredLetters : MonoBehaviour
{
	public KMBombModule Module;
    public KMAudio Audio;
	
    public KMSelectable[] keys;
    public TextMesh[] texts;
    public Color[] colors;
    string[][] letterSequences = new string[4][];
    int[][] colorSequences = new int[4][];
    int[] scores = new int[4];
    string[] alphabet = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M","N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"};
    
	int[][] startingScores = new int[][]
    {
        new int[] {5,4,8,3},
        new int[] {3,2,9,6},
        new int[] {1,2,7,5},
        new int[] {3,6,5,1},
        new int[] {2,3,7,4},
        new int[] {1,4,6,9},
        new int[] {8,3,7,2},
        new int[] {4,1,3,4},
        new int[] {2,1,1,6},
        new int[] {8,4,9,5},
        new int[] {2,6,9,2},
        new int[] {5,4,4,3},
        new int[] {8,5,6,4},
        new int[] {3,2,8,2},
        new int[] {5,1,7,1},
        new int[] {1,6,1,3},
        new int[] {1,5,7,8},
        new int[] {4,8,3,6},
        new int[] {7,8,3,9},
        new int[] {3,6,6,7},
        new int[] {8,5,5,6},
        new int[] {3,6,1,6},
        new int[] {2,3,2,8},
        new int[] {4,9,2,1},
        new int[] {2,7,2,2},
        new int[] {8,9,3,7}
    };
	
    int[][] subsequentScores = new int[][]
    {
        new int[] {3,1,6,7},
        new int[] {7,2,2,8},
        new int[] {2,2,3,6},
        new int[] {5,9,6,3},
        new int[] {4,1,8,5},
        new int[] {4,2,6,1},
        new int[] {2,6,7,3},
        new int[] {3,4,8,1},
        new int[] {5,6,3,9},
        new int[] {1,7,8,5},
        new int[] {3,6,1,8},
        new int[] {9,4,8,8},
        new int[] {3,8,8,6},
        new int[] {2,4,1,3},
        new int[] {5,2,7,1},
        new int[] {4,1,4,2},
        new int[] {1,6,8,2},
        new int[] {3,5,9,2},
        new int[] {7,2,9,2},
        new int[] {1,2,6,4},
        new int[] {7,9,2,8},
        new int[] {3,6,1,4},
        new int[] {3,4,7,2},
        new int[] {8,9,4,7},
        new int[] {3,9,4,9},
        new int[] {3,2,8,4}
    };
	
    List<int> order = new List<int>(), Pressed = new List<int>();
    int Stage;
	
    bool[] correct = new bool[4];
    string[] messagePool = new string[] { "F**K", "GOOD", "WOW!", "NICE", "GG:)", "PRUZ", "POG!", "ABCD", "XEL.", "LETR", "THX!", "SLVD", "COOL", "YEAH", "UWIN"};
    string[] colorNames = new string[] { "Red", "Yellow", "Green", "Blue" };
    
	
	//Logging
    int moduleId;
    static int ModuleIdCounter = 1;
    bool solved;

    void Awake()
	{
        moduleId = ModuleIdCounter++;
        for (int i = 0; i < 4; i++)
        {
            int j = i;
            keys[j].OnInteract += delegate { PressKey(j); return false; };
        }
        Module.OnActivate += delegate { Activate(); };
	}
	
	void PressKey(int Press)
	{
		keys[Press].AddInteractionPunch(0.2f);
		Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, keys[Press].transform);
		if (!solved)
        {
			if (scores[Press] == order[Stage] && new[] {Press}.Any(c => !Pressed.Contains(c)))
            {
                Debug.LogFormat("[Colored Letters #{0}] You pressed Button {1}. That was correct.", moduleId, Press + 1);
				Stage++;
				Pressed.Add(Press);
				if (Stage == 4)
				{
					int SolveText = rnd.Range(0, messagePool.Length);
					Debug.LogFormat("[Colored Letters #{0}] Module solved.", moduleId);
                    Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
                    Module.HandlePass();
					StopAllCoroutines();
					for (int x = 0; x < 4; x++)
					{
						texts[x].text = messagePool[SolveText][x].ToString();
						texts[x].color = Color.green;
					}
                    solved = true;
				}
			}
			
			else
			{
				Debug.LogFormat("[Colored Letters #{0}] You pressed Button {1}. That was incorrect. The module resets", moduleId, Press + 1);
				for (int i = 0; i < 4; i++)
				{
					scores[i] = 0;
				}
				Module.HandleStrike();
				StopAllCoroutines();
                Activate();
			}
		}
	}
	
	void Activate()
	{
		Stage = 0;
		GenerateScores();
		order = scores.ToList();
        order.Sort();
        order.Reverse();
		Pressed = new List<int>();
		Debug.LogFormat("[Colored Letters #{0}] -------------------------------------------------------", moduleId);
		for (int i = 0; i < 4; i++)
        {
            string loggingString = "";
            for (int j = 0; j < letterSequences[i].Length - 1; j++)
            {
                loggingString += colorNames[colorSequences[i][j]] + " " + letterSequences[i][j];
                if (j != letterSequences[i].Length - 2) loggingString += ", ";
            }
            Debug.LogFormat("[Colored Letters #{0}] Button {1}'s character string is {2}", moduleId, i + 1, loggingString);
        }
		Debug.LogFormat("[Colored Letters #{0}] -------------------------------------------------------", moduleId);
		Debug.LogFormat("[Colored Letters #{0}] The scores of the buttons in order are {1}, {2}, {3}, and {4}.", moduleId, scores[0], scores[1], scores[2], scores[3]);
		Debug.LogFormat("[Colored Letters #{0}] -------------------------------------------------------", moduleId);
	}
	
	void GenerateScores()
	{
		for (int i = 0; i < 4; i++)
		{
			letterSequences[i] = new string[rnd.Range(5, 8)];
			colorSequences[i] = new int[letterSequences[i].Length];
			for (int j = 0; j < letterSequences[i].Length - 1; j++)
			{
				colorSequences[i][j] = rnd.Range(0, 4);
				letterSequences[i][j] = alphabet[rnd.Range(0, 26)];
				scores[i] += j == 0 ? startingScores[Array.IndexOf(alphabet, letterSequences[i][j])][colorSequences[i][j]] : subsequentScores[Array.IndexOf(alphabet, letterSequences[i][j])][colorSequences[i][j]];
			}
			StartCoroutine(ShowSequence(i));
		}
	}
	
	IEnumerator ShowSequence(int posIndex)
    {
        int sequenceIndex = rnd.Range(0, letterSequences[posIndex].Length);
        while (!correct[posIndex])
        {
            texts[posIndex].text = letterSequences[posIndex][sequenceIndex];
            texts[posIndex].color = colors[colorSequences[posIndex][sequenceIndex]];
            sequenceIndex++;
            if (sequenceIndex == letterSequences[posIndex].Length)
			{
				texts[posIndex].text = "";
				sequenceIndex = 0;
			}
			yield return new WaitForSecondsRealtime(0.5f);  
        }
    }
	
	#pragma warning disable 414
	private string TwitchHelpMessage = "Use !{0} press [1-4] to press the buttons in reading order (The button press length must be 4 + must not have a duplicate).";
	#pragma warning restore 414
	
	string Number = "1234";
    IEnumerator ProcessTwitchCommand(string command)
    {
		string[] parameters = command.Split(' ');
		if (Regex.IsMatch(parameters[0], @"^\s*press\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
			yield return null;
			if (parameters.Length != 2)
			{
				yield return "sendtochaterror Invalid parameter length. The command was not processed.";
				yield break;
			}

			if (parameters[1].Length != 4)
			{
				yield return "sendtochaterror Invalid button press length. The command was not processed.";
				yield break;
			}
			
			if (parameters[1].Distinct().Count() != 4 || !parameters[1].ToCharArray().All(c => Number.Contains(c)))
			{
				yield return "sendtochaterror Command contains an invalid/duplicate button number. The command was not processed.";
				yield break;
			}
			
			for (int x = 0; x < 4; x++)
			{
				keys[Int32.Parse(parameters[1][x].ToString()) - 1].OnInteract();
				yield return new WaitForSecondsRealtime(0.2f);
			}
		}
	}

    IEnumerator TwitchHandleForcedSolve()
    {
        int start = Stage;
        for (int i = start; i < 4; i++)
        {
            List<int> presses = new List<int>();
            for (int j = 0; j < 4; j++)
            {
                if (scores[j] == order[i] && !Pressed.Contains(j))
                    presses.Add(j);
            }
            keys[presses.PickRandom()].OnInteract();
            yield return new WaitForSecondsRealtime(0.2f);
        }
    }
}
