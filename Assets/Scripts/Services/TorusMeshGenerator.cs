using Models;
using UnityEngine;
using System.Collections.Generic;

namespace Services
{
    public class TorusMeshGenerator : IMeshGenerator
    {
        public float majorRadius { get; private set; } // 3
        public float minorRadius { get; private set; } // 1

        public int thetaSteps { get; private set; } // subdivisions along major circle per ring
        public int phiSteps { get; private set; } // subdivisions along minor circle per slice
        public int rhoSteps { get; private set; } // radial subdivisions for volume wedges

        public Material baseMaterial { get; private set; }
        
        private readonly TorusModel torusModel;
        
        
        public TorusMeshGenerator(TorusModel torusModel, float majorRadius = 3, float minorRadius = 1, int thetaSteps = 4, int phiSteps = 4,
            int rhoSteps = 2, Material baseMaterial = null)
        {
            this.torusModel = torusModel;
            this.majorRadius = majorRadius;
            this.minorRadius = minorRadius;
            this.thetaSteps = thetaSteps;
            this.phiSteps = phiSteps;
            this.rhoSteps = rhoSteps;
            this.baseMaterial = baseMaterial;
        }
        
        public void Generate(Transform container, ColorModelConfig config)
        {
            var torusConfig = config as TorusColorModelConfig;
            
            if (container == null)
            {
                Debug.LogError("TorusMeshGenerator: container is null.");
                return;
            }

            if (torusConfig == null)
            {
                Debug.LogError("TorusMeshGenerator: config is null.");
                return;
            }

            // Cleanup previous children
            foreach (Transform child in container)
                Object.Destroy(child.gameObject);

            float dTheta = 2f * Mathf.PI / torusConfig.ringCount;
            float dPhi = 2f * Mathf.PI / torusConfig.slicesPerRing;

            if (baseMaterial == null)
                baseMaterial = new Material(Shader.Find("Standard"));

            for (int ring = 0; ring < torusConfig.ringCount; ring++)
            {
                var ringModel = new TorusRingModel();
                
                var ringGO = new GameObject($"Ring_{ring}");
                ringGO.transform.SetParent(container);
                ringGO.transform.localPosition = Vector3.zero;
                ringGO.transform.localRotation = Quaternion.identity;
                ringGO.isStatic = true;

                float thetaStart = ring * dTheta;
                float thetaEnd = (ring + 1) * dTheta;

                for (int slice = 0; slice < torusConfig.slicesPerRing; slice++)
                {
                    float phiStart = slice * dPhi;
                    float phiEnd = (slice + 1) * dPhi;

                    GameObject sliceGO = new GameObject($"Slice_{ring}_{slice}");
                    sliceGO.transform.SetParent(ringGO.transform);
                    sliceGO.transform.localPosition = Vector3.zero;
                    sliceGO.transform.localRotation = Quaternion.identity;
                    sliceGO.isStatic = true;

                    MeshFilter mf = sliceGO.AddComponent<MeshFilter>();
                    MeshRenderer mr = sliceGO.AddComponent<MeshRenderer>();
                    mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    mr.receiveShadows = false;
                    mf.mesh = GenerateDoubleSidedSliceMesh(thetaStart, thetaEnd, phiStart, phiEnd);

                    // Apply color from config
                    float hue = torusConfig.GetHue(ring, slice);
                    Material mat = new Material(baseMaterial);
                    var color = Color.HSVToRGB(hue / 360, torusConfig.saturation / 360, torusConfig.value / 360);
                    mat.color = color;
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", mat.color * 0.3f);
                    mr.material = mat;
                    
                    ringModel.AddSlice(new (color));
                }
                
                torusModel.Add(new TorusRingViewModel(ringModel, ringGO));
            }
        }

