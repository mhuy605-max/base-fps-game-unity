using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FPSGame.Editor
{
    public static class FPSGameSetup
    {
        const string ScenePath = "Assets/Scenes/FPS_Scene.unity";

        [MenuItem("FPS/Create FPS Scene")]
        public static void CreateFPSSceneMenu()
        {
            CreateFPSScene(openAfterCreate: true);
        }

        public static void CreateFPSScene(bool openAfterCreate = false)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateLighting();
            CreateArena();
            GameObject player = CreatePlayer();
            CreateTargets(8);
            CreateEnemies(3);
            CreateGameManager(player.transform.position);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (openAfterCreate)
                EditorSceneManager.OpenScene(ScenePath);

            Debug.Log($"FPS scene created at {ScenePath}. Press Play to test.");
        }

        static void CreateLighting()
        {
            GameObject lightGo = new GameObject("Directional Light");
            Light light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        static void CreateArena()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(4f, 1f, 4f);
            ground.GetComponent<Renderer>().sharedMaterial = CreateColorMaterial(new Color(0.25f, 0.35f, 0.25f));

            CreateWall("Wall_North", new Vector3(0f, 1.5f, 20f), new Vector3(40f, 3f, 1f));
            CreateWall("Wall_South", new Vector3(0f, 1.5f, -20f), new Vector3(40f, 3f, 1f));
            CreateWall("Wall_East", new Vector3(20f, 1.5f, 0f), new Vector3(1f, 3f, 40f));
            CreateWall("Wall_West", new Vector3(-20f, 1.5f, 0f), new Vector3(1f, 3f, 40f));

            CreateCover("Cover_1", new Vector3(-8f, 0.75f, 6f), new Vector3(3f, 1.5f, 1f));
            CreateCover("Cover_2", new Vector3(8f, 0.75f, -5f), new Vector3(1f, 1.5f, 4f));
            CreateCover("Cover_3", new Vector3(0f, 0.75f, 10f), new Vector3(6f, 1.5f, 1f));
        }

        static GameObject CreateWall(string name, Vector3 pos, Vector3 scale)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.position = pos;
            wall.transform.localScale = scale;
            wall.GetComponent<Renderer>().sharedMaterial = CreateColorMaterial(new Color(0.4f, 0.4f, 0.45f));
            return wall;
        }

        static GameObject CreateCover(string name, Vector3 pos, Vector3 scale)
        {
            GameObject cover = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cover.name = name;
            cover.transform.position = pos;
            cover.transform.localScale = scale;
            cover.GetComponent<Renderer>().sharedMaterial = CreateColorMaterial(new Color(0.55f, 0.45f, 0.35f));
            return cover;
        }

        static GameObject CreatePlayer()
        {
            GameObject player = new GameObject("FPS_Player");
            player.tag = "Player";
            player.transform.position = new Vector3(0f, 1f, -12f);

            CharacterController cc = player.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.35f;
            cc.center = new Vector3(0f, 0.9f, 0f);

            FPSHealth health = player.AddComponent<FPSHealth>();
            health.isPlayer = true;
            health.maxHealth = 100;

            GameObject cameraGo = new GameObject("Camera");
            cameraGo.transform.SetParent(player.transform);
            cameraGo.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            Camera cam = cameraGo.AddComponent<Camera>();
            cam.nearClipPlane = 0.05f;
            cameraGo.AddComponent<AudioListener>();

            FPSController controller = player.AddComponent<FPSController>();
            controller.cameraRoot = cameraGo.transform;

            FPSWeapon weapon = player.AddComponent<FPSWeapon>();
            weapon.aimCamera = cam;

            GameObject gun = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gun.name = "GunModel";
            gun.transform.SetParent(cameraGo.transform);
            gun.transform.localPosition = new Vector3(0.25f, -0.2f, 0.4f);
            gun.transform.localScale = new Vector3(0.08f, 0.08f, 0.35f);
            Object.DestroyImmediate(gun.GetComponent<Collider>());
            gun.GetComponent<Renderer>().sharedMaterial = CreateColorMaterial(Color.gray);

            return player;
        }

        static void CreateTargets(int count)
        {
            GameObject parent = new GameObject("Targets");
            float radius = 14f;

            for (int i = 0; i < count; i++)
            {
                float angle = i * Mathf.PI * 2f / count;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, 1f, Mathf.Sin(angle) * radius);

                GameObject target = GameObject.CreatePrimitive(PrimitiveType.Cube);
                target.name = $"Target_{i + 1}";
                target.transform.SetParent(parent.transform);
                target.transform.position = pos;
                target.transform.localScale = new Vector3(1.2f, 1.8f, 0.4f);
                target.transform.LookAt(Vector3.zero);

                FPSHealth health = target.AddComponent<FPSHealth>();
                health.maxHealth = 50;
                target.GetComponent<Renderer>().sharedMaterial = CreateColorMaterial(
                    Color.HSVToRGB((float)i / count, 0.7f, 0.9f));
            }
        }

        static void CreateEnemies(int count)
        {
            GameObject parent = new GameObject("Enemies");
            Vector3[] positions = {
                new Vector3(-12f, 0.5f, 8f),
                new Vector3(12f, 0.5f, 4f),
                new Vector3(0f, 0.5f, 14f),
            };

            for (int i = 0; i < count && i < positions.Length; i++)
            {
                GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                enemy.name = $"Enemy_{i + 1}";
                enemy.transform.SetParent(parent.transform);
                enemy.transform.position = positions[i];

                FPSHealth health = enemy.AddComponent<FPSHealth>();
                health.maxHealth = 75;
                enemy.AddComponent<FPSEnemy>();
                enemy.GetComponent<Renderer>().sharedMaterial = CreateColorMaterial(new Color(0.8f, 0.2f, 0.2f));
            }
        }

        static void CreateGameManager(Vector3 spawnPos)
        {
            GameObject go = new GameObject("GameManager");
            FPSGameManager manager = go.AddComponent<FPSGameManager>();
            manager.playerSpawnPosition = spawnPos;
            manager.killsToWin = 11;

            FPSHealth playerHealth = Object.FindFirstObjectByType<FPSHealth>();
            FPSWeapon weapon = Object.FindFirstObjectByType<FPSWeapon>();
            if (playerHealth != null)
                manager.RegisterPlayer(playerHealth, weapon);
        }

        static Material CreateColorMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");

            Material mat = new Material(shader);
            mat.color = color;
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", color);
            return mat;
        }

        static void AddSceneToBuildSettings(string scenePath)
        {
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            foreach (var s in scenes)
            {
                if (s.path == scenePath)
                    return;
            }

            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
