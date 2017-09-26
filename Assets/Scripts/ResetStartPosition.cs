using UnityEngine;

public class ResetStartPosition : MonoBehaviour
{
    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.R)) return;
        var resetPosition = new Vector3(0f, transform.position.y, -4.1f);
        transform.position = resetPosition;
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    }
}