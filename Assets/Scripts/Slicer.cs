using System;
using System.Collections;
using EzySlice;
using UnityEngine;
using Random = UnityEngine.Random;

public class Slicer : MonoBehaviour
{
    public Material materialAfterSlice;
    public LayerMask sliceMask;
    public bool isTouched;
    public SliceListener sliceListener;

    private void Update()
    {
        if (!isTouched) return;
        isTouched = false;
        Collider[] objectsToBeSliced = Physics.OverlapBox(transform.position, new Vector3(1, 0.1f, 0.1f),
            transform.rotation, sliceMask);
            
        if (objectsToBeSliced.Length != 0)
        {
            foreach (Collider objectToBeSliced in objectsToBeSliced)
            {
                if (objectToBeSliced == null) continue;
                Mesh originalMesh = objectToBeSliced.GetComponent<MeshFilter>().sharedMesh;
                float originalVolume = VolumeOfMesh(originalMesh);
                var originalObjectVelocity = objectToBeSliced.GetComponent<Rigidbody>().velocity;

                SlicedHull slicedObject = SliceObject(objectToBeSliced.gameObject, materialAfterSlice);
                if (slicedObject != null)
                {
                    GameObject upperHullGameObject =
                        slicedObject.CreateUpperHull(objectToBeSliced.gameObject, materialAfterSlice);
                    GameObject lowerHullGameObject =
                        slicedObject.CreateLowerHull(objectToBeSliced.gameObject, materialAfterSlice);

                    upperHullGameObject.transform.position = objectToBeSliced.transform.position;
                    lowerHullGameObject.transform.position = objectToBeSliced.transform.position;

                    MakeItPhysical(upperHullGameObject);
                    MakeItPhysical(lowerHullGameObject);

                    Mesh upperHullMesh = upperHullGameObject.GetComponent<MeshFilter>().sharedMesh;
                    float upperHullVolume = VolumeOfMesh(upperHullMesh);

                    Mesh lowerHullMesh = lowerHullGameObject.GetComponent<MeshFilter>().sharedMesh;
                    float lowerHullVolume = VolumeOfMesh(lowerHullMesh);

                    sliceListener.points +=
                        Convert.ToInt32(CalculatePointsBasedOnCut(originalVolume, upperHullVolume,
                            lowerHullVolume));

                    upperHullGameObject.GetComponent<Rigidbody>().velocity = originalObjectVelocity;
                    lowerHullGameObject.GetComponent<Rigidbody>().velocity = originalObjectVelocity * -1;

                    Destroy(objectToBeSliced.gameObject);

                    StartCoroutine(SelfDestruct(upperHullGameObject));
                    StartCoroutine(SelfDestruct(lowerHullGameObject));
                }
            }
        }
        else
        {
            sliceListener.points += Random.Range(5, 20);
        }
    }

    private float CalculatePointsBasedOnCut(float originalVolume, float upperVolume, float lowerVolume)
    {
        if (upperVolume < lowerVolume)
        {
            return upperVolume / originalVolume * 100 * 2;
        }

        return lowerVolume / originalVolume * 100 * 2;
    }

    float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float v321 = p3.x * p2.y * p1.z;
        float v231 = p2.x * p3.y * p1.z;
        float v312 = p3.x * p1.y * p2.z;
        float v132 = p1.x * p3.y * p2.z;
        float v213 = p2.x * p1.y * p3.z;
        float v123 = p1.x * p2.y * p3.z;
        return (1.0f / 6.0f) * (-v321 + v231 + v312 - v132 - v213 + v123);
    }

    float VolumeOfMesh(Mesh mesh)
    {
        float volume = 0;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            Vector3 p1 = vertices[triangles[i + 0]];
            Vector3 p2 = vertices[triangles[i + 1]];
            Vector3 p3 = vertices[triangles[i + 2]];
            volume += SignedVolumeOfTriangle(p1, p2, p3);
        }

        return Mathf.Abs(volume);
    }


    private IEnumerator SelfDestruct(GameObject go)
    {
        yield return new WaitForSeconds(5f);
        Destroy(go);
    }

    private void MakeItPhysical(GameObject obj)
    {
        obj.AddComponent<MeshCollider>().convex = true;
        obj.AddComponent<Rigidbody>();
        obj.tag = "Fruit";
    }

    private SlicedHull SliceObject(GameObject obj, Material crossSectionMaterial = null)
    {
        return obj.Slice(transform.position, transform.up, crossSectionMaterial);
    }
}