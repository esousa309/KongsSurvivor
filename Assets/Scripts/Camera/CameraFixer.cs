using UnityEngine;

public class CameraFixer : MonoBehaviour
{
    public bool useTopDownView = true;
    public float topDownHeight = 15f;
    public float perspectiveFOV = 60f;
    public Vector3 perspectiveOffset = new Vector3(0, 8, -10);
    
    private Camera cam;
    private Transform target;
    
    void Start()
    {
        cam = GetComponent<Camera>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }
        
        if (useTopDownView)
        {
            SetupTopDownView();
        }
        else
        {
            SetupPerspectiveView();
        }
    }
    
    void SetupTopDownView()
    {
        cam.orthographic = false; // Switch to perspective for better 3D
        cam.fieldOfView = 60;
        transform.rotation = Quaternion.Euler(90, 0, 0); // Look straight down
    }
    
    void SetupPerspectiveView()
    {
        cam.orthographic = false;
        cam.fieldOfView = perspectiveFOV;
        transform.rotation = Quaternion.Euler(30, 0, 0); // Angled view
    }
    
    void LateUpdate()
    {
        if (target != null)
        {
            if (useTopDownView)
            {
                Vector3 newPos = target.position;
                newPos.y = topDownHeight;
                transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime * 5f);
            }
            else
            {
                Vector3 desiredPos = target.position + perspectiveOffset;
                transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * 5f);
            }
        }
    }
}