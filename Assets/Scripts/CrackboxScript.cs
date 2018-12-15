using UnityEngine;
using Crackbox;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public class CrackboxScript : MonoBehaviour
{
    public KMAudio Audio;
    public KMBombModule Module;

    public KMSelectable[] ArrowButtons;
    public KMSelectable[] NumberedButtons;
    public KMSelectable CheckButton;

    public MeshRenderer[] BoxRenderers;
    public TextMesh[] BoxTexts;
    public Material GridBoxBlack;
    public Material GridBoxNormal;
    public Material GridBoxSelected;
    public Material GridBoxWrong;

    private static int _moduleIdCounter = 1;
    private int _moduleId;
    private bool isSolved = false;
    private static readonly Regex SetRegEx = new Regex("^set ([1-9]|10)$");
    private static readonly Regex MoveRegEx = new Regex("^move ([u|d|l|r]+)$");
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = "To move the selected box, do: !{0} move ullr to move it up, left, left, and right. To set a number, do: set #. To submit an answer do: !{0} check.";
    #pragma warning restore 414

    private int currentlySelectedItem;
    private bool interactable = true;

    private CrackboxGridItem[] gridItems;
    private CrackboxGridItem[] originalGridItems;

    // Use this for initialization
    void Start ()
    {
        _moduleId = _moduleIdCounter++;
        Module.OnActivate += Activate;
	}

    void Activate()
    {
        InitGrid();
        ReinitializeGrid();

        for (int i = 0; i < 4; i++)
        {
            int j = i;
            ArrowButtons[i].OnInteract += delegate
            {
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
                ArrowButtons[j].AddInteractionPunch(0.4f);
                if (isSolved || !interactable)
                {
                    return false;
                }
                OnArrowPress(j);
                return false;
            };
        }

        for (int i = 0; i < 10; i++)
        {           
            int j = i;
            NumberedButtons[i].OnInteract += delegate
            {
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
                NumberedButtons[j].AddInteractionPunch(0.4f);
                if (isSolved || !interactable)
                {
                    return false;
                }
                OnNumberButtonPress(j);
                return false;
            };
        }

        CheckButton.OnInteract += delegate
        {            
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
            CheckButton.AddInteractionPunch();
            if (isSolved || !interactable)
            {
                return false;
            }
            var solved = CrackboxLogic.IsSolved(this.gridItems);
            Debug.LogFormat("[Crackbox #{0}] Submitted:", _moduleId);
            GridDebugLog(gridItems, x => string.Format("{0}", (x.IsBlack ? "B" : (x.Value == 0 ? "*" : x.Value.ToString()))));
            if (solved)
            {
                isSolved = true;
                Debug.LogFormat("[Crackbox #{0}] That is correct, module solved!", _moduleId);
                StartCoroutine("SolveAnimation", GridBoxSelected);
            }
            else
            {
                Debug.LogFormat("[Crackbox #{0}] That is incorrect, strike!", _moduleId);
                StartCoroutine("SolveAnimation", GridBoxWrong);
            }
            return false;
        };
    }

    private void ReinitializeGrid()
    {
        for (int i = 0; i < gridItems.Length; i++)
        {
            var gridItem = gridItems[i];
            this.BoxTexts[i].text = !gridItem.IsBlack && gridItem.Value != 0 ? gridItem.Value.ToString() : string.Empty;
            this.BoxRenderers[i].material = gridItem.IsBlack ? GridBoxBlack : GridBoxNormal;
        }

        this.currentlySelectedItem = UnityEngine.Random.Range(0, gridItems.Length);
        while (gridItems[this.currentlySelectedItem].IsBlack)
        {
            this.currentlySelectedItem = UnityEngine.Random.Range(0, gridItems.Length);
        }

        this.BoxRenderers[this.currentlySelectedItem].material = GridBoxSelected;
    }

    // Update is called once per frame
    void Update ()
    {    		
	}

    private void InitGrid()
    {
        originalGridItems = null;
        var solutionFinder = new CrackboxSolutionFinder();
        while (originalGridItems == null)
        {
            originalGridItems = solutionFinder.FindSolution(CrackboxLogic.CreateGrid().ToArray());
        }

        Debug.LogFormat("[Crackbox #{0}] One possible solution:", _moduleId);
        GridDebugLog(originalGridItems, x => string.Format("{0}", (x.IsBlack ? "B" : x.Value.ToString())));

        CrackboxSolutionFinder.Anonymize(originalGridItems);
        gridItems = CrackboxGridItem.Clone(originalGridItems);

        Debug.LogFormat("[Crackbox #{0}] Initial grid:", _moduleId);
        GridDebugLog(originalGridItems, x => string.Format("{0}", (x.IsBlack ? "B" : (x.Value == 0 ? "*" : x.Value.ToString()))));
    }

    private void GridDebugLog(CrackboxGridItem[] items, Func<CrackboxGridItem, string> func)
    {
        for (var rowNumber = 0; rowNumber < items.Count() / 4; ++rowNumber)
        {
            var row = items.Skip(rowNumber * 4).Take(4);
            Debug.LogFormat("[Crackbox #{0}] {1}", _moduleId, string.Join(", ", row.Select(func).ToArray()));
        }
    }

    private void OnNumberButtonPress(int i)
    {
        if (gridItems[this.currentlySelectedItem].IsLocked || gridItems[currentlySelectedItem].IsBlack)
        {
            return;
        }

        UpdateGridItem(i + 1);
    }

    private void UpdateGridItem(int value)
    {
        BoxTexts[currentlySelectedItem].text = (value.ToString());
        gridItems[this.currentlySelectedItem].Value = value;
    }

    private IEnumerator SolveAnimation(Material material)
    {
        interactable = false;
        BoxRenderers[currentlySelectedItem].material = GridBoxNormal;
        for (int i = 0; i < gridItems.Length; i++)
        {
            BoxTexts[i].text = string.Empty;
            BoxRenderers[i].material = material;
            yield return new WaitForSeconds(0.15f);
        }

        for (int j = 0; j < 4; j++)
        {
            BoxRenderers.ForEach(x => x.material = GridBoxBlack);
            yield return new WaitForSeconds(0.1f);

            BoxRenderers.ForEach(x => x.material = material);
            yield return new WaitForSeconds(0.1f);
        }

        if (isSolved)
        {
            Module.HandlePass();
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
            interactable = true;
        }
        else
        {
            Module.HandleStrike();
            gridItems = originalGridItems;
            yield return new WaitForSeconds(0.5f);
            ReinitializeGrid();
            interactable = true;
        }

    }

    private void OnArrowPress(int i)
    {
        ArrowButtonDirection direction = ArrowButtonDirection.Up;
        switch (i)
        {
            case 0:
                direction = ArrowButtonDirection.Up;
                break;
            case 1:
                direction = ArrowButtonDirection.Left;
                break;
            case 2:
                direction = ArrowButtonDirection.Right;
                break;
            case 3:
                direction = ArrowButtonDirection.Down;
                break;
            default:
                throw new InvalidOperationException();
        }

        var nextButton = CrackboxLogic.GetNextIndex(this.currentlySelectedItem, this.gridItems, direction);

        BoxRenderers[this.currentlySelectedItem].material = gridItems[this.currentlySelectedItem].IsBlack ? GridBoxBlack : GridBoxNormal;
        BoxRenderers[nextButton].material = GridBoxSelected;
        this.currentlySelectedItem = nextButton;
    }

    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant().Trim();
        var setMatch = SetRegEx.Match(command);
        var moveMatch = MoveRegEx.Match(command);
        if (setMatch.Success)
        {
            yield return null;
            NumberedButtons[int.Parse(setMatch.Groups[1].Value) - 1].OnInteract();
            yield break;
        }

        if (moveMatch.Success)
        {
            var moves = moveMatch.Groups[1].Value;
            foreach(char move in moves)
            {
                yield return null;
                ArrowButtons[MoveCharToInt(move)].OnInteract();
                yield return new WaitForSeconds(0.2f);
            }
            yield break;
        }

        if (command.Equals("submit") || command.Equals("check"))
        {
            yield return null;
            CheckButton.OnInteract();
            if (isSolved)
            {
                yield return "solve";
            }
            else
            {
                yield return "strike";
            }
            yield break;
        }
        yield break;
    }

    private int MoveCharToInt(char c)
    {
        switch (c)
        {
            case 'u':
                return 0;
            case 'l':
                return 1;
            case 'r':
                return 2;
            default:
                return 3;
        }
    }
}
