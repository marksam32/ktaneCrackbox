using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using Crackbox;
using UnityEngine;

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

    private int currentlySelectedItem;
    private bool interactable = true;

    private CrackboxGridItem[] gridItems;
    private CrackboxGridItem[] originalGridItems;

    // Use this for initialization
    void Start()
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
            var solved = CrackboxLogic.IsSolved(gridItems);
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
            BoxTexts[i].text = !gridItem.IsBlack && gridItem.Value != 0 ? gridItem.Value.ToString() : string.Empty;
            BoxRenderers[i].material = gridItem.IsBlack ? GridBoxBlack : GridBoxNormal;
        }

        currentlySelectedItem = UnityEngine.Random.Range(0, gridItems.Length);
        while (gridItems[currentlySelectedItem].IsBlack)
        {
            currentlySelectedItem = UnityEngine.Random.Range(0, gridItems.Length);
        }

        BoxRenderers[currentlySelectedItem].material = GridBoxSelected;
    }

    // Update is called once per frame
    void Update()
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
        if (gridItems[currentlySelectedItem].IsLocked || gridItems[currentlySelectedItem].IsBlack)
        {
            return;
        }

        UpdateGridItem(i + 1);
    }

    private void UpdateGridItem(int value)
    {
        BoxTexts[currentlySelectedItem].text = (value.ToString());
        gridItems[currentlySelectedItem].Value = value;
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

        var nextButton = CrackboxLogic.GetNextIndex(currentlySelectedItem, gridItems, direction);

        BoxRenderers[currentlySelectedItem].material = gridItems[currentlySelectedItem].IsBlack ? GridBoxBlack : GridBoxNormal;
        BoxRenderers[nextButton].material = GridBoxSelected;
        currentlySelectedItem = nextButton;
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = "!{0} ul 2 d 3 r 4 [move selection box and enter numbers] | !{0} check [submit]";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        if (Regex.IsMatch(command, @"^\s*(check|submit|go)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            CheckButton.OnInteract();
            yield return isSolved ? "solve" : "strike";
            yield break;
        }

        var match = Regex.Match(command, @"^\s*(?:move|set)?\s*([udlr]|10|[1-9]| +)+\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (!match.Success)
            yield break;

        var elements = match.Groups[1].Captures.Cast<Capture>().Select(c => c.Value).Where(c => !c.All(ch => ch == ' ')).ToArray();
        yield return null;
        yield return elements.Select(el =>
        {
            int i;
            return
                el == "u" || el == "U" ? ArrowButtons[0] :
                el == "l" || el == "L" ? ArrowButtons[1] :
                el == "r" || el == "R" ? ArrowButtons[2] :
                el == "d" || el == "D" ? ArrowButtons[3] :
                int.TryParse(el, out i) ? NumberedButtons[i - 1] : null;
        });
    }
}
