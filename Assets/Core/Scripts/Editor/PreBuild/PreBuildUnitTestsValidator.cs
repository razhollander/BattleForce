using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace CoreDomain.Scripts.Editor.PreBuild
{
    public class PreBuildUnitTestsValidator : IPreprocessBuildWithReport, ICallbacks
    {
        private bool _testRunComplete;
        private bool _hasFailedTests;

        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            _testRunComplete = false;
            _hasFailedTests = false;

            var testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
            var filter = new Filter
            {
                testMode = TestMode.EditMode
            };

            testRunnerApi.RegisterCallbacks(this);
            testRunnerApi.Execute(new ExecutionSettings
            {
                filters = new[] { filter },
                runSynchronously = true
            });

            // Block the main thread until tests are done
            while (!_testRunComplete)
            {
                System.Threading.Thread.Sleep(100);
            }

            if (_hasFailedTests)
            {
                throw new BuildFailedException("Build failed: Unit tests did not pass.");
            }
        }


        public void RunStarted(ITestAdaptor testsToRun) { }

        public void RunFinished(ITestResultAdaptor result)
        {
            _testRunComplete = true;
            _hasFailedTests = !result.PassCount.Equals(result.Test.TestCaseCount);
        }

        public void TestStarted(ITestAdaptor test) { }

        public void TestFinished(ITestResultAdaptor result) { }
    }
}