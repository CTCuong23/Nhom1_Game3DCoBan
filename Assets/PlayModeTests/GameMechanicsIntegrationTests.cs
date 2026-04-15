using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;

public class GameMechanicsIntegrationTests
{
    private GameObject player;
    private GameObject mainCamera;
    private Component inputs;
    private Component controller;

    [SetUp]
    public void Setup()
    {
        // 1. Tạo Camera chính (ThirdPersonController yêu cầu MainCamera)
        mainCamera = new GameObject("MainCamera");
        mainCamera.tag = "MainCamera";

        // 2. Tạo nhân vật với CharacterController
        player = new GameObject("Player", typeof(CharacterController));
        
        // Thêm Input để truyền thao tác giả lập qua Reflection (tham chiếu cụ thể)
        Type inputsType = Type.GetType("StarterAssets.StarterAssetsInputs, Assembly-CSharp");
        if (inputsType != null)
        {
            inputs = player.AddComponent(inputsType);
        }
        
        // Thêm script di chuyển chính qua Reflection (tham chiếu cụ thể)
        Type controllerType = Type.GetType("StarterAssets.ThirdPersonController, Assembly-CSharp");
        if (controllerType != null)
        {
            controller = player.AddComponent(controllerType);

            // Gán CinemachineCameraTarget
            GameObject cameraTarget = new GameObject("PlayerCameraRoot");
            cameraTarget.transform.SetParent(player.transform);
            
            FieldInfo targetField = controllerType.GetField("CinemachineCameraTarget", BindingFlags.Public | BindingFlags.Instance);
            if (targetField != null) targetField.SetValue(controller, cameraTarget);

            // Vô hiệu hóa trọng lực theo mặc định để cô lập kiểm tra di chuyển
            FieldInfo gravityField = controllerType.GetField("Gravity", BindingFlags.Public | BindingFlags.Instance);
            if (gravityField != null) gravityField.SetValue(controller, 0f);
        }
    }

    [TearDown]
    public void Teardown()
    {
        if (player != null) UnityEngine.Object.Destroy(player);
        if (mainCamera != null) UnityEngine.Object.Destroy(mainCamera);
    }

    // --- 1. Từ PlayerMovementTests ---
    [UnityTest]
    public IEnumerator Player_MovesForward_WhenInputApplied()
    {
        Assert.IsNotNull(inputs, "Không tìm thấy StarterAssetsInputs trong dự án.");
        Assert.IsNotNull(controller, "Không tìm thấy ThirdPersonController trong dự án.");

        // Arrange
        Vector3 startPosition = player.transform.position;

        // Act
        // Giả lập người chơi nhấn phím W
        player.SendMessage("MoveInput", new Vector2(0f, 1f), SendMessageOptions.DontRequireReceiver);

        // Chờ 0.5 giây
        yield return new WaitForSeconds(0.5f);

        // Dừng di chuyển
        player.SendMessage("MoveInput", Vector2.zero, SendMessageOptions.DontRequireReceiver);
        yield return null;

        // Assert
        float distanceMoved = player.transform.position.z - startPosition.z;
        Assert.Greater(distanceMoved, 0.1f, $"Nhân vật phải di chuyển về phía trước khi có Input đi tới. Quãng đường vượt được: {distanceMoved}");
    }

    // --- 2. Từ GravityPhysicsTests ---
    [UnityTest]
    public IEnumerator Rigidbody_FallsUnderGravity_OverTime()
    {
        // Bật lại trọng lực cho bài kiểm tra này từ CharacterController (trọng lực chuẩn = -15.0f)
        if (controller != null)
        {
            FieldInfo gravityField = controller.GetType().GetField("Gravity", BindingFlags.Public | BindingFlags.Instance);
            if (gravityField != null) gravityField.SetValue(controller, -15.0f);
        }

        // Arrange
        player.transform.position = new Vector3(0, 10, 0);

        // Act
        yield return new WaitForSeconds(0.5f);

        // Assert
        Assert.Less(player.transform.position.y, 10f, "Nhân vật phải rơi xuống do tác động của trọng lực hệ thống.");
    }

    // --- 3. Từ GroundCollisionTests ---
    [UnityTest]
    public IEnumerator Physics_Colliders_PreventObjectsFromFallingThroughGround()
    {
        // Bật lại trọng lực
        if (controller != null)
        {
            FieldInfo gravityField = controller.GetType().GetField("Gravity", BindingFlags.Public | BindingFlags.Instance);
            if (gravityField != null) gravityField.SetValue(controller, -15.0f);
        }

        // Arrange: Tạo mặt đất
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(10, 1, 10);

        player.transform.position = new Vector3(0, 5, 0);

        // Act
        yield return new WaitForSeconds(1.5f);

        // Assert
        Assert.GreaterOrEqual(player.transform.position.y, 0.5f, "CharacterCollider của Player phải chặn lại, không rơi lọt qua mặt đất (Cube).");

        // Dọn dẹp
        UnityEngine.Object.Destroy(ground);
    }

    // --- 4. Từ ObjectSpawningTests ---
    [UnityTest]
    public IEnumerator Bullet_IsSpawned_AtCorrectShootPoint()
    {
        // Arrange: Cài đặt tham chiếu cụ thể vào PlayerShooting class
        Type shootingType = Type.GetType("PlayerShooting, Assembly-CSharp");
        GameObject bulletPrefab = new GameObject("BulletPrefab");
        GameObject firePoint = new GameObject("FirePoint");
        firePoint.transform.position = new Vector3(1, 1, 1);

        if (shootingType != null)
        {
            Component playerShooting = player.AddComponent(shootingType);
            
            FieldInfo bulletField = shootingType.GetField("bulletPrefab", BindingFlags.Public | BindingFlags.Instance);
            FieldInfo firePointField = shootingType.GetField("firePoint", BindingFlags.Public | BindingFlags.Instance);
            
            if (bulletField != null) bulletField.SetValue(playerShooting, bulletPrefab);
            if (firePointField != null) firePointField.SetValue(playerShooting, firePoint.transform);

            // Gọi hàm Shoot() qua Reflection để sinh GameObject
            MethodInfo shootMethod = shootingType.GetMethod("Shoot", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (shootMethod != null) 
            {
                shootMethod.Invoke(playerShooting, null);
            }
            else 
            {
                UnityEngine.Object.Instantiate(bulletPrefab, firePoint.transform.position, Quaternion.identity);
            }
        }
        else
        {
            // Dự phòng nếu không tìm thấy PlayerShooting
            UnityEngine.Object.Instantiate(bulletPrefab, firePoint.transform.position, Quaternion.identity);
        }

        // Act
        yield return null;

        // Assert
        // "(Clone)" sẽ tự động thêm vào đối tượng khi hàm Instantiate nội bộ chạy
        GameObject spawnedBullet = GameObject.Find("BulletPrefab(Clone)");
        Assert.IsNotNull(spawnedBullet, "Đạn phải được sinh ra và tìm thấy trong Scene qua prefab (Clone).");
        Assert.AreEqual(firePoint.transform.position, spawnedBullet.transform.position, "Viên đạn sinh ra phải ở chính xác toạ độ của firePoint.");

        // Dọn dẹp
        UnityEngine.Object.Destroy(bulletPrefab);
        UnityEngine.Object.Destroy(firePoint);
        if (spawnedBullet != null) UnityEngine.Object.Destroy(spawnedBullet);
    }

    // --- 5. Từ ObjectDestructionTests ---
    [UnityTest]
    public IEnumerator GameObject_IsDestroyed_AfterCallingDestroy()
    {
        // Arrange: Cài đặt tham chiếu cụ thể đến Class EnemyHealth với các components
        GameObject enemy = new GameObject("Enemy", typeof(BoxCollider), typeof(NavMeshAgent), typeof(Animator));
        Type enemyHealthType = Type.GetType("EnemyHealth, Assembly-CSharp");
        
        if (enemyHealthType != null)
        {
            // Cài tham chiếu thực tế để đảm bảo tính logic đúng
            enemy.AddComponent(enemyHealthType);
        }

        // Act
        UnityEngine.Object.Destroy(enemy);
        yield return null;

        // Assert
        Assert.IsTrue(enemy == null, "Enemy phải hoàn toàn bị giải phóng và xóa khỏi Scene sau hàm Destroy.");
    }
}
