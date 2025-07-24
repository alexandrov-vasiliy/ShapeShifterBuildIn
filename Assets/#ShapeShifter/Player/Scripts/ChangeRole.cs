using Mirror;
using UnityEngine;

namespace _ShapeShifter.Player.Scripts
{
    public enum Role { Survivor, Hunter }

    public class ChangeRole : NetworkBehaviour
    {
        [SerializeField] GameObject survivorRoot;
        [SerializeField] GameObject hunterRoot;

        [SyncVar(hook = nameof(OnRoleChanged))]
        Role currentRole = Role.Survivor;

        [Server] public void SetRole(Role role) => currentRole = role;

        void OnRoleChanged(Role _, Role newRole)
        {
            survivorRoot.SetActive(newRole == Role.Survivor);
            hunterRoot  .SetActive(newRole == Role.Hunter);
        }
    }
}