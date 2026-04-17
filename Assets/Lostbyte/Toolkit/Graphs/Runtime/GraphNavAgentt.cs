using System.Collections;
using System.Collections.Generic;
using Lostbyte.Toolkit.CustomEditor;
using UnityEngine;

namespace Lostbyte.Toolkit.Graphs
{
    public class GraphNavAgentt : MonoBehaviour
    {
        [field: SerializeField, Autowired(Autowired.Type.Parent), EditModeOnly] public Graph Graph { get; private set; } 
    }
}
