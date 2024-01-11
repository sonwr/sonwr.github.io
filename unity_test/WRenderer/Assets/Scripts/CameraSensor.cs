using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class CameraSensor : MonoBehaviour
{
    public float scale = 0.001f;
    public float width = 320.0f;
    public float height = 320.0f;

    public List<GameObject> jointObjects2D;

    // 이 스크립트를 사용하여 화면에 표시할 Sprite
    //public Sprite mySprite;

    // Prefab for the sphere to represent joints
    public GameObject jointPrefab;

    // Path to the JSON file within the Assets folder
    private string filePath = Directory.GetCurrentDirectory() + "/Data/deeprobot_joint_2d.json";

    // Start is called before the first frame update
    void Start()
    {
        /*
        // ----------------------
        // 2D Image
        // ----------------------

        // 새 GameObject 생성
        GameObject myGameObject = new GameObject("MySprite");

        // SpriteRenderer 컴포넌트 추가
        SpriteRenderer spriteRenderer = myGameObject.AddComponent<SpriteRenderer>();

        // SpriteRenderer에 Sprite 할당
        spriteRenderer.sprite = mySprite;
        spriteRenderer.transform.SetParent(this.transform);
        */

        // ----------------------
        // 2D Joint
        // ----------------------

        // Read and process the JSON file
        string dataAsJson = File.ReadAllText(filePath);
        var joints = JsonConvert.DeserializeObject<float[][]>(dataAsJson);

        // Create a list to store the instantiated joint GameObjects
        jointObjects2D = new List<GameObject>();

        // Instantiate joints and store them in the list
        for (int i = 0; i < joints.Length; i++)
        {
            float x = (width / 2 - joints[i][0]) * scale;
            float y = (height / 2 - joints[i][1]) * scale;

            Vector3 jointPosition = new Vector3(x, y, 0);
            GameObject joint = Instantiate(jointPrefab, jointPosition, Quaternion.identity);
            joint.transform.SetParent(this.transform);
            joint.transform.SetPositionAndRotation(jointPosition, Quaternion.identity);
            joint.transform.localPosition = jointPosition;

            jointObjects2D.Add(joint);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
