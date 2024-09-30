using System;
using System.Collections.Generic;
using System.Linq;

namespace Jelly;

public static class SceneManager
{
    private static List<Entity> PersistentEntities {
        get {
            if(ActiveScene is null) return [];

            return [
                ..from entity in ActiveScene.Entities
                where entity.Persistent
                select entity
            ];
        }
    }

    internal static Scene scene;
    internal static Scene nextScene;

    public delegate void SceneChangedEvent(Scene oldScene, Scene newScene);

    /// <summary>
    /// The currently active Scene. Note that if set, the Scene will not actually change until the end of the Update
    /// </summary>
    public static Scene ActiveScene {
        get => scene;
        set {
            if(!ReferenceEquals(scene, value))
            {
                nextScene = value;
                if(scene is null) ChangeSceneImmediately(nextScene);
            }
        }
    }

    public static event SceneChangedEvent ActiveSceneChanged;

    public static bool ChangeSceneImmediately(Scene newScene)
    {
        nextScene = newScene;
        if(scene != nextScene)
        {
            var lastScene = scene;

            scene?.End();

            var persistentEntities = PersistentEntities;

            scene = nextScene;

            GC.Collect();
            GC.WaitForPendingFinalizers();

            scene?.Entities.AddRange(persistentEntities);

            ActiveSceneChanged?.Invoke(lastScene, nextScene);

            JellyBackend.Logger.LogInfo($"Loaded scene {newScene.Name}");

            scene?.Begin();

            return true;
        }
        return false;
    }
}
