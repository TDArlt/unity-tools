using UnityEngine;
using UnityEditor;

//[CreateAssetMenu(menuName = "Tools/Create Scratch List")] 
public class ScratchList : ScriptableObject
{
    [Header("Your personal Scratch List")]
    [Comment("Note that the list isn't stored; will only last for this session")]
    public string Comments = "";

    public Object Position1;
    public Object Position2;
    public Object Position3;
    public Object Position4;
    public Object Position5;
    public Object Position6;
    public Object Position7;
    public Object Position8;
    public Object Position9;

    public Object[] More;

    [TextArea]
    public string PersonalNotes = "";


    [MenuItem("Tools/Scratch List")]
    static void DoIt()
    {
        ScratchList list = AssetDatabase.LoadAssetAtPath<ScratchList>("Assets/Scripts/Editor/Tools/ScratchList.asset");
        Selection.activeObject = list;

        //EditorUtility.SetDirty(list);
    }
}


/// <summary>
/// This is an attribute for the inspector enabling you to insert a comment anywhere you like
/// </summary>
public class CommentAttribute : PropertyAttribute
{
    public readonly string Tooltip;
    public readonly string Comment;

    /// <summary>
    /// This is an attribute for the inspector enabling you to insert a comment anywhere you like
    /// </summary>
    /// <param name="comment">is the comment you like to note down</param>
    /// <param name="tooltip">is a tooltip you like to show here as well (optional)</param>
    public CommentAttribute(string comment, string tooltip)
    {
        Tooltip = tooltip;
        Comment = comment;
    }

    /// <summary>
    /// This is an attribute for the inspector enabling you to insert a comment anywhere you like
    /// </summary>
    /// <param name="comment">is the comment you like to note down</param>
    public CommentAttribute(string comment)
    {
        Tooltip = "";
        Comment = comment;
    }
}


/// <summary>This one draws a comment area into the inspector window</summary>
[CustomPropertyDrawer(typeof(CommentAttribute))]
public class CommentDrawer : PropertyDrawer
{
    const int textHeight = 20;

    /// <summary>The current attribute</summary>
    CommentAttribute CommentAttribute { get { return (CommentAttribute)attribute; } }


    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
    {
        return textHeight;
    }


    public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
    {
        EditorGUI.LabelField(position, new GUIContent(CommentAttribute.Comment, CommentAttribute.Tooltip));
    }
}
