using ShyGuy.AI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Scopophobia.ShyGuy.AI
{
    public class DoorCollisionDetectHook : MonoBehaviour
    {
#pragma warning disable CS8618
        public ShyGuyAI shyGuyScript;
#pragma warning restore CS8618
        DoorLock? doorLock;
        public bool Triggering;
        float timeInTrigger = 0f;
        const float doorBashForce = 55f;
        const float despawnDoorAfterBashTime = 5f;

        void OnTriggerStay(Collider other)
        {
            if (shyGuyScript.currentBehaviourStateIndex != 2) return;
            if (Triggering || !other.CompareTag("InteractTrigger")) return;
            doorLock = other.gameObject.GetComponent<DoorLock>();
            if (doorLock == null || doorLock.isDoorOpened) return;
            GameObject? doorObj = doorLock.transform.parent.transform.parent.gameObject;
            GameObject? doorMesh = doorObj.transform.Find("DoorMesh")?.gameObject;
            if(doorMesh != null)
            {
                timeInTrigger += Time.fixedDeltaTime;
                if (!(timeInTrigger <= 1f))
                {
                    Triggering = true;
                    timeInTrigger = 0f;
                    other.tag = "Untagged";
                    shyGuyScript.inSpecialAnimation = true;
                }
            }
        }
        public void BashDoor()
        {
            if (doorLock == null)
            {
                shyGuyScript.inSpecialAnimation = false;
                return;
            }
            GameObject steelDoorObj = doorLock.transform.parent.transform.parent.gameObject;
            GameObject? doorMesh = steelDoorObj.transform.Find("DoorMesh")?.gameObject;
            if (doorMesh == null)
            {
                shyGuyScript.inSpecialAnimation = false;
                return;
            }
            GameObject flyingDoorPrefab = new GameObject("FlyingDoor");
            BoxCollider tempCollider = flyingDoorPrefab.AddComponent<BoxCollider>();
            tempCollider.isTrigger = true;
            tempCollider.size = new Vector3(1f, 1.5f, 3f);
            flyingDoorPrefab.AddComponent<DoorPlayerCollisionDetect>();
            AudioSource tempAS = flyingDoorPrefab.AddComponent<AudioSource>();
            tempAS.spatialBlend = 1f;
            tempAS.maxDistance = 60f;
            tempAS.rolloffMode = AudioRolloffMode.Linear;
            tempAS.volume = 1f;
            GameObject flyingDoor = Instantiate(flyingDoorPrefab, doorLock.transform.position, doorLock.transform.rotation);
            doorMesh.transform.SetParent(flyingDoor.transform);
            Destroy(flyingDoorPrefab);
            Rigidbody rb = flyingDoor.AddComponent<Rigidbody>();
            rb.mass = 1f;
            rb.useGravity = true;
            rb.isKinematic = true;
            Vector3 doorForward = flyingDoor.transform.position + flyingDoor.transform.right * 2f;
            Vector3 doorBackward = flyingDoor.transform.position - flyingDoor.transform.right * 2f;
            Vector3 direction;
            if (Vector3.Distance(doorForward, base.transform.position) < Vector3.Distance(doorBackward, base.transform.position))
            {
                direction = (doorBackward - doorForward).normalized;
                flyingDoor.transform.position = flyingDoor.transform.position - flyingDoor.transform.right;
            }
            else
            {
                direction = (doorForward - doorBackward).normalized;
                flyingDoor.transform.position = flyingDoor.transform.position + flyingDoor.transform.right;
            }
            Vector3 upDirection = base.transform.TransformDirection(Vector3.up).normalized * 0.1f;
            Vector3 playerHitDirection = (direction + upDirection).normalized;
            flyingDoor.GetComponent<DoorPlayerCollisionDetect>().force = playerHitDirection * doorBashForce;
            rb.isKinematic = false;
            rb.AddForce(direction * doorBashForce, ForceMode.Impulse);
            AudioSource doorAudio = flyingDoor.GetComponent<AudioSource>();
            Triggering = false;
            doorLock = null;
            shyGuyScript.inSpecialAnimation = false;
            Destroy(flyingDoor, despawnDoorAfterBashTime);
        }
    }

}

