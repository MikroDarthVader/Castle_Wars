using System;
using System.Collections.Generic;
using UnityEngine;

public class InputSpeaker : MonoBehaviour
{
    static List<InputListener> listeners;
    private static InputState input;
    private Queue<InputEvent> inputByFrame;
    private Event e;
    private List<string> wasSpecPressed;//need for keyPressed single-time detecting, because e.keyDown generates all time when key/spec/button was pressed
    private List<KeyCode> wasKeyPressed;
    private List<int> wasMousePressed;

    private void Awake()
    {
        listeners = new List<InputListener>();
        input = new InputState();
        inputByFrame = new Queue<InputEvent>();

        wasSpecPressed = new List<string>();
        wasKeyPressed = new List<KeyCode>();
        wasMousePressed = new List<int>();
    }

    void OnGUI()
    {
        e = Event.current;
        while (Event.GetEventCount() > 0)
        {
            Event.PopEvent(e);
            /*checking specs*/
            if (wasSpecPressed.Contains("shift") && !e.shift)
            {
                inputByFrame.Enqueue(new InputEvent("shift", false));
                wasSpecPressed.Remove("shift");
            }
            else if (!wasSpecPressed.Contains("shift") && e.shift)
            {
                inputByFrame.Enqueue(new InputEvent("shift", true));
                wasSpecPressed.Add("shift");
            }

            if (e.isKey)
            {
                if (e.functionKey)
                {
                    if (e.keyCode == KeyCode.LeftControl || e.keyCode == KeyCode.RightControl)
                    {
                        if (e.type == EventType.KeyUp && wasSpecPressed.Contains("ctrl"))
                        {
                            inputByFrame.Enqueue(new InputEvent("ctrl", false));
                            wasSpecPressed.Remove("ctrl");
                        }
                        else if (e.type == EventType.KeyDown && !wasSpecPressed.Contains("ctrl"))
                        {
                            inputByFrame.Enqueue(new InputEvent("ctrl", true));
                            wasSpecPressed.Add("ctrl");
                        }
                    }

                    if (e.keyCode == KeyCode.LeftAlt || e.keyCode == KeyCode.RightAlt)
                    {
                        if (e.type == EventType.KeyUp && wasSpecPressed.Contains("alt"))
                        {
                            inputByFrame.Enqueue(new InputEvent("alt", false));
                            wasSpecPressed.Remove("alt");
                        }
                        else if (e.type == EventType.KeyDown && !wasSpecPressed.Contains("alt"))
                        {
                            inputByFrame.Enqueue(new InputEvent("alt", true));
                            wasSpecPressed.Add("alt");
                        }
                    }
                }
                /*checking specs*/

                /*checking keyboard*/
                else if (e.keyCode != KeyCode.None)
                {
                    if (e.type == EventType.KeyUp && wasKeyPressed.Contains(e.keyCode))
                    {
                        inputByFrame.Enqueue(new InputEvent("keyboard", false, (float)e.keyCode));  //freeze if two keys was released at the same time
                        wasKeyPressed.Remove(e.keyCode);
                    }
                    else if (e.type == EventType.KeyDown && !wasKeyPressed.Contains(e.keyCode))
                    {
                        inputByFrame.Enqueue(new InputEvent("keyboard", true, (float)e.keyCode));
                        wasKeyPressed.Add(e.keyCode);
                    }
                }
                /*checking keyboard*/
            }

            /*checking mouse*/
            if (e.isMouse)
            {
                if (e.type == EventType.MouseUp && wasMousePressed.Contains(e.button))
                {
                    inputByFrame.Enqueue(new InputEvent("mouse", false, e.button));
                    wasMousePressed.Remove(e.button);
                }
                else if (e.type == EventType.MouseDown && !wasMousePressed.Contains(e.button))
                {
                    inputByFrame.Enqueue(new InputEvent("mouse", true, e.button));
                    wasMousePressed.Add(e.button);
                }
            }
            /*checking mouse*/

            /*checking scroll*/
            if (e.isScrollWheel)
            {
                inputByFrame.Enqueue(new InputEvent("scrollDelta", e.delta.y));
            }
            /*checking scroll*/
        }

        /*checking for specs/keyboard/mouse up - errors*/
        if (wasSpecPressed.Contains("shift") && !Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
        {
            inputByFrame.Enqueue(new InputEvent("shift", false));
            wasSpecPressed.Remove("shift");
        }

        if (wasSpecPressed.Contains("ctrl") && !Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))
        {
            inputByFrame.Enqueue(new InputEvent("ctrl", false));
            wasSpecPressed.Remove("ctrl");
        }

        if (wasSpecPressed.Contains("alt") && !Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.RightAlt))
        {
            inputByFrame.Enqueue(new InputEvent("alt", false));
            wasSpecPressed.Remove("alt");
        }

        for (int i = 0; i < wasKeyPressed.Count; i++)
            if (!Input.GetKey(wasKeyPressed[i]))
            {
                inputByFrame.Enqueue(new InputEvent("keyboard", false, (float)e.keyCode));
                wasKeyPressed.Remove(wasKeyPressed[i]);
                i--;
            }

        for (int i = 0; i < wasMousePressed.Count; i++)
            if (!Input.GetMouseButton(wasMousePressed[i]))
            {
                inputByFrame.Enqueue(new InputEvent("mouse", false, e.button));
                wasMousePressed.Remove(wasMousePressed[i]);
                i--;
            }
        /*checking for specs/keyboard/mouse up - errors*/
    }

    void Update()
    {
        while (inputByFrame.Count > 0)
        {
            InputEvent delta = inputByFrame.Dequeue();
            //Debug.Log(delta);
            //Debug.Log(input);
            foreach (InputListener listener in listeners)
            {
                listener.onAction(input, delta);
            }
            input += delta;
        }

        /*string s = "";
        foreach(KeyCode k in input.keyboard)
        {
            s += k + "   ";
        }
        Debug.Log(s);    //log pressed keys*/
    }

    public static void addToListeners(InputListener listener)
    {
        if (listener != null)
        {
            listeners.Add(listener);
        }
    }

    public static List<InputCombination> loadCombinations(string name)
    {
        List<InputCombination> controlsConfig = null;
        if (name == "player") //will be reading from json !!!ONLY FOR TEST
        {
            controlsConfig = new List<InputCombination>();
            controlsConfig.Add(new InputCombination("castle_save", new InputState(false, false, false, new List<KeyCode> { KeyCode.P }, new List<int>(), 0), new InputEvent("keyboard", true, (float)KeyCode.LeftBracket)));
            controlsConfig.Add(new InputCombination("castle_load", new InputState(false, false, false, new List<KeyCode> { KeyCode.P }, new List<int>(), 0), new InputEvent("keyboard", true, (float)KeyCode.RightBracket)));
            controlsConfig.Add(new InputCombination("operate", new InputState(), new InputEvent("mouse", true, 0)));
            controlsConfig.Add(new InputCombination("start_aiming", new InputState(), new InputEvent("mouse", true, 1)));
            controlsConfig.Add(new InputCombination("stop_aiming", new InputState(), new InputEvent("mouse", false, 1)));
            controlsConfig.Add(new InputCombination("actionMode1", new InputState(), new InputEvent("keyboard", false, (float)KeyCode.Alpha1)));
            controlsConfig.Add(new InputCombination("actionMode2", new InputState(), new InputEvent("keyboard", false, (float)KeyCode.Alpha2)));
            controlsConfig.Add(new InputCombination("actionMode3", new InputState(), new InputEvent("keyboard", false, (float)KeyCode.Alpha3)));
        }
        return controlsConfig;
    }
}

public class InputEvent
{
    public bool positive;
    public KeyValuePair<string, float> state;

    public override string ToString()
    {
        return positive + "  " + state.Key + "  " + state.Value;
    }

    public InputEvent(string key, bool positive, float value)
    {
        this.positive = positive;
        state = new KeyValuePair<string, float>(key, value);
    }

    public InputEvent(string key, bool positive)
    {
        this.positive = positive;
        state = new KeyValuePair<string, float>(key, 0);
    }

    public InputEvent(string key, float value)
    {
        positive = value >= 0;
        state = new KeyValuePair<string, float>(key, value);
    }

    public static bool operator ==(InputEvent left, InputEvent right)
    {
        return (left.positive == right.positive && left.state.Key == right.state.Key && left.state.Value == right.state.Value);
    }

    public static bool operator !=(InputEvent left, InputEvent right)
    {
        return !(left == right);
    }

    public override int GetHashCode()
    {
        return positive ? state.Key.Length * 100 + (int)state.Value + 10000 : state.Key.Length * 100 + (int)state.Value - 10000;
    }

    public override bool Equals(object obj)
    {
        return (InputEvent)obj == this;
    }
}

public class InputState
{
    public bool shift;
    public bool ctrl;
    public bool alt;
    public List<KeyCode> keyboard;
    public List<int> mouse;
    public float scrollDelta;

    public InputState(bool shift, bool ctrl, bool alt, List<KeyCode> keyboard, List<int> mouse, float scrollDelta)
    {
        this.shift = shift;
        this.ctrl = ctrl;
        this.alt = alt;
        this.keyboard = keyboard;
        this.mouse = mouse;
        this.scrollDelta = scrollDelta;
    }

    public InputState()
    {
        shift = false;
        ctrl = false;
        alt = false;
        keyboard = new List<KeyCode>();
        mouse = new List<int>();
        scrollDelta = 0;
    }

    public InputState(InputState other)
    {
        shift = other.shift;
        ctrl = other.ctrl;
        alt = other.alt;
        keyboard = new List<KeyCode>(other.keyboard);
        mouse = new List<int>(other.mouse);
        scrollDelta = other.scrollDelta;
    }

    public bool Contains(InputState combination)
    {
        if ((shift == combination.shift) && (ctrl == combination.ctrl) && (alt == combination.alt) &&       //specificators equality
                ((combination.keyboard.Count == 0 || Helper.ListEquals(combination.keyboard, keyboard)) &&  //keys equality
                (combination.mouse.Count == 0 || Helper.ListEquals(combination.mouse, mouse)) &&            //mouse equality   
                (combination.scrollDelta == 0 || combination.scrollDelta != 0)))                            //scroll equality
            return true;
        else
            return false;
    }

    /// <summary>
    /// Use this if InputEvent is positive
    /// </summary>
    /// <param name="inputState"></param>
    /// <param name="inputEvent"></param>
    /// <returns></returns>
    public static InputState operator +(InputState inputState, InputEvent inputEvent)
    {
        if (!inputEvent.positive)
            return inputState - inputEvent;

        switch (inputEvent.state.Key)
        {
            case "shift":
                inputState.shift = true;
                inputState.scrollDelta = 0;
                break;

            case "ctrl":
                inputState.ctrl = true;
                inputState.scrollDelta = 0;
                break;

            case "alt":
                inputState.alt = true;
                inputState.scrollDelta = 0;
                break;

            case "keyboard":
                inputState.keyboard.Add((KeyCode)inputEvent.state.Value);
                inputState.scrollDelta = 0;
                break;

            case "mouse":
                inputState.mouse.Add((int)inputEvent.state.Value);
                inputState.scrollDelta = 0;
                break;

            case "scrollDelta":
                inputState.scrollDelta = inputEvent.state.Value;
                break;
        }
        return inputState;
    }
    /// <summary>
    /// Use this if InputEvent is negative
    /// </summary>
    /// <param name="inputState"></param>
    /// <param name="inputEvent"></param>
    /// <returns></returns>
    public static InputState operator -(InputState inputState, InputEvent inputEvent)
    {
        if (inputEvent.positive)
            return inputState + inputEvent;

        switch (inputEvent.state.Key)
        {
            case "shift":
                inputState.shift = false;
                break;

            case "ctrl":
                inputState.ctrl = false;
                break;

            case "alt":
                inputState.alt = false;
                break;

            case "keyboard":
                inputState.keyboard.Remove((KeyCode)inputEvent.state.Value);
                break;

            case "mouse":
                inputState.mouse.Remove((int)inputEvent.state.Value);
                break;
        }
        return inputState;
    }

    public override string ToString()
    {
        return shift + "  " + ctrl + "  " + alt + "  {" + String.Join(",", keyboard.ConvertAll(i => i.ToString()).ToArray()) +
                                                    "}  {" + String.Join(",", mouse.ConvertAll(i => i.ToString()).ToArray()) +
                                                    "}  " + scrollDelta;
    }
}

public struct InputCombination
{
    public readonly string name;
    public readonly InputState command;
    public readonly InputEvent delta;

    public InputCombination(string name, InputState command, InputEvent delta)
    {
        this.name = name;
        this.command = command;
        this.delta = delta;
    }
}

public class InputListener
{
    public delegate void Action();
    public readonly Dictionary<InputCombination, Action> inputActions;
    public readonly string name;

    public InputListener(object handler, string name)
    {
        inputActions = new Dictionary<InputCombination, Action>();
        this.name = name;
        foreach (InputCombination combination in InputSpeaker.loadCombinations(name))
        {
            addCombination(combination, (Action)Delegate.CreateDelegate(typeof(Action), handler, combination.name));
        }
        InputSpeaker.addToListeners(this);
    }

    public void addCombination(InputCombination combination, Action action)
    {
        inputActions.Add(combination, action);
    }

    public string getName()
    {
        return name;
    }

    public void onAction(InputState input, InputEvent delta)
    {
        foreach (KeyValuePair<InputCombination, Action> actionPair in inputActions)
        {
            if (input.Contains(actionPair.Key.command) && delta == actionPair.Key.delta)
            {
                actionPair.Value();
            }
        }
    }
}