using UnityEngine;

namespace GameArchitecture.Core.Events
{
    /// <summary>
    /// Event published when player movement input is received.
    /// </summary>
    public readonly struct PlayerMoveInputEvent
    {
        public readonly Vector2 MoveDirection;

        public PlayerMoveInputEvent(Vector2 moveDirection)
        {
            MoveDirection = moveDirection;
        }
    }

    /// <summary>
    /// Event published when player jump input is pressed.
    /// </summary>
    public readonly struct PlayerJumpInputEvent
    {
        public readonly bool IsPressed;

        public PlayerJumpInputEvent(bool isPressed)
        {
            IsPressed = isPressed;
        }
    }

    /// <summary>
    /// Event published when player interact input is pressed.
    /// </summary>
    public readonly struct PlayerInteractInputEvent
    {
        public readonly bool IsPressed;

        public PlayerInteractInputEvent(bool isPressed)
        {
            IsPressed = isPressed;
        }
    }
}