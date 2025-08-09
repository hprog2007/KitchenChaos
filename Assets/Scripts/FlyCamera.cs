using UnityEngine;

public class FlyCamera : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float lookSpeed = 2f;
    public KeyCode toggleKey = KeyCode.L;

    private float yaw = 0f;
    private float pitch = 0f;
    private bool flyEnabled = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
            flyEnabled = !flyEnabled;

        if (!flyEnabled) return;

        // Look
        yaw += lookSpeed * Input.GetAxis("Mouse X");
        pitch -= lookSpeed * Input.GetAxis("Mouse Y");
        pitch = Mathf.Clamp(pitch, -89f, 89f);
        transform.eulerAngles = new Vector3(pitch, yaw, 0f);

        // Move
        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        if (Input.GetKey(KeyCode.E)) move.y += 1;
        if (Input.GetKey(KeyCode.Q)) move.y -= 1;

        transform.Translate(move * moveSpeed * Time.deltaTime, Space.Self);
    }
}
