/// <summary>
/// 启动时直接进入Level01并Play
/// </summary>
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SteelRain.EditorTools
{
    public static class PlayLevel01
    {
        [MenuItem("Steel Rain/Play Level01")]
        public static void PlayLevel01Scene()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Level01_VerticalSlice.unity", OpenSceneMode.Single);
            EditorApplication.isPlaying = true;
        }
    }
}
