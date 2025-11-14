using CoreDomain.Scripts.Utils;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace CoreDomain.Scripts.Editor.UnitTests
{
    public class MathUtilsTests : ZenjectUnitTestFixture
    {
        [TestCase(95f, 85f, 90f, 10f, true, TestName = "CrossedTargetAngle_WithinTolerance")]
        [TestCase(99f, 89f, 90f, 10f, true, TestName = "CrossedExactlyAtUpperAndLowerBounds")]
        [TestCase(90f, 90f, 90f, 10f, false, TestName = "NoCross_SameAsTarget")]
        [TestCase(100f, 89f, 90f, 10f, false, TestName = "NoCross_PreviousAboveTolerance")]
        [TestCase(95f, 70f, 90f, 10f, false, TestName = "NoCross_CurrentBelowTolerance")]
        [TestCase(80f, 70f, 90f, 10f, false, TestName = "NoCross_PreviousTooLow")]
        [TestCase(91f, 89f, 90f, 0.99f, false, TestName = "CrossedWithinTightTolerance")]
        [TestCase(92f, 91f, 90f, 1f, false, TestName = "DidNotCross_TightTolerance")]
        public void TestDidCrossTargetAngle(float previous, float current, float target, float tolerance, bool expected)
        {
            var result = MathUtils.DidCrossTargetAngle(previous, current, target, tolerance);
            Assert.That(result, Is.EqualTo(expected), $"Failed on: previous={previous}, current={current}, target={target}, tolerance={tolerance}");
        }
        
        [TestCase(0f, 1f, 0f,   0f, 1f, 0f,   0f, 1f, 0f, TestName = "Up_to_Up_NoRotation")]
        [TestCase(0f, 1f, 0f,   1f, 0f, 0f,   1f, 0f, 0f, TestName = "Up_to_Right_90DegCW")]
        [TestCase(0f, 1f, 0f,  -1f, 0f, 0f,  -1f, 0f, 0f, TestName = "Up_to_Left_90DegCCW")]
        [TestCase(0f, 1f, 0f,   0f, 0f, 1f,   0f, 0f, 1f, TestName = "Up_to_Forward_RotatedOverZ")]
        [TestCase(0f, 1f, 0f,   0.707f, 0.707f, 0f,   0.707f, 0.707f, 0f, TestName = "Up_to_45DegDiagonal")]

        public void RotatedVector_ShouldMatchExpectedWithinTolerance(
            float vectorX, float vectorY, float vectorZ,
            float normalX, float normalY, float normalZ,
            float expectedX, float expectedY, float expectedZ)
        {
            var vector = new Vector3(vectorX, vectorY, vectorZ);
            var normal = new Vector3(normalX, normalY, normalZ).normalized;
            var expected = new Vector3(expectedX, expectedY, expectedZ).normalized;
            var actual = MathUtils.RotateVectorRelativeToSurface(vector, normal).normalized;
            var angle = Vector3.Angle(expected, actual);
            Assert.That(angle, Is.LessThanOrEqualTo(0.001f), $"Rotation incorrect. Expected {expected}, got {actual}, angle diff = {angle}");
        }
    }
}