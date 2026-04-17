using System.Collections;
using System.Collections.Generic;
using Lostbyte.Toolkit.Common;
using UnityEngine;

namespace Lostbyte.Toolkit.Scenes
{
    public class SceneLoader : MonoBehaviour
    {
        public void LoadSceneAsync(SceneReference scene)
        {
            SceneManager.Instance.LoadSceneAsync(scene, gameObject.scene).Forget();
        }
    }
}
