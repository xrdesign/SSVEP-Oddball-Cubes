using UnityEngine;

[RequireComponent(typeof(Animator))]

public class IKControl : MonoBehaviour {

    protected Animator animator;

    public bool ikActive = false;
    public Transform leftHandObj = null;
    public Transform rightHandObj = null;
    public Transform leftFootObj = null;
    public Transform rightFootObj = null;
    public Transform headObj = null;
    public Transform spineObj = null;
    public float offset = 1.0f;
    private float origHeight;

    void Start() {
        animator = GetComponent<Animator>();
        origHeight = gameObject.transform.position.y;
    }

    private void LateUpdate() {
        if (headObj != null) {
            Transform head = animator.GetBoneTransform(HumanBodyBones.Head);
            head.rotation = headObj.rotation;
            //head.position = headObj.position;
        }

        if (spineObj != null) {
            
            Transform hips = animator.GetBoneTransform(HumanBodyBones.Hips);
            hips.rotation = spineObj.rotation;
            Vector3 oldPosition = gameObject.transform.position;
            hips.transform.position = new Vector3(spineObj.position.x, spineObj.position.y, spineObj.position.z);
            Quaternion oldRotation = gameObject.transform.rotation;
            gameObject.transform.rotation = Quaternion.Euler(oldRotation.eulerAngles.x, spineObj.eulerAngles.y, oldRotation.eulerAngles.z);

            // offset to avoid showing mouth parts in view
            //gameObject.transform.position += gameObject.transform.forward * -0.2f;
        }
        else
        {
            Vector3 oldPosition = gameObject.transform.position;
            gameObject.transform.position = new Vector3(headObj.position.x, headObj.position.y - 0.6f, headObj.position.z);
            Quaternion oldRotation = gameObject.transform.rotation;
            gameObject.transform.rotation = Quaternion.Euler(oldRotation.eulerAngles.x, headObj.rotation.eulerAngles.y, oldRotation.eulerAngles.z);
            gameObject.transform.position += gameObject.transform.forward * -0.1f;
        }
    }

    //a callback for calculating IK
    void OnAnimatorIK() {
        if (animator) {

            //if the IK is active, set the position and rotation directly to the goal. 
            if (ikActive) {
                if (leftHandObj != null) {
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                    animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandObj.position);
                    animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandObj.rotation);
                }

                // Set the right hand target position and rotation, if one has been assigned
                if (rightHandObj != null) {
                    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                    animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandObj.position);
                    animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandObj.rotation);
                }

                if (leftFootObj != null) {
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);
                    animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootObj.position);
                    animator.SetIKRotation(AvatarIKGoal.LeftFoot, leftFootObj.rotation);
                }

                // Set the right hand target position and rotation, if one has been assigned
                if (rightFootObj != null) {
                    animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1);
                    animator.SetIKPosition(AvatarIKGoal.RightFoot, rightFootObj.position);
                    animator.SetIKRotation(AvatarIKGoal.RightFoot, rightFootObj.rotation);
                }
            }

            //if the IK is not active, set the position and rotation of the hand and head back to the original position
            else {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
                animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0);
                animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0);
                animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0);
            }
        }
    }
}