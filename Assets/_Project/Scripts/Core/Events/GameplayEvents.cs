using UnityEngine;

namespace GameArchitecture.Core.Events
{
    /// <summary>
    /// Event published when an object moves in the game world.
    /// </summary>
    public readonly struct ObjectMovedEvent
    {
        public readonly GameObject GameObject;
        public readonly Vector3 NewPosition;
        public readonly Vector3 Velocity;

        public ObjectMovedEvent(GameObject gameObject, Vector3 newPosition, Vector3 velocity)
        {
            GameObject = gameObject;
            NewPosition = newPosition;
            Velocity = velocity;
        }
    }

    /// <summary>
    /// Event published when a player performs a jump action.
    /// </summary>
    public readonly struct PlayerJumpedEvent
    {
        public readonly GameObject Player;
        public readonly float JumpForce;

        public PlayerJumpedEvent(GameObject player, float jumpForce)
        {
            Player = player;
            JumpForce = jumpForce;
        }
    }

    /// <summary>
    /// Event published when an object interacts with something in the world.
    /// </summary>
    public readonly struct ObjectInteractedEvent
    {
        public readonly GameObject Interactor;
        public readonly GameObject Target;

        public ObjectInteractedEvent(GameObject interactor, GameObject target)
        {
            Interactor = interactor;
            Target = target;
        }
    }

    /// <summary>
    /// Event published when an object is spawned in the game.
    /// </summary>
    public readonly struct ObjectSpawnedEvent
    {
        public readonly GameObject GameObject;
        public readonly Vector3 SpawnPosition;

        public ObjectSpawnedEvent(GameObject gameObject, Vector3 spawnPosition)
        {
            GameObject = gameObject;
            SpawnPosition = spawnPosition;
        }
    }
}