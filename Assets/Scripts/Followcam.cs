using UnityEngine;

public class Followcam : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Transform target; // 캐릭터
    public Vector3 offset = new Vector3(-0.5f, 0, -10); // 카메라 거리
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (target.position.x < 0f)
        {
            transform.position = new Vector3(
            offset.x,
            offset.y,
            offset.z
            );
        }
        else
        transform.position = new Vector3(
        target.position.x + offset.x,
        offset.y,  
        offset.z   
    );
    }
}
