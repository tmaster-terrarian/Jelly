namespace Jelly.Tamperment;

public static class BTS
{
    /// <summary>
    /// Only use this method if you have to. WILL cause desyncs if you aren't careful!
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="id"></param>
    public static void SetEntityID(this Entity entity, long? id)
    {
        entity.EntityID = id ?? entity.EntityID;
    }

    /// <summary>
    /// Only use this method if you have to. WILL cause desyncs if you aren't careful!
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="id"></param>
    public static void IgnoreNextSync(this Entity entity)
    {
        entity.skipSync = true;
    }
}
