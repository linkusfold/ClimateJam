using Game_Manager;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(GameManager))]
public class GMInspector : Editor
{
    public override void OnInspectorGUI()
    {

        GameManager gameManager = (GameManager)target;
        base.OnInspectorGUI();

        if (GUILayout.Button("Destroy all enemies"))
        {
            gameManager.DestroyAllEnemies();
        }

        if (GUILayout.Button("Stop all enemies"))
        {
            gameManager.PauseAllEnemies();
        }

        if (GUILayout.Button("Start all enemies"))
        {
            gameManager.ResumeAllEnemies();
        }
    }
}
#endif