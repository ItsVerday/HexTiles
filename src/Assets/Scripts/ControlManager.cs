using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlManager : MonoBehaviour
{
    public Manager.Direction direction = Manager.Direction.UP_RIGHT;
    public bool shouldMove = false;
    public float timeSinceLastMove;

    public Vector2 touchStart;

    // Update is called once per frame
    void Update()
    {
        timeSinceLastMove += Time.deltaTime;
        switch (SystemInfo.deviceType)
        {
            case DeviceType.Desktop:
                keyboardControls();
                mouseControls();
                break;

            case DeviceType.Handheld:
                touchControls();
                break;
        }
    }

    public void move(Board board)
    {
        if (timeSinceLastMove < (SystemInfo.deviceType == DeviceType.Handheld ? 0.2f : 0.05f))
        {
            return;
        }

        if (shouldMove)
        {
            board.move(direction);

            shouldMove = false;
            timeSinceLastMove = 0f;
        }
    }

    public void keyboardControls()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            direction = Manager.Direction.UP_RIGHT;
            shouldMove = true;
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            direction = Manager.Direction.RIGHT;
            shouldMove = true;
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            direction = Manager.Direction.DOWN_RIGHT;
            shouldMove = true;
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            direction = Manager.Direction.DOWN_LEFT;
            shouldMove = true;
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            direction = Manager.Direction.LEFT;
            shouldMove = true;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            direction = Manager.Direction.UP_LEFT;
            shouldMove = true;
        }
    }

    public void touchControls()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                touchStart = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                Vector2 delta = touch.position - touchStart;
                touchControlsDelta(delta);
            }
        }
    }

    public void mouseControls()
    {
        if (Input.GetMouseButtonDown(0))
        {
            touchStart = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Vector2 delta = (Vector2)Input.mousePosition - touchStart;
            touchControlsDelta(delta);
        }
    }

    public void touchControlsDelta(Vector2 delta)
    {
        if (delta.magnitude < 16f)
        {
            return;
        }

        float angle = Mathf.Rad2Deg * Mathf.Atan2(delta.y, -delta.x);
        angle = Mathf.Floor((angle - 30f) / 60f);

        Manager.Direction direction = Manager.Direction.RIGHT;
        switch ((int) angle)
        {
            case 1:
                direction = Manager.Direction.UP_RIGHT;
                break;
            case 0:
                direction = Manager.Direction.UP_LEFT;
                break;
            case -1:
                direction = Manager.Direction.LEFT;
                break;
            case -2:
                direction = Manager.Direction.DOWN_LEFT;
                break;
            case -3:
                direction = Manager.Direction.DOWN_RIGHT;
                break;
        }

        this.direction = direction;
        shouldMove = true;
    }
}