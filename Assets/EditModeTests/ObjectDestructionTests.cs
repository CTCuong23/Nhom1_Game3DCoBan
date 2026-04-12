using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ObjectDestructionTests
{
    [UnityTest]
    public IEnumerator GameObject_IsDestroyed_AfterCallingDestroy()
    {
        // Arrange
        GameObject enemy = new GameObject("Enemy");

        // Act
        Object.Destroy(enemy);
        yield return null;

        // Assert
        Assert.IsTrue(enemy == null, "Enemy phải hoàn toàn bị xóa khỏi Scene sau khi gọi hàm Destroy.");
    }
}