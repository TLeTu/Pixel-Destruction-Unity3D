using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class LevelEditorWindow : EditorWindow
{
    private LevelConfig activeConfig;
    private Vector2 scrollPos;
    private Editor blockDataEditor; 
    private Editor weaponEditor;
    private bool showWeaponEditor = true;

    [MenuItem("Tools/Level Editor")]
    public static void ShowWindow()
    {
        GetWindow<LevelEditorWindow>("Level Editor");
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnGUI()
    {
        GUILayout.Label("Level Editor Tool", EditorStyles.boldLabel);

        activeConfig = (LevelConfig)EditorGUILayout.ObjectField("Level Config", activeConfig, typeof(LevelConfig), false);

        if (activeConfig == null)
        {
            if (GUILayout.Button("Create New Level Config", GUILayout.Height(30)))
            {
                CreateNewLevelConfig();
            }
            return;
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        DrawLevelSettings();
        EditorGUILayout.Space(15);

        DrawBlockManager();
        EditorGUILayout.Space(15);

        DrawObstacleManager();

        EditorGUILayout.EndScrollView();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(activeConfig);
            AssetDatabase.SaveAssets();
        }
    }

    private void DrawLevelSettings()
    {
        EditorGUILayout.LabelField("Level Settings", EditorStyles.toolbarButton);
        
        EditorGUI.indentLevel++;
        activeConfig.targetDestroyCount = EditorGUILayout.IntField("Target Destroy Count", activeConfig.targetDestroyCount);
        activeConfig.scoreThreshold = EditorGUILayout.IntField("Score Threshold", activeConfig.scoreThreshold);
        activeConfig.initialWeaponCount = EditorGUILayout.IntField("Initial Weapon Count", activeConfig.initialWeaponCount);
        EditorGUI.indentLevel--;

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Tap Damage Settings", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        activeConfig.damageRadius = EditorGUILayout.FloatField("Damage Radius", activeConfig.damageRadius);
        activeConfig.minTapDamage = EditorGUILayout.IntField("Min Tap Damage", activeConfig.minTapDamage);
        activeConfig.maxTapDamage = EditorGUILayout.IntField("Max Tap Damage", activeConfig.maxTapDamage);
        EditorGUI.indentLevel--;

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Weapon Settings", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        activeConfig.weaponPrefab = (GameObject)EditorGUILayout.ObjectField("Weapon Prefab", activeConfig.weaponPrefab, typeof(GameObject), false);
        
        if (activeConfig.weaponPrefab != null)
        {
            showWeaponEditor = EditorGUILayout.Foldout(showWeaponEditor, "Edit Weapon Parameters");
            if (showWeaponEditor)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                
                var weaponLogic = activeConfig.weaponPrefab.GetComponent<IWeaponController>() as MonoBehaviour;
                
                if (weaponLogic != null)
                {
                    Editor.CreateCachedEditor(weaponLogic, null, ref weaponEditor);
                    
                    EditorGUI.indentLevel++;
                    weaponEditor.OnInspectorGUI();
                    EditorGUI.indentLevel--;
                }
                else
                {
                    EditorGUILayout.HelpBox("LỖI: Prefab này không có script nào kế thừa IWeaponController!", MessageType.Error);
                }
                
                EditorGUILayout.EndVertical();
            }
        }
        EditorGUI.indentLevel--;
    }

    private void DrawBlockManager()
    {
        EditorGUILayout.LabelField("Blocks Config", EditorStyles.toolbarButton);
        
        for (int i = 0; i < activeConfig.blocksToSpawn.Count; i++)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.BeginHorizontal();
            
            activeConfig.blocksToSpawn[i] = (BlockData)EditorGUILayout.ObjectField($"Block Slot {i}", activeConfig.blocksToSpawn[i], typeof(BlockData), false);
            
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                activeConfig.blocksToSpawn.RemoveAt(i);
                break; 
            }
            EditorGUILayout.EndHorizontal();

            if (activeConfig.blocksToSpawn[i] != null)
            {
                Editor.CreateCachedEditor(activeConfig.blocksToSpawn[i], null, ref blockDataEditor);
                
                EditorGUI.indentLevel++;
                blockDataEditor.OnInspectorGUI();
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Empty Slot", GUILayout.Height(25)))
        {
            activeConfig.blocksToSpawn.Add(null);
        }
        
        if (GUILayout.Button("Create New Block Data", GUILayout.Height(25)))
        {
            CreateNewBlockData();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawObstacleManager()
    {
        EditorGUILayout.LabelField("Obstacles (Drag & Drop in Scene View)", EditorStyles.toolbarButton);
        
        if (activeConfig.obstaclePositions == null) activeConfig.obstaclePositions = new Vector3[0];

        for (int i = 0; i < activeConfig.obstaclePositions.Length; i++)
        {
            EditorGUILayout.BeginHorizontal();
            activeConfig.obstaclePositions[i] = EditorGUILayout.Vector3Field($"Obstacle {i}", activeConfig.obstaclePositions[i]);
            
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                RemoveObstacle(i);
                break;
            }
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Add New Obstacle", GUILayout.Height(25)))
        {
            AddObstacle();
        }
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (activeConfig == null || activeConfig.obstaclePositions == null) return;

        Handles.color = Color.red;

        for (int i = 0; i < activeConfig.obstaclePositions.Length; i++)
        {
            EditorGUI.BeginChangeCheck();
            
            Vector3 newPos = Handles.PositionHandle(activeConfig.obstaclePositions[i], Quaternion.identity);
            Handles.CubeHandleCap(0, activeConfig.obstaclePositions[i], Quaternion.identity, 0.5f, EventType.Repaint);
            Handles.Label(activeConfig.obstaclePositions[i] + Vector3.up, $"Obstacle {i}");

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(activeConfig, "Move Obstacle Position");
                activeConfig.obstaclePositions[i] = newPos;
                Repaint(); 
            }
        }
    }

    private void AddObstacle()
    {
        var list = new List<Vector3>(activeConfig.obstaclePositions);
        list.Add(Vector3.zero);
        activeConfig.obstaclePositions = list.ToArray();
    }

    private void RemoveObstacle(int index)
    {
        var list = new List<Vector3>(activeConfig.obstaclePositions);
        list.RemoveAt(index);
        activeConfig.obstaclePositions = list.ToArray();
    }

    private void CreateNewLevelConfig()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save Level Config", "Level_New", "asset", "");
        if (!string.IsNullOrEmpty(path))
        {
            LevelConfig newConfig = CreateInstance<LevelConfig>();
            AssetDatabase.CreateAsset(newConfig, path);
            AssetDatabase.SaveAssets();
            activeConfig = newConfig; 
        }
    }

    private void CreateNewBlockData()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save New Block Data", "NewBlockData", "asset", "");
        
        if (!string.IsNullOrEmpty(path))
        {
            BlockData newBlock = CreateInstance<BlockData>();
            AssetDatabase.CreateAsset(newBlock, path);
            AssetDatabase.SaveAssets();
            activeConfig.blocksToSpawn.Add(newBlock);
        }
    }
}