using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GamepadVibrate : MonoBehaviour
{
    private InputDevice device;
    private int playerIndex = -1;

    private void Start()
    {
        playerIndex = GetComponent<PlayerCursor>().PlayerIndex;
        /*device = GetComponent<PlayerInput>().devices[GetComponent<PlayerCursor>().PlayerIndex];
        if (device.GetType() != typeof(Gamepad))
        {
            device = null;
        }*/
    }

    public void VibrateShort()
    {
        StartCoroutine(Vibrate());
        IEnumerator Vibrate()
        {
            Gamepad.all[playerIndex].SetMotorSpeeds(0.5f, 0.5f);
            yield return new WaitForSeconds(0.25f);
            Gamepad.all[playerIndex].SetMotorSpeeds(0, 0);
        }
    }
}