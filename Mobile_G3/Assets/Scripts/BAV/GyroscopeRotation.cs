using UnityEngine;

public class GyroscopeRotation : MonoBehaviour
{
    private Quaternion initialRotation;

    private void Start()
    {
        // Calibrate the initial rotation
        initialRotation = transform.rotation;
    }

    private void Update()
    {
        // Check if the gyroscope is available
        if (SystemInfo.supportsGyroscope)
        {
            // Get the gyroscope rotation
            Quaternion deviceRotation = DeviceRotation();

            // Calculate the rotation difference from the initial rotation
            Quaternion offsetRotation = Quaternion.Inverse(initialRotation) * deviceRotation;

            // Calculate the rotation angle around the y-axis (phone tilted left/right)
            float angle = Mathf.Atan2(2f * (offsetRotation.y * offsetRotation.w + offsetRotation.x * offsetRotation.z),
                1f - 2f * (offsetRotation.y * offsetRotation.y + offsetRotation.x * offsetRotation.x)) * Mathf.Rad2Deg;

            // Rotate the object to the left when the phone is inclined to the left
            if (angle < -1f)
            {
                // Adjust the rotation speed based on the angle
                float rotationSpeed = Mathf.Clamp(angle * 2f, -90f, 90f);
                
                // Rotate the object over time
                transform.rotation = Quaternion.Euler(initialRotation.eulerAngles.x, rotationSpeed * Time.deltaTime, initialRotation.eulerAngles.z);
            }
        }
    }

    private Quaternion DeviceRotation()
    {
        Quaternion gyroAttitude = Input.gyro.attitude;
        Quaternion rotation = new Quaternion(gyroAttitude.x, gyroAttitude.y, -gyroAttitude.z, -gyroAttitude.w);
        return rotation;
    }
}