using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Kesho.Core;
using Kesho.Runtime;

public sealed class SceneRouteTests
{
    [UnityTest] public IEnumerator EveryAuthoredTargetHasAWalkableRoute()
    {
        var chapter=JsonUtility.FromJson<Chapter>(Resources.Load<TextAsset>("Story/opening").text);
        foreach(string scene in WorldCatalog.Scenes)
        {
            yield return SceneManager.LoadSceneAsync(scene);
            var stage=Object.FindFirstObjectByType<Stage>(); Assert.IsNotNull(stage,scene);
            var walker=stage.Player.GetComponent<StreetWalker>();
            foreach(var node in chapter.nodes)
            {
                if(node.scene!=scene) continue;
                Assert.IsTrue(stage.Targets.ContainsKey(node.target),scene+" contains "+node.target);
                Assert.IsTrue(walker.WalkTo(stage.Targets[node.target].position),scene+" route to "+node.target);
                walker.Stop();
            }
        }
    }
}
