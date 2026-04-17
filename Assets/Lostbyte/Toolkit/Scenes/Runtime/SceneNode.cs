using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lostbyte.Toolkit.Scenes
{
    public class SceneNode
    {
        public Scene SceneInstance { get; set; }
        public SceneNode Parent { get; private set; }
        public List<SceneNode> Children { get; private set; }
        public string ScenePath => SceneInstance.path;

        public SceneNode(Scene scene, SceneNode parent)
        {
            Parent = parent;
            Children = new List<SceneNode>();
            SceneInstance = scene;
            
            parent?.Children.Add(this);
        }
    }
}
