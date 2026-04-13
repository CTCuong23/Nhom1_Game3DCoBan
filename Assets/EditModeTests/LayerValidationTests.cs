using NUnit.Framework;
using UnityEngine;

public class LayerValidationTests
{
    [Test]
    public void ProjectSettings_ContainsEssentialLayers()
    {
        // Arrange
        string requiredLayer = "Enemy";

        // Act
        // Hàm NameToLayer sẽ trả về -1 nếu Layer đó chưa được tạo trong project
        int layerIndex = LayerMask.NameToLayer(requiredLayer);

        // Assert
        Assert.AreNotEqual(-1, layerIndex, $"Dự án đang thiếu Layer '{requiredLayer}'. Hãy vào Edit > Project Settings > Tags and Layers để thêm vào.");
    }
}