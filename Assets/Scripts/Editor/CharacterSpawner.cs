using UnityEngine;
using UnityEditor;

public class CharacterSpawner : EditorWindow
{
    private GameObject manPrefab;
    private GameObject girlWithHeelPrefab;
    private GameObject girlNoHeelPrefab;

    [MenuItem("工具/角色放置工具")]
    public static void ShowWindow()
    {
        GetWindow<CharacterSpawner>("角色放置工具");
    }

    private void OnGUI()
    {
        GUILayout.Label("角色放置工具", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        manPrefab = (GameObject)EditorGUILayout.ObjectField("男性角色预制体", manPrefab, typeof(GameObject), false);
        girlWithHeelPrefab = (GameObject)EditorGUILayout.ObjectField("穿高跟鞋女性预制体", girlWithHeelPrefab, typeof(GameObject), false);
        girlNoHeelPrefab = (GameObject)EditorGUILayout.ObjectField("不穿高跟鞋女性预制体", girlNoHeelPrefab, typeof(GameObject), false);

        EditorGUILayout.Space();
        if (GUILayout.Button("放置男性角色"))
        {
            SpawnCharacter(manPrefab);
        }

        if (GUILayout.Button("放置穿高跟鞋女性"))
        {
            SpawnCharacter(girlWithHeelPrefab);
        }

        if (GUILayout.Button("放置不穿高跟鞋女性"))
        {
            SpawnCharacter(girlNoHeelPrefab);
        }
    }

    private void SpawnCharacter(GameObject prefab)
    {
        if (prefab == null)
        {
            EditorUtility.DisplayDialog("错误", "请先选择预制体！", "确定");
            return;
        }

        // 在场景中创建角色
        GameObject character = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        if (character != null)
        {
            // 注册到场景管理器
            EditorSceneCharacterManager.Instance.RegisterCharacter(character);
            
            // 设置位置
            character.transform.position = new Vector3(0, 0, 0);
            
            // 选中新创建的角色
            Selection.activeGameObject = character;
        }
    }
} 