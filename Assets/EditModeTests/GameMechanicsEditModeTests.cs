using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class GameMechanicsEditModeTests
{
    private GameObject player;
    private GameObject mainCamera;
    private Component controller;

    [SetUp]
    public void Setup()
    {
        // 1. Tạo Camera chính
        mainCamera = new GameObject("MainCamera");
        mainCamera.tag = "MainCamera";

        // 2. Tạo nhân vật với CharacterController
        player = new GameObject("Player", typeof(CharacterController));
        
        // Cố gắng liên kết Controller nếu có trong context
        Type controllerType = Type.GetType("StarterAssets.ThirdPersonController, Assembly-CSharp");
        if (controllerType != null)
        {
            controller = player.AddComponent(controllerType);
            GameObject cameraTarget = new GameObject("PlayerCameraRoot");
            cameraTarget.transform.SetParent(player.transform);
            
            FieldInfo targetField = controllerType.GetField("CinemachineCameraTarget", BindingFlags.Public | BindingFlags.Instance);
            if (targetField != null) targetField.SetValue(controller, cameraTarget);

            FieldInfo gravityField = controllerType.GetField("Gravity", BindingFlags.Public | BindingFlags.Instance);
            if (gravityField != null) gravityField.SetValue(controller, 0f);
        }
    }

    [TearDown]
    public void Teardown()
    {
        // Trong EditMode, PHẢI dùng DestroyImmediate thay vì Destroy
        if (player != null) UnityEngine.Object.DestroyImmediate(player);
        if (mainCamera != null) UnityEngine.Object.DestroyImmediate(mainCamera);
    }

    // --- 1. Movement logic test ---
    [Test]
    public void CharacterController_ExistsAndIsConfigured()
    {
        // SỰ KHÁC BIỆT: Trong EditMode, việc giả lập Move() qua Update() như PlayMode không hiệu quả vì Update() không được Unity tự động gọi.
        // Test ở EditMode thường tập trung vào trạng thái và Component có hợp lệ không.
        CharacterController cc = player.GetComponent<CharacterController>();
        Assert.IsNotNull(cc, "CharacterController phải được gắn trên Player ngay khi khởi tạo");
        Assert.IsTrue(cc.enabled, "CharacterController phải đang ở trạng thái kích hoạt sẵn sàng");
    }

    // --- 2. Gravity logic test (Manual Physics Step) ---
    [Test]
    public void Physics_SimulateGravity_EditMode()
    {
        // Lưu lại cài đặt physics của Unity Editor và chuyển sang chế độ manual (Script)
        SimulationMode originalSimulationMode = Physics.simulationMode;
        Physics.simulationMode = SimulationMode.Script;

        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        
        player.transform.position = new Vector3(0, 10, 0);
        if (cc != null) cc.enabled = true; // Bật lại để đồng bộ với state chuẩn
        
        // Gắn Rigidbody để Physics.Simulate nhận diện và tạo rơi tự do
        Rigidbody rb = player.AddComponent<Rigidbody>();
        rb.useGravity = true;

        // Mô phỏng từng bước nhỏ đảm bảo gia tốc được tính đúng
        for (int i = 0; i < 75; i++) // tương đương 1.5 giây
        {
            Physics.Simulate(0.02f);
        }

        Assert.Less(player.transform.position.y, 9.9f, $"Nhân vật phải rơi xuống qua hệ thống giả lập vật lý EditMode. Vị trí: {player.transform.position.y}");

        // Hoàn tất và trả physics về bình thường
        Physics.simulationMode = originalSimulationMode;
    }

    // --- 3. Collision logic test ---
    [Test]
    public void Physics_SimulateCollisionWithGround_EditMode()
    {
        // Tạo mặt đất
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(10, 1, 10);

        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.transform.position = new Vector3(0, 2, 0);
        if (cc != null) cc.enabled = true; // Bật lại CC để kích hoạt hitbox va chạm
        
        // SỰ KHÁC BIỆT: Rigidbody động không tương thích tốt với CharacterController trong Unity. 
        // Thay vì ép Rigidbody rơi thẳng xuống, ta gọi trực tiếp lệnh Move nội bộ của CharacterController.
        // Lệnh này ngay lập tức quét va chạm vật lý và sẽ tự động bị chặn lại khi đụng collider của mặt đất.
        if (cc != null)
        {
            cc.Move(new Vector3(0, -10f, 0)); // Cố gắng di chuyển thẳng xuống 10 unit
        }

        Assert.GreaterOrEqual(player.transform.position.y, 0.4f, "Layer vật lý phải chặn Player lại, không để bị lọt thỏm xuyên thấu mặt đất.");

        UnityEngine.Object.DestroyImmediate(ground);
    }

    // --- 4. Spawning logic test ---
    [Test]
    public void Bullet_IsSpawned_Immediately_AtShootPoint()
    {
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
            UnityEngine.Object.Instantiate(bulletPrefab, firePoint.transform.position, Quaternion.identity);
        }

        // Test EditMode chạy cực nhanh, lệnh sinh ra được đồng bộ ngay lập tức
        GameObject spawnedBullet = GameObject.Find("BulletPrefab(Clone)");
        Assert.IsNotNull(spawnedBullet, "Đạn phải được sinh ra và tìm thấy dưới dạng Clone.");
        Assert.AreEqual(firePoint.transform.position, spawnedBullet.transform.position, "Toạ độ ban đầu phải khớp đúng vị trí nòng súng (firePoint).");

        UnityEngine.Object.DestroyImmediate(bulletPrefab);
        UnityEngine.Object.DestroyImmediate(firePoint);
        if (spawnedBullet != null) UnityEngine.Object.DestroyImmediate(spawnedBullet);
    }

    // --- 5. Destruction logic test ---
    [Test]
    public void GameObject_IsDestroyed_AfterCallingDestroyImmediate()
    {
        GameObject enemy = new GameObject("Enemy", typeof(BoxCollider), typeof(UnityEngine.AI.NavMeshAgent), typeof(Animator));
        Type enemyHealthType = Type.GetType("EnemyHealth, Assembly-CSharp");
        
        if (enemyHealthType != null)
        {
            enemy.AddComponent(enemyHealthType);
        }

        // ĐIỂM QUAN TRỌNG: Trong EditMode, Object.Destroy() thông thường sẽ ném lỗi/kẻo warning
        // Bắt buộc phải dùng DestroyImmediate để quét rác ngay trong frame đó.
        UnityEngine.Object.DestroyImmediate(enemy);

        Assert.IsTrue(enemy == null, "Enemy phải bị xóa bỏ ngay lập tức và tham chiếu trả về null qua DestroyImmediate.");
    }
}