        // --- Mesh generation methods (unchanged from previous final version) ---
        private Mesh GenerateDoubleSidedSliceMesh(float thetaStart, float thetaEnd, float phiStart, float phiEnd)
        {
            List<Vector3> frontVerts = new List<Vector3>();
            List<Vector3> frontNormals = new List<Vector3>();
            List<int> frontTris = new List<int>();

            BuildCurvedSurface(frontVerts, frontNormals, frontTris,
                minorRadius, phiStart, phiEnd, thetaStart, thetaEnd, phiSteps, thetaSteps, 1f);
            BuildPhiCap(frontVerts, frontNormals, frontTris,
                phiStart, thetaStart, thetaEnd, rhoSteps, thetaSteps, -1f);
            BuildPhiCap(frontVerts, frontNormals, frontTris,
                phiEnd, thetaStart, thetaEnd, rhoSteps, thetaSteps, 1f);
            BuildThetaCap(frontVerts, frontNormals, frontTris,
                thetaStart, phiStart, phiEnd, rhoSteps, phiSteps, -1f);
            BuildThetaCap(frontVerts, frontNormals, frontTris,
                thetaEnd, phiStart, phiEnd, rhoSteps, phiSteps, 1f);

            int frontCount = frontVerts.Count;
            Vector3[] finalVerts = new Vector3[frontCount * 2];
            Vector3[] finalNormals = new Vector3[frontCount * 2];
            List<int> finalTris = new List<int>(frontTris);
            finalTris.AddRange(frontTris);

            for (int i = 0; i < frontCount; i++)
            {
                finalVerts[i] = frontVerts[i];
                finalNormals[i] = frontNormals[i];
                finalVerts[i + frontCount] = frontVerts[i];
                finalNormals[i + frontCount] = -frontNormals[i];
            }

            for (int i = 0; i < frontTris.Count; i += 3)
            {
                int i0 = frontTris[i] + frontCount;
                int i1 = frontTris[i + 1] + frontCount;
                int i2 = frontTris[i + 2] + frontCount;
                finalTris.Add(i0);
                finalTris.Add(i2);
                finalTris.Add(i1);
            }

            Mesh mesh = new Mesh();
            mesh.vertices = finalVerts;
            mesh.normals = finalNormals;
            mesh.triangles = finalTris.ToArray();
            mesh.RecalculateBounds();
            return mesh;
        }

        private void BuildCurvedSurface(List<Vector3> verts, List<Vector3> norms, List<int> tris,
            float rho, float phiStart, float phiEnd, float thetaStart, float thetaEnd,
            int phiSteps, int thetaSteps, float normalSign)
        {
            int startIndex = verts.Count;
            for (int it = 0; it <= thetaSteps; it++)
            {
                float t = Mathf.Lerp(thetaStart, thetaEnd, (float)it / thetaSteps);
                for (int ip = 0; ip <= phiSteps; ip++)
                {
                    float p = Mathf.Lerp(phiStart, phiEnd, (float)ip / phiSteps);
                    verts.Add(TorusPoint(rho, p, t));
                    Vector3 dP_dphi = new Vector3(-rho * Mathf.Sin(p) * Mathf.Cos(t),
                        rho * Mathf.Cos(p),
                        -rho * Mathf.Sin(p) * Mathf.Sin(t));
                    Vector3 dP_dtheta = new Vector3(-(majorRadius + rho * Mathf.Cos(p)) * Mathf.Sin(t),
                        0,
                        (majorRadius + rho * Mathf.Cos(p)) * Mathf.Cos(t));
                    norms.Add(Vector3.Cross(dP_dphi, dP_dtheta).normalized * normalSign);
                }
            }

            for (int it = 0; it < thetaSteps; it++)
            {
                for (int ip = 0; ip < phiSteps; ip++)
                {
                    int i00 = startIndex + it * (phiSteps + 1) + ip;
                    int i10 = startIndex + (it + 1) * (phiSteps + 1) + ip;
                    int i01 = i00 + 1;
                    int i11 = i10 + 1;
                    tris.Add(i00);
                    tris.Add(i01);
                    tris.Add(i11);
                    tris.Add(i00);
                    tris.Add(i11);
                    tris.Add(i10);
                }
            }
        }

        private void BuildPhiCap(List<Vector3> verts, List<Vector3> norms, List<int> tris,
            float phiConst, float thetaStart, float thetaEnd, int rhoSteps, int thetaSteps, float normalSign)
        {
            int startIndex = verts.Count;
            for (int ir = 0; ir <= rhoSteps; ir++)
            {
                float rho = Mathf.Lerp(0f, minorRadius, (float)ir / rhoSteps);
                for (int it = 0; it <= thetaSteps; it++)
                {
                    float t = Mathf.Lerp(thetaStart, thetaEnd, (float)it / thetaSteps);
                    verts.Add(TorusPoint(rho, phiConst, t));
                    Vector3 dP_dphi = new Vector3(-rho * Mathf.Sin(phiConst) * Mathf.Cos(t),
                        rho * Mathf.Cos(phiConst),
                        -rho * Mathf.Sin(phiConst) * Mathf.Sin(t));
                    if (rho == 0f)
                        norms.Add(new Vector3(Mathf.Cos(phiConst), -Mathf.Sin(phiConst), 0).normalized * normalSign);
                    else
                        norms.Add(dP_dphi.normalized * normalSign);
                }
            }

            for (int ir = 0; ir < rhoSteps; ir++)
            {
                for (int it = 0; it < thetaSteps; it++)
                {
                    int i00 = startIndex + ir * (thetaSteps + 1) + it;
                    int i10 = startIndex + (ir + 1) * (thetaSteps + 1) + it;
                    int i01 = i00 + 1;
                    int i11 = i10 + 1;
                    if (normalSign > 0)
                    {
                        tris.Add(i00);
                        tris.Add(i01);
                        tris.Add(i11);
                        tris.Add(i00);
                        tris.Add(i11);
                        tris.Add(i10);
                    }
                    else
                    {
                        tris.Add(i00);
                        tris.Add(i10);
                        tris.Add(i11);
                        tris.Add(i00);
                        tris.Add(i11);
                        tris.Add(i01);
                    }
                }
            }
        }

        private void BuildThetaCap(List<Vector3> verts, List<Vector3> norms, List<int> tris,
            float thetaConst, float phiStart, float phiEnd, int rhoSteps, int phiSteps, float normalSign)
        {
            int startIndex = verts.Count;
            for (int ir = 0; ir <= rhoSteps; ir++)
            {
                float rho = Mathf.Lerp(0f, minorRadius, (float)ir / rhoSteps);
                for (int ip = 0; ip <= phiSteps; ip++)
                {
                    float p = Mathf.Lerp(phiStart, phiEnd, (float)ip / phiSteps);
                    verts.Add(TorusPoint(rho, p, thetaConst));
                    Vector3 dP_dtheta = new Vector3(-(majorRadius + rho * Mathf.Cos(p)) * Mathf.Sin(thetaConst),
                        0,
                        (majorRadius + rho * Mathf.Cos(p)) * Mathf.Cos(thetaConst));
                    norms.Add(dP_dtheta.normalized * normalSign);
                }
            }

            for (int ir = 0; ir < rhoSteps; ir++)
            {
                for (int ip = 0; ip < phiSteps; ip++)
                {
                    int i00 = startIndex + ir * (phiSteps + 1) + ip;
                    int i10 = startIndex + (ir + 1) * (phiSteps + 1) + ip;
                    int i01 = i00 + 1;
                    int i11 = i10 + 1;
                    if (normalSign > 0)
                    {
                        tris.Add(i00);
                        tris.Add(i10);
                        tris.Add(i11);
                        tris.Add(i00);
                        tris.Add(i11);
                        tris.Add(i01);
                    }
                    else
                    {
                        tris.Add(i00);
                        tris.Add(i01);
                        tris.Add(i11);
                        tris.Add(i00);
                        tris.Add(i11);
                        tris.Add(i10);
                    }
                }
            }
        }

        private Vector3 TorusPoint(float rho, float phi, float theta)
        {
            float x = (majorRadius + rho * Mathf.Cos(phi)) * Mathf.Cos(theta);
            float y = rho * Mathf.Sin(phi);
            float z = (majorRadius + rho * Mathf.Cos(phi)) * Mathf.Sin(theta);
            return new Vector3(x, y, z);
        }
    }
}