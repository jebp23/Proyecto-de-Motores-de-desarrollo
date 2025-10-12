using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Notes/Content List")]
public class NoteContentList : ScriptableObject
{
    [TextArea] public List<string> notes = new List<string>();
}
