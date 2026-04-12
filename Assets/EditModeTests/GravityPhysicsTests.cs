using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class GravityPhysicsTests
{
    [UnityTest]
    public IEnumerator Rigidbody_FallsUnderGravity_OverTime()
    {
        // Arrange
        GameObject player = new GameObject("TestPlayer");
        Rigidbody rb = player.AddComponent<Rigidbody>();
        player.transform.position = new Vector3(0, 10, 0);

        // Act
        yield return new WaitForSeconds(0.5f);

        // Assert
        Assert.Less(player.transform.position.y, 10f, "Vật thể gắn Rigidbody phải rơi xuống do tác động của trọng lực.");

        // Dọn dẹp
        Object.Destroy(player);
    }
}