using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface IResourcesInterface
{
    void Initlizate();

    void PreLoadObj(string path, int count = 1);

    void PreLoadResource<T>(string path) where T : UnityEngine.Object;

    GameObject Instantiate(string path);
    GameObject Instantiate(string path, Transform parent);
    GameObject Instantiate(string path, Transform parent, Vector3 localPosition);
    GameObject Instantiate(string path, Transform parent, Vector3 localPosition, Vector3 localScale);
    GameObject Instantiate(string path, Transform parent, Vector3 localPosition, Vector3 localScale, Quaternion quateraion);
    UniTask<GameObject> InstantiateAsync(string path);
    UniTask<GameObject> InstantiateAndLoadAsync(string path);

    void RemoveObjectLoadTCS(uint crc);

    void Release(GameObject obj, bool destroy = false);

    void Release(Texture texture);
    Sprite LoadSprite(string path);

    Texture LoadTexture(string path);

    AudioClip LoadAudio(string path);

    TextAsset LoadTextAsset(string path);

    Sprite LoadAtlasSprite(string atlasPath, string spriteName);

    UniTask<Texture> LoadTextureAsync(string path);

    UniTask<Sprite> LoadSpriteAsync(string path, Image image, bool setNativeSize = false);
    void ClearAllAsyncLoadTask();

    /// <summary>
    /// 是否深度清理
    /// </summary>
    /// <param name="absoluteCleaning"></param>
    void ClearResoucesAssets(bool absoluteCleaning);
}
