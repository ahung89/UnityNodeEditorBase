﻿
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// The visual representation of a logic unit such as an object or function.
/// </summary>
public class EditorNode
{
    public static readonly Vector2 kDefaultSize = new Vector2(140f, 110f);

    /// <summary>
    /// The space reserved between knobs.
    /// </summary>
    public const float kKnobOffset = 4f;

    /// <summary>
    /// The space reserved for the header (title) of the node.
    /// </summary>
    public const float kHeaderHeight = 15f;

    /// <summary>
    /// The max label width for a field in the body.
    /// </summary>
    public const float kBodyLabelWidth = 100f;

    /// <summary>
    /// The rect of the node in canvas space.
    /// </summary>
    public Rect bodyRect;

    public string name = "Node";

    /// <summary>
    /// How much additional offset to apply when resizing.
    /// </summary>
    public const float resizePaddingX = 20f;

    private List<EditorOutputKnob> _outputs = new List<EditorOutputKnob>();
    private List<EditorInputKnob> _inputs = new List<EditorInputKnob>();

    public EditorNode()
    {
        bodyRect.size = kDefaultSize;
    }

    public virtual void OnGUI()
    {
        OnNodeHeaderGUI();
        OnKnobGUI();
        onBodyGuiInternal();
    }

    /// <summary>
    /// Renders the knob names. By default, after the header.
    /// </summary>
    public virtual void OnKnobGUI()
    {
        int inputCount = _inputs.Count;
        int outputCount = _outputs.Count;

        int maxCount = (int)Mathf.Max(inputCount, outputCount);

        // The entire knob section is stacked rows of inputs and outputs.
        GUILayout.BeginVertical();

        for (int i = 0; i < maxCount; ++i) {

            GUILayout.BeginHorizontal();

            // Render the knob layout horizontally.
            if (i < inputCount) _inputs[i].OnGUI(i);
            if (i < outputCount) _outputs[i].OnGUI(i);

            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();
    }

    /// <summary>
    /// Render the title/header of the node. By default, renders on top of the node.
    /// </summary>
    public virtual void OnNodeHeaderGUI()
    {
        // Draw header
        GUILayout.Box(name, HeaderStyle);
    }

    /// <summary>
    /// Draws the body of the node. By default, after the knob names.
    /// </summary>
    public virtual void OnBodyGUI() { }

    // Handles the coloring and layout of the body.
    // This is for convenience so the user does not need to worry about this boiler plate code.
    protected virtual void onBodyGuiInternal()
    {
        float oldLabelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = kBodyLabelWidth;

        // Cache the old label style.
        // Do this first before changing the EditorStyles.label style.
        // So the original values are kept.
        var oldLabelStyle = NodeEditor.UnityLabelStyle;

        // Setup new values for the label style.
        EditorStyles.label.normal = DefaultStyle.normal;
        EditorStyles.label.active = DefaultStyle.active;
        EditorStyles.label.focused = DefaultStyle.focused;

        EditorGUILayout.BeginVertical();

        GUILayout.Space(kKnobOffset);
        OnBodyGUI();

        // Revert back to old label style.
        EditorStyles.label.normal = oldLabelStyle.normal;
        EditorStyles.label.active = oldLabelStyle.active;
        EditorStyles.label.focused = oldLabelStyle.focused;

        EditorGUIUtility.labelWidth = oldLabelWidth;
        EditorGUILayout.EndVertical();
    }

    public EditorInputKnob AddInput()
    {
        var input = new EditorInputKnob(this);
        _inputs.Add(input);

        return input;
    }

    public EditorOutputKnob AddOutput()
    {
        var output = new EditorOutputKnob(this);
        _outputs.Add(output);

        return output;
    }

    /// <summary>
    /// Called when the output knob had an input connection removed.
    /// </summary>
    /// <param name="removedInput"></param>
    public virtual void OnInputConnectionRemoved(EditorInputKnob removedInput) { }

    /// <summary>
    /// Called when the output knob made a connection to an input knob.
    /// </summary>
    /// <param name="addedInput"></param>
    public virtual void OnNewInputConnection(EditorInputKnob addedInput) { }

    public IEnumerable<EditorOutputKnob> Outputs
    {
        get { return _outputs; }
    }

    public IEnumerable<EditorInputKnob> Inputs
    {
        get { return _inputs; }
    }

    public int InputCount
    {
        get { return _inputs.Count; }
    }

    public int OutputCount
    {
        get { return _outputs.Count; }
    }

    public EditorInputKnob GetInput(int index)
    {
        return _inputs[index];
    }

    public EditorOutputKnob GetOutput(int index)
    {
        return _outputs[index];
    }

    /// <summary>
    /// Get the Y value of the top header.
    /// </summary>
    public float HeaderTop
    {
        get { return bodyRect.yMin + kHeaderHeight; }
    }

    #region Styles and Contents

    public static GUIStyle _defStyle;
    public static GUIStyle DefaultStyle
    {
        get
        {
            if (_defStyle == null) {
                _defStyle = new GUIStyle(EditorStyles.label);
                _defStyle.normal.textColor = Color.white * 0.9f;
                _defStyle.active.textColor = ColorExtensions.From255(126, 186, 255) * 0.9f;
                _defStyle.focused.textColor = ColorExtensions.From255(126, 186, 255);
            }

            return _defStyle;
        }
    }

    public GUIStyle HeaderStyle
    {
        get
        {
            var style = new GUIStyle();
            
            style.stretchWidth = true;
            style.alignment = TextAnchor.MiddleLeft;
            style.padding.left = 5;
            style.normal.textColor = Color.white * 0.9f;
            style.normal.background = TextureLib.GetTintTex("Square", ColorExtensions.From255(79, 82, 94));
            style.fixedHeight = kHeaderHeight;

            return style;
        }
    }

    /// <summary>
    /// Resize the node to fit the knobs.
    /// </summary>
    public void FitKnobs()
    {
        int maxCount = (int)Mathf.Max(_inputs.Count, _outputs.Count);

        float totalKnobsHeight = maxCount * EditorKnob.kMinSize.y;
        float totalOffsetHeight = (maxCount - 1) * kKnobOffset;

        float heightRequired = totalKnobsHeight + totalOffsetHeight + kHeaderHeight;

        // Add some extra height at the end.
        bodyRect.height = heightRequired + kHeaderHeight / 2f;
    }

    #endregion
}
