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
    
    private GameObject tempObstaclePrefab;
    private List<GameObject> previewInstances = new List<GameObject>();

    [MenuItem("Tools/Level Editor")]
    public static void ShowWindow()
    {
        GetWindow<LevelEditorWindow>("Level Editor");
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        RefreshRealPreview(); // Tự động hiển thị vật thể nếu code vừa reload
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        ClearPreviewInstances(); // Dọn dẹp sạch sẽ Scene khi đóng Tool
    }

    private void OnGUI()
    {
        GUILayout.Label("Level Editor Tool", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        activeConfig = (LevelConfig)EditorGUILayout.ObjectField("Level Config", activeConfig, typeof(LevelConfig), false);
        if (EditorGUI.EndChangeCheck())
        {
            // Cập nhật lại toàn bộ vật thể khi chuyển sang Level khác
            RefreshRealPreview();
        }

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

        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        
        GUI.backgroundColor = new Color(0.6f, 1f, 0.6f); 
        if (GUILayout.Button("SAVE LEVEL", GUILayout.Height(40)))
        {
            ForceSave();
        }
        GUI.backgroundColor = Color.white; 

        GUI.backgroundColor = new Color(1f, 0.6f, 0.6f);
        if (GUILayout.Button("EXIT", GUILayout.Width(100), GUILayout.Height(40)))
        {
            this.Close(); 
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndHorizontal();
        GUILayout.Space(5);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(activeConfig);
        }
    }

    private void ForceSave()
    {
        if (activeConfig != null)
        {
            EditorUtility.SetDirty(activeConfig);
            AssetDatabase.SaveAssets();
            this.ShowNotification(new GUIContent("Level Saved Successfully!"));
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
        EditorGUILayout.LabelField("Obstacles Settings", EditorStyles.toolbarButton);
        
        EditorGUI.indentLevel++;
        EditorGUI.BeginChangeCheck();
        tempObstaclePrefab = (GameObject)EditorGUILayout.ObjectField("Editor Obstacle Prefab", tempObstaclePrefab, typeof(GameObject), false);
        if (EditorGUI.EndChangeCheck())
        {
            // Cập nhật sinh vật thể thật ngay khi GD kéo Prefab mới vào
            RefreshRealPreview();
        }
        EditorGUI.indentLevel--;

        EditorGUILayout.Space(5);

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

        for (int i = 0; i < activeConfig.obstaclePositions.Length; i++)
        {
            EditorGUI.BeginChangeCheck();
            Vector3 newPos = Handles.PositionHandle(activeConfig.obstaclePositions[i], Quaternion.identity);
            
            // Nếu chưa có Prefab, vẽ cục màu đỏ làm điểm neo
            if (tempObstaclePrefab == null)
            {
                Handles.color = Color.red;
                Handles.CubeHandleCap(0, activeConfig.obstaclePositions[i], Quaternion.identity, 0.5f, EventType.Repaint);
            }

            Handles.Label(activeConfig.obstaclePositions[i] + Vector3.up, $"Obstacle {i}");

            // KHI GD KÉO MŨI TÊN
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(activeConfig, "Move Obstacle Position");
                activeConfig.obstaclePositions[i] = newPos;
                
                // Cập nhật vị trí của vật thể thật tương ứng ngay lập tức
                if (i < previewInstances.Count && previewInstances[i] != null)
                {
                    previewInstances[i].transform.position = newPos;
                }

                Repaint(); 
            }
        }
    }

    // --- CÁC HÀM XỬ LÝ VẬT THỂ THẬT (REAL PREVIEW) ---
    private void RefreshRealPreview()
    {
        ClearPreviewInstances();
        
        if (activeConfig == null || tempObstaclePrefab == null || activeConfig.obstaclePositions == null) return;

        for (int i = 0; i < activeConfig.obstaclePositions.Length; i++)
        {
            GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(tempObstaclePrefab);
            if (obj != null)
            {
                obj.transform.position = activeConfig.obstaclePositions[i];
                // Cờ này báo cho Unity: Hiện trong Hierarchy, nhưng CẤM LƯU vào file Scene
                obj.hideFlags = HideFlags.DontSave; 
                previewInstances.Add(obj);
            }
        }
    }

    private void ClearPreviewInstances()
    {
        foreach (var obj in previewInstances)
        {
            if (obj != null) DestroyImmediate(obj);
        }
        previewInstances.Clear();
    }

    private void AddObstacle()
    {
        var list = new List<Vector3>(activeConfig.obstaclePositions);
        list.Add(Vector3.zero);
        activeConfig.obstaclePositions = list.ToArray();
        RefreshRealPreview(); // Khởi tạo lại list vật thể thật
    }

    private void RemoveObstacle(int index)
    {
        var list = new List<Vector3>(activeConfig.obstaclePositions);
        list.RemoveAt(index);
        activeConfig.obstaclePositions = list.ToArray();
        RefreshRealPreview(); // Khởi tạo lại list vật thể thật
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
            RefreshRealPreview();
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