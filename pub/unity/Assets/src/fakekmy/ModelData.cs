﻿using SharpKmyMath;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SharpKmyGfx
{
    public class ModelData
    {
        internal string path;
        internal int refcount;
        internal GameObject obj;
        internal static List<ModelData> models = new List<ModelData>();

        // ModelInstance.cs StaticModelBatcher.csで使用
        internal const float SCALE_FOR_UNITY = 100.0f;
        internal static readonly string MODELNAME_PREFIX_TEMPLATE = "(Template)";
        internal static readonly string MODELNAME_PREFIX_CLONED = "(Cloned)";

        internal GameObject instantiate(Yukar.Common.UnityUtil.ParentType parent)
        {
            var instance = UnityEngine.Object.Instantiate(obj);
            Yukar.Common.UnityUtil.setParent(parent, ref instance);
            instance.name = instance.name.Replace("(Clone)", "").Replace(MODELNAME_PREFIX_TEMPLATE, MODELNAME_PREFIX_CLONED);
            return instance;
        }

        internal static ModelData load(string path)
        {
            var ret = new ModelData();
            ret.path = path;

            // 応急処置
            if (path.EndsWith(".meta"))
                path = path.Substring(0, path.Length - 5);

            var resPath = Yukar.Common.UnityUtil.pathConvertToUnityResource(path);
            var prefab = Resources.Load(resPath) as GameObject;
            //Debug.Log(path);

            if (prefab == null)
            {
                Debug.Log("Model data " + resPath + " is not found.");
                ret.obj = Yukar.Common.UnityUtil.createObject(Yukar.Common.UnityUtil.ParentType.TEMPLATE, MODELNAME_PREFIX_TEMPLATE + resPath);
                return ret;
            }

            ret.obj = UnityEngine.Object.Instantiate(prefab);
            Yukar.Common.UnityUtil.setParent(Yukar.Common.UnityUtil.ParentType.TEMPLATE, ref ret.obj);
            ret.obj.name = MODELNAME_PREFIX_TEMPLATE + ret.obj.name.Replace("(Clone)", "");
            
            models.Add(ret);
            
            // body が body 1 になっている TypeA モデルがあるので補正する
            var wrongNameBodyNode = Yukar.Common.UnityUtil.findChild(ret.obj, "body 1");
            if (wrongNameBodyNode != null)
                wrongNameBodyNode.name = "body";

            ret.obj.SetActive(false);

            return ret;
        }
        internal void Release()
        {
            models.RemoveAll(x => x.path == path);
            if (UnityEntry.IsImportMapScene())
                UnityEngine.Object.DestroyImmediate(obj);
            else
                UnityEngine.Object.Destroy(obj);
        }

        internal int getMaterialIndex(string mtlname, int startIndex = 0)
        {
            int idx = 0;

            if (getMaterialIndexStrict(mtlname, 0, startIndex, out idx))
                return idx;

            return 0;
        }

        private bool getMaterialIndexStrict(string mtlname, int strictLevel, int startIndex, out int count)
        {
            count = startIndex;

            // _mat は外す
            if (mtlname.Length >= 4)
                mtlname = mtlname.Substring(0, mtlname.Length - 4);

            // 名前が一致していたらその番号を返す
            var children = Yukar.Common.UnityUtil.getChildren(obj);
            while (children.Count > count)
            {
                var trns = children[count];
                var mesh = trns.GetComponent<MeshRenderer>();
                if (UnityEntry.IsImportMapScene())
                {
                    if (mesh != null && mesh.sharedMaterial.name == mtlname)
                        return true;
                }
                else
                {
                    if (mesh != null && mesh.material.name == mtlname)
                        return true;
                }
                count++;
            }

            count = startIndex;
            if (strictLevel > 0)
                return false;

            while (children.Count > count)
            {
                var trns = children[count];
                var mesh = trns.GetComponent<MeshRenderer>();
                if (UnityEntry.IsImportMapScene())
                {
                    if (mesh != null && mesh.sharedMaterial.name.Contains(mtlname))
                        return true;
                }
                else
                {
                    if (mesh != null && mesh.material.name.Contains(mtlname))
                        return true;
                }
                count++;
            }

            return false;
        }

        internal int getMeshClusterMaterialIndex(int didx)
        {
            return 0;
        }

        internal void setBlendType(string mtlname, int blendtype)
        {
            // インポートの時もうやってるので、ここでは何もしない
            /*
            var meshes = getMesh(mtlname);
            foreach (var mesh in meshes)
            {
                if (mesh == null)
                    return;
                var shader = mesh.material.shader;
                switch (blendtype)
                {
                    case 2: // 加算合成
                        shader = UnityEngine.Shader.Find("Mobile/Particles/Additive");
                        break;
                    case 4: // 乗算合成
                        shader = UnityEngine.Shader.Find("Mobile/Particles/Multiply");
                        break;
                }
                mesh.material.shader = shader;
            }
            */
        }

        private MeshRenderer[] getMesh(string mtlname)
        {
            var count = -1;
            var result = new List<MeshRenderer>();
            var children = Yukar.Common.UnityUtil.getChildren(obj);
            while (children.Count > count)
            {
                count++;

                int idx = 0;
                if (getMaterialIndexStrict(mtlname, 0, count, out idx) ||
                    (children.Count == 0 && count == 0))
                    result.Add(getMesh(idx));
                if (count <= idx)
                    count = idx;
            }
            return result.ToArray();
        }

        internal MeshRenderer getMesh(int idx)
        {
            var children = Yukar.Common.UnityUtil.getChildren(obj);
            if (children.Count <= idx)
            {
                if (idx == 0)
                    return obj.transform.GetComponent<MeshRenderer>();

                return null;
            }
            
            return children[idx].GetComponent<MeshRenderer>();
        }

        internal MeshRenderer getMesh(ModelInstance inst, int idx)
        {
            var children = Yukar.Common.UnityUtil.getChildren(inst.instance);
            if (children.Count <= idx)
            {
                if (idx == 0)
                    return inst.instance.transform.GetComponent<MeshRenderer>();

                return null;
            }

            return children[idx].GetComponent<MeshRenderer>();
        }

        internal SharpKmyMath.Vector3 getCenter()
        {
            // TODO
            return new SharpKmyMath.Vector3(0, 1, 0);
        }

        internal SharpKmyMath.Vector3 getSize()
        {
            return new SharpKmyMath.Vector3(1, 1, 1);
        }
        
    }
}