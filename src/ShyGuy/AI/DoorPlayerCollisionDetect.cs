using BepInEx.Logging;
using GameNetcodeStuff;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Scopophobia.ShyGuy.AI
{
    internal class DoorPlayerCollisionDetect : MonoBehaviour
    {

        float doorFlyingTime = 3f;

        bool hitPlayer;
        bool isActive = true;
        public Vector3 force;
        const int doorBashDamage = 30;

        public void Start()
        {
            StartCoroutine(DisableAfterDelay());
        }

        void OnTriggerEnter(Collider other)
        {
            if (isActive && !hitPlayer && other.CompareTag("Player"))
            {
                PlayerControllerB player = other.GetComponent<PlayerControllerB>();
                if (player != StartOfRound.Instance.localPlayerController) { return; }
                player.DamagePlayer(doorBashDamage, true, true, CauseOfDeath.Inertia, 0, false, force);
                StartCoroutine(AddForceToPlayer(player));
                hitPlayer = true;
            }
        }

        private IEnumerator AddForceToPlayer(PlayerControllerB player)
        {
            PlayerControllerB player2 = player;
            Rigidbody rb = player2.playerRigidbody;
            rb.isKinematic = false;
            rb.velocity = Vector3.zero;
            player2.externalForceAutoFade += force;
            yield return new WaitForSeconds(0.5f);
            yield return new WaitUntil(() => player2.thisController.isGrounded || player2.isInHangarShipRoom);
            rb.isKinematic = true;
        }

        private IEnumerator DisableAfterDelay()
        {
            yield return new WaitForSeconds(doorFlyingTime);
            isActive = false;
        }
    }
}
