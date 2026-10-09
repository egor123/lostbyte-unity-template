using UnityEngine;

namespace Lostbyte.Toolkit.Scenes
{
    public class ActiveSceneSetter : MonoBehaviour
    {
        private void Start() => UpdateActiveScene();

        public void UpdateActiveScene()
        {
            UnityEngine.SceneManagement.SceneManager.SetActiveScene(gameObject.scene);
        }
    }
}
