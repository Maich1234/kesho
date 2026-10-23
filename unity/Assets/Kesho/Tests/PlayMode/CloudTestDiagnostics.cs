using NUnit.Framework.Interfaces;
using UnityEngine;
using UnityEngine.TestRunner;
using Kesho.Runtime;

[assembly: TestRunCallback(typeof(KeshoCloudTestDiagnostics))]

/// <summary>Keep real failures visible in cloud logs without changing the test result or gate.</summary>
public sealed class KeshoCloudTestDiagnostics : ITestRunCallback
{
    public void RunStarted(ITest testsToRun)
    {
        // The test runner opens an empty backup scene before the first gameplay scene.
        // Bootstrap the same production listener there; never ignore missing-listener logs.
        if(Application.isPlaying) SessionAudioListener.Ensure();
        Debug.Log("[Kesho Tests] Starting "+testsToRun.FullName);
    }
    public void TestStarted(ITest test) { }
    public void TestFinished(ITestResult result)
    {
        if(result.Test.IsSuite) return;
        Debug.Log("[Kesho Tests] "+result.ResultState.Status+" "+result.Test.FullName);
        if(result.ResultState.Status==TestStatus.Failed)
            Debug.Log("[Kesho Tests] Failure details: "+result.Message+"\n"+result.StackTrace);
    }
    public void RunFinished(ITestResult result)
    {
        Debug.Log("[Kesho Tests] Complete: passed="+result.PassCount+", failed="+result.FailCount+", skipped="+result.SkipCount+", inconclusive="+result.InconclusiveCount);
    }
}
