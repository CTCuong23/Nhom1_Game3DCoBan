using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayerMovementTests
{
    [UnityTest]
    public IEnumerator Player_MovesForward_WhenVelocityApplied()
    {
        // Arrange
        GameObject player = new GameObject("Player");
        Rigidbody rb = player.AddComponent<Rigidbody>();
        rb.useGravity = false;

        // Act
        rb.linearVelocity = Vector3.forward * 5f;
        yield return new WaitForSeconds(1f);

        // Assert
        Assert.Greater(player.transform.position.z, 0f, "Nhân vật phải di chuyển về phía trước khi được cấp vận tốc (Z > 0).");

        // Dọn dẹp
        Object.Destroy(player);
    }
}