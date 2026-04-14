using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ObjectSpawningTests
{
    [UnityTest]
    public IEnumerator Bullet_IsSpawned_AtCorrectShootPoint()
    {
        // Arrange
        GameObject bulletPrefab = new GameObject("BulletPrefab");
        Vector3 shootPoint = new Vector3(1, 1, 1);

        // Act
        GameObject spawnedBullet = Object.Instantiate(bulletPrefab, shootPoint, Quaternion.identity);
        yield return null;

        // Assert
        Assert.IsNotNull(spawnedBullet, "Đạn phải được sinh ra thành công trong Scene.");
        Assert.AreEqual(shootPoint, spawnedBullet.transform.position, "Viên đạn sinh ra phải nằm chính xác tại vị trí nòng súng.");

        // Dọn dẹp
        Object.Destroy(bulletPrefab);
        if (spawnedBullet != null) Object.Destroy(spawnedBullet);
    }
}