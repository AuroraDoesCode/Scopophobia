using GameNetcodeStuff;
using HarmonyLib;
using ShyGuy.AI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace Scopophobia
{
    public class ShyGuyPaintingProp : GrabbableObject
    {
        private bool isTriggered;
        private bool hasSpawned;
        public PlayerControllerB targetPlayer;
        public HashSet<PlayerControllerB> oldTarget = new HashSet<PlayerControllerB>();//change this to a list so we can save more players than just one for each painting.
        private float spawnTimer;
        private bool updatedScannode;
        public bool isNotTargetted;
        public int randomChance;
        private float triggeredTime = 15f;
        private ScanNodeProperties scanNode;

        public AudioSource PaintingSound;

        [Header("Painting Audio")]
        public AudioClip[] PaintingCrySFX;
        public AudioClip[] fearSFX; 
        private float lastUseTime = 0f;
        private float useCooldown = 2f;

        public override int GetItemDataToSave()
        {
            return base.GetItemDataToSave();
        }
        public void Awake()
        {

        }
        public override void Start()
        {
            base.Start();
            try
            {
                scanNode = GetComponentInChildren<ScanNodeProperties>();
                if (Config.hidePaintingName) { UpdateScannode(1); }
            }
            catch { ScopophobiaPlugin.logger.LogInfo("Failed to Init Shy Guy Painting"); }
        }

        public override void EquipItem()
        {
            base.EquipItem();
            ScopophobiaPlugin.logger.LogInfo($"Shy Guy Painting Grabbed. Am I Owner?: {IsOwner}"); 
        }
        [ServerRpc(RequireOwnership = false)]
        public void TriggerLogicServerRpc(int clientId)
        {
            targetPlayer = StartOfRound.Instance.allPlayerScripts[clientId];
            isTriggered = true;
            randomChance = UnityEngine.Random.Range(0, 75);
            if (randomChance >= 35)
            {
                PlayAudioFX(fearSFX);
                StartSpawnShyGuy();
                hasSpawned = true;
            }
            else
            {
                PlayAudioFX(PaintingCrySFX);
                oldTarget.Add(playerHeldBy); 
                SetVals();
                if (IsOwner)
                {
                    HUDManager.Instance.DisplayTip("There's an odd sound", "There's an odd sound emanating from the painting, better be careful!", false, false, "LC_ShyGuyPaintingTip1");
                }
            }
        }
        public void UpdateScannode(int which = 1)
        {
            switch (which)
            {
                case 1: scanNode.headerText = Config.hidePaintingName && !string.IsNullOrWhiteSpace(Config.nameToUseForPainting) ? Config.nameToUseForPainting : "Fancy Painting"; break;
                case 2: scanNode.headerText = "Odd Painting of SCP-096"; break;
            }
        }
        public override void Update()
        {
            base.Update();

            // Return early if not held or already completed the effect
            if (!isHeld || hasSpawned || isTriggered || playerHeldBy == null) return;

            // Keep it on the owner, so it doesnt trigger for all clients
            if (!IsOwner) return;

            //stop players triggering repeatedly
            if (oldTarget.Contains(playerHeldBy)) return;

            //one off activation, just mark it as triggered
            isTriggered = true;
            targetPlayer = playerHeldBy;
            ScopophobiaPlugin.logger.LogInfo($"Shy Guy Painting triggered by {targetPlayer.playerUsername}");

            randomChance = UnityEngine.Random.Range(0, 75);
            if (randomChance >= 35)
            {
                PlayAudioFX(fearSFX);
                if (IsServer)
                {
                    //spawn from host, not client
                    StartSpawnShyGuy();
                }
                else
                {
                    // Spawn from client, not host
                    TriggerLogicServerRpc((int)targetPlayer.actualClientId);
                }
            }
            else
            {
                PlayAudioFX(PaintingCrySFX);
                oldTarget.Add(playerHeldBy);
                isTriggered = false;
                targetPlayer = null;
                if (IsOwner)
                {
                    HUDManager.Instance.DisplayTip("There's an odd sound", "There's an odd sound emanating from the painting, better be careful!", false, false, "LC_ShyGuyPaintingTip1");
                }
            }
        }
        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);

            if (!IsOwner || hasSpawned) return;
            if (Time.time - lastUseTime < useCooldown) return;
            lastUseTime = Time.time;

            ScopophobiaPlugin.logger.LogInfo("Shy Guy Painting used — forcing spawn.");

            isTriggered = true;
            hasSpawned = true;
            targetPlayer = playerHeldBy;

            PlayAudioFX(fearSFX);

            if (IsServer)
            {
                StartSpawnShyGuy();
            }
            else
            {
                TriggerLogicServerRpc((int)playerHeldBy.actualClientId);
            }
            StartCoroutine(ResetPaintingCooldown());
        }
        private IEnumerator ResetPaintingCooldown()
        {
            yield return new WaitForSeconds(30f);
            SetVals(); // Resets flags like isTriggered, hasSpawned, etc.
            ScopophobiaPlugin.logger.LogInfo("Painting has been reset and is ready to trigger again.");
        }
       
        public void PlayAudioFX(AudioClip[] clip)
        {
            if (PaintingSound == null) return;
            if (clip == null) return;
            int num = Random.Range(0, clip.Length);
            PaintingSound.clip = clip[num];
            PaintingSound.volume = 0.3f;
            PaintingSound.Play();
        }

        public void StartSpawnShyGuy()
        {
            var shyGuyEnemy = RoundManager.Instance.currentLevel.Enemies.Find(x => x.enemyType.enemyName == "Shy guy");
            Vector3 spawnPos = RoundManager.Instance.GetRandomNavMeshPositionInRadius(
                targetPlayer.transform.position, 15f, RoundManager.Instance.navHit
            );
            if (IsServer)
            {
                SpawnEnemyOnServer(spawnPos, targetPlayer);
            }
            else
            {
                SpawnEnemyServerRpc(spawnPos, targetPlayer.actualClientId);
            }
        }
        public void SetVals()
        {
            hasSpawned = false;
            isTriggered = false;
            if (targetPlayer != null && !oldTarget.Contains(targetPlayer))
                oldTarget.Remove(targetPlayer);
            targetPlayer = null;
            randomChance = 0;
            isNotTargetted = false;
        }
        [ServerRpc]
        public void SpawnEnemyServerRpc(Vector3 spawnPos, ulong targetClientId)
        {
            var target = StartOfRound.Instance.allPlayerScripts[(int)targetClientId];
            SpawnEnemyOnServer(spawnPos, target);
        }
        public void SpawnEnemyOnServer(Vector3 spawnPos, PlayerControllerB target)
        {
            var enemy = RoundManager.Instance.currentLevel.Enemies.Find(x => x.enemyType.enemyName == "Shy guy");
            if (enemy == null) return;
            var obj = Instantiate(enemy.enemyType.enemyPrefab, spawnPos, Quaternion.identity);
            var netObj = obj.GetComponent<NetworkObject>();
            netObj.Spawn(destroyWithScene: true);
            var ai = obj.GetComponentInChildren<ShyGuyAI>();
            StartCoroutine(InitializeAI(ai, target));

        }
        private IEnumerator InitializeAI(ShyGuyAI ai, PlayerControllerB target)
        {
            yield return new WaitForSeconds(0.25f); // give shyguy time to start up properly before setting the flags

            ai.AddTargetToList((int)target.actualClientId);
            ai.targetPlayer = target;
            ai.SwitchToBehaviourState(1);
            ai.DoAIInterval();
        }
    }
}
