namespace Jelly.Net;

public enum SyncPacketType : byte
{
    EntityUpdate,
    EntityAdded,
    EntityRemoved,
    ComponentUpdate,
    ComponentAdded,
    ComponentRemoved,
    Projectile,
}
