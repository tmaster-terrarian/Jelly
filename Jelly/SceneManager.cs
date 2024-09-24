using System;
using System.Collections.Generic;

namespace Jelly;

public static class SceneManager
{
    public delegate void SceneChangedEvent(Scene oldScene, Scene newScene);

    internal static Scene scene;
    internal static Scene nextScene;

    private static List<Entity> CurrentEntities => ActiveScene is not null ? [..ActiveScene.Entities] : [];

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

            scene = nextScene;

            GC.Collect();
            GC.WaitForPendingFinalizers();

            ActiveSceneChanged?.Invoke(lastScene, nextScene);

            JellyBackend.Logger.LogInfo($"Loaded scene {newScene.Name}");

            scene?.Begin();

            return true;
        }
        return false;
    }
}
