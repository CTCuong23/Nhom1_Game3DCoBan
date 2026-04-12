using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class GroundCollisionTests
{
    [UnityTest]
    public IEnumerator Physics_Colliders_PreventObjectsFromFallingThroughGround()
    {
        // Arrange
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(10, 1, 10);

        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        player.transform.position = new Vector3(0, 5, 0);
        Rigidbody rb = player.AddComponent<Rigidbody>();

        // Act
        yield return new WaitForSeconds(1.5f);

        // Assert
        Assert.GreaterOrEqual(player.transform.position.y, 0.5f, "Collider phải chặn người chơi lại, không được lọt xuống dưới mặt đất.");

        // Dọn dẹp
        Object.Destroy(ground);
        Object.Destroy(player);
    }
}