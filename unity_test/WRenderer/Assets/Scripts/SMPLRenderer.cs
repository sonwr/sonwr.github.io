using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SMPLRenderer : MonoBehaviour
{
    public JointRenderer jointRenderer;
    public Animator animator;

    public float angle = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (jointRenderer != null)
        {
            List<GameObject> jointList = jointRenderer.jointGameObjectsDeepRobot;
            if (jointList.Count > 0)
            {
                // Arm

                // Neck[0] - Left Shoulder[1] - Left Elbow[5]
                Transform jointTransform = animator.GetBoneTransform(HumanBodyBones.RightShoulder);
                if (jointTransform != null)
                    jointTransform.localRotation = CalcJointRotation(jointList[0], jointList[1], jointList[5]);

                // Left Shoulder[1] - Left Elbow[5] - Left Wrist[9]
                jointTransform = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
                if (jointTransform != null)
                    jointTransform.localRotation = CalcJointRotation(jointList[1], jointList[5], jointList[9]);


                // Neck[0] - Right Shoulder[2] - Right Elbow[6]
                jointTransform = animator.GetBoneTransform(HumanBodyBones.LeftShoulder);
                if (jointTransform != null)
                    jointTransform.localRotation = CalcJointRotation(jointList[0], jointList[2], jointList[6]);

                // Right Shoulder[2] - Right Elbow[6] - Right Wrist[10]
                jointTransform = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
                if (jointTransform != null)
                    jointTransform.localRotation = CalcJointRotation(jointList[2], jointList[6], jointList[10]);


                // Leg

                // Neck[0] - Left Hip[3] - Left Knee[7]
                jointTransform = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
                if (jointTransform != null)
                    jointTransform.localRotation = CalcJointRotation(jointList[0], jointList[3], jointList[7]);

                // Left Hip[3] - Left Knee[7] - Left Ankle[11]
                jointTransform = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
                if (jointTransform != null)
                    jointTransform.localRotation = CalcJointRotation(jointList[3], jointList[7], jointList[11]);


                // Neck[0] - Right Hip[4] - Right Knee[8]
                jointTransform = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
                if (jointTransform != null)
                    jointTransform.localRotation = CalcJointRotation(jointList[0], jointList[4], jointList[8]);

                // Right Hip[4] - Right Knee[8] - Right Ankle[12]
                jointTransform = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
                if (jointTransform != null)
                    jointTransform.localRotation = CalcJointRotation(jointList[4], jointList[8], jointList[12]);
            }
        }
    }

    private Quaternion CalcJointRotation(GameObject joint1, GameObject joint2, GameObject joint3)
    {
        Transform transform1 = joint1.transform;
        Transform transform2 = joint2.transform;
        Transform transform3 = joint3.transform;

        // Y, Z 축의 방향을 반대로 설정합니다.
        Vector3 vector1 = new Vector3(transform2.localPosition.x, transform2.localPosition.y, transform2.localPosition.z) -
                          new Vector3(transform1.localPosition.x, transform1.localPosition.y, transform1.localPosition.z);

        Vector3 vector2 = new Vector3(transform3.localPosition.x, transform3.localPosition.y, transform3.localPosition.z) -
                          new Vector3(transform2.localPosition.x, transform2.localPosition.y, transform2.localPosition.z);

        return Quaternion.FromToRotation(vector1, vector2);
    }
}
