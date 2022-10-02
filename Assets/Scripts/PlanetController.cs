using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static Pincushion.LD51.PlanetController;

namespace Pincushion.LD51
{
    public class PlanetController : MonoBehaviour
    {
        public SceneController scene;
        public GameObject planetModel;

        private Dictionary<int, BaseController> activeBases = new Dictionary<int, BaseController>();
        private Dictionary<int, BaseController> destroyedBases = new Dictionary<int, BaseController>();
        private Dictionary<int, BaseController> allBases = new Dictionary<int, BaseController>();
        private List<EnemyFighterController> fighters = new List<EnemyFighterController>();

        public float LastUpgradeTime { get; set; }
        public int NextUpgradeBases { get; set; }
        public int NextUpgradeFighters { get { return ActiveBaseCount + NextUpgradeBases; }}


        public int ActiveFighterCount { get { return fighters.Count; } }

        private int destroyedFighterCount = 0;
        public int DestroyedFighterCount { get { return destroyedFighterCount; } }

        public int ActiveBaseCount { get { return activeBases.Count; } }
        private int destroyedBaseCount = 0;
        public int DestroyedBaseCount { get { return destroyedBaseCount; } }
        public int InactiveBaseCount { get { return allBases.Count - activeBases.Count - destroyedBaseCount; } }

        private PlanetLocation[] possibleBaseLocations;
        public PlanetLocation[] PossibleBaseLocations { get { return possibleBaseLocations; } }

        private float diameter = -1f;
        public float Diameter {
            get
            {
                return diameter > 0? diameter : GetDiameter();
            }
        }

        private void Awake()
        {
            possibleBaseLocations = GetPossibleBaseLocations();
            NextUpgradeBases = 2; // Start with 2 bases
        }

        private void Start()
        {
            InstantiateAllBasesAsUnoperational();
            UpgradeEnemy();
            NextUpgradeBases = 1; // Add 1 base every 10s

            //CheckWinCondition();
        }

        public void UpgradeEnemy()
        {
            // Add Bases
            int maxBases = possibleBaseLocations.Length - ActiveBaseCount;
            int baseCount = NextUpgradeBases > maxBases ? maxBases : NextUpgradeBases;
            int basesAdded = 0;

            for (int i = 0; i < possibleBaseLocations.Length && baseCount > basesAdded; i++)
            {
                if (activeBases.ContainsKey(i) || destroyedBases.ContainsKey(i))
                {
                    continue;
                }

                basesAdded++;
                AddBase(i);
            }

            // Spawn Fighters
            foreach (KeyValuePair<int, BaseController> kv in activeBases)
            {
                BaseController baseController = kv.Value;
                baseController.SpawnFigher();
            }

            LastUpgradeTime = scene.Time;
        }

        public void RegisterFighter(EnemyFighterController fighter)
        {
            fighters.Add(fighter);
        }

        public void DeregisterFigher(EnemyFighterController fighter)
        {
            if (fighters.Contains(fighter))
            {
                fighters.Remove(fighter);
                destroyedFighterCount++;
            }
        }

        private void InstantiateAllBasesAsUnoperational()
        {
            GameObject basePrefab = Resources.Load<GameObject>("Base");

            for (int i = 0; i < possibleBaseLocations.Length; i++)
            {
                GameObject baseGo = Instantiate(basePrefab);

                baseGo.transform.position = possibleBaseLocations[i].position;
                //baseGo.transform.up = possibleBaseLocations[i].normal;
                baseGo.transform.up = possibleBaseLocations[i].position.normalized;// possibleBaseLocations[i].normal;

                BaseController baseController = baseGo.GetComponent<BaseController>();
                baseController.planet = this;
                baseController.baseId = i;

                allBases.Add(i, baseController);
            }
        }

        private void AddBase(int i)
        {
            if (allBases.ContainsKey(i))
            {
                BaseController baseController = allBases[i];

                baseController.MakeOperational();
                activeBases.Add(i, baseController);
            }
            else
            {
                Debug.LogError("Requested base wasn't available");
            }
        }

        public void PlayerEnteredGateCollider()
        {
            if (activeBases.Count == 0)
            {
                GetComponent<SphereCollider>().enabled = false;
            }
        }

        public void PlayerExitedGateCollider()
        {
            if (activeBases.Count == 0)
            {
                //GetComponent<SphereCollider>().enabled = true;
            }
        }

        private void CheckWinCondition()
        {
            if (activeBases.Count == 0)
            {
                StartCoroutine(OpenDoorCoroutine());
                // win condition is now on the player/core
            }
        }

        public void BaseDestroyed(BaseController baseController)
        {
            if (activeBases.ContainsKey(baseController.baseId))
            {
                destroyedBaseCount++;
                activeBases.Remove(baseController.baseId);
                destroyedBases.Add(baseController.baseId, baseController);
                CheckWinCondition();
            }
            else if (allBases.ContainsKey(baseController.baseId) && !destroyedBases.ContainsKey(baseController.baseId))
            {
                // it was inactive
                destroyedBaseCount++;
                destroyedBases.Add(baseController.baseId, baseController);
            }
            else
            {
                Debug.LogError("Destroyed base doesn't exist");
            }
        }

        private PlanetLocation[] GetPossibleBaseLocations()
        {
            List<PlanetLocation> planetLocations = new List<PlanetLocation>();

            int childCount = planetModel.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform childTransform = planetModel.transform.GetChild(i);

                if (childTransform.name.StartsWith("Base"))
                {
                    PlanetLocation location = new PlanetLocation();
                    location.position = childTransform.position;

                    //MeshFilter meshFilter = childTransform.gameObject.GetComponent<MeshFilter>();
                    //location.normal = meshFilter.mesh.normals[0];

                    planetLocations.Add(location);
                    childTransform.gameObject.SetActive(false);
                }
            }

            return planetLocations.ToArray();
        }

        public float GetDiameter()
        {
            int childCount = planetModel.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform childTransform = planetModel.transform.GetChild(i);

                if (childTransform.name == "Cube")
                {
                    MeshFilter meshFilter = childTransform.gameObject.GetComponent<MeshFilter>();
                    Bounds bound = meshFilter.sharedMesh.bounds;
                    diameter = bound.size.y / 2f;
                }
            }
            return diameter;
        }

        IEnumerator OpenDoorCoroutine()
        {
            yield return null;

            scene.Sound.PlaySound("GateOpen");

            Transform doorTransform = default(Transform);
            int childCount = planetModel.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform childTransform = planetModel.transform.GetChild(i);

                if (childTransform.name == "Door")
                {
                    doorTransform = childTransform;
                }
            }

            float startTime = scene.Time;
            float lastDeltaTime = scene.Time;
            float deltaTime = 0;

            while ((scene.Time - startTime) < 5f)
            {
                deltaTime = scene.Time - lastDeltaTime;
                lastDeltaTime = scene.Time;
                doorTransform.RotateAround(transform.position, Vector2.right, deltaTime*10f); // moving 1degree per sec

                yield return new WaitForSeconds(0.1f);
            }

            scene.Sound.StopSound("GateOpen");
        }

        public class PlanetLocation
        {
            public Vector3 position;
            public Vector3 normal;
        }
    }
}