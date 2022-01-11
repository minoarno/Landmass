using UnityEngine;
using UnityEditor;
using UnityEditor.TerrainTools;
using UnityEngine.TerrainUtils;
using UnityEditor.Experimental.Rendering;
using UnityEngine.TerrainTools;

    public class MyPaintHeightTool : TerrainPaintTool<MyPaintHeightTool>
    {
        Material m_Material = null;
        Material GetPaintMaterial()
        {
            if (m_Material == null)
                m_Material = new Material(Shader.Find("Hidden/Terrain/PaintHeight"));
            return m_Material;
        }

        public override string GetName()
        {
            return "My Paint Height Tool";
        }

        public override string GetDescription()
        {
            return "Left click to raise.\n\nHold shift and left click to lower.";
        }

        public override void OnSceneGUI(Terrain terrain, IOnSceneGUI editContext)
        {
            TerrainPaintUtilityEditor.ShowDefaultPreviewBrush(terrain, editContext.brushTexture, editContext.brushSize);
        }

        public override bool OnPaint(Terrain terrain, IOnPaint editContext)
        {
            Material mat = TerrainPaintUtility.GetBuiltinPaintMaterial();

            float rotationDegrees = 0.0f;
            BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(terrain, editContext.uv, editContext.brushSize, rotationDegrees);
            PaintContext paintContext = TerrainPaintUtility.BeginPaintHeightmap(terrain, brushXform.GetBrushXYBounds());

            // apply brush
            Vector4 brushParams = new Vector4(editContext.brushStrength * 0.01f, 0.0f, 0.0f, 0.0f);
            mat.SetTexture("_BrushTex", editContext.brushTexture);
            mat.SetVector("_BrushParams", brushParams);
            TerrainPaintUtility.SetupTerrainToolMaterialProperties(paintContext, brushXform, mat);

            //Graphics.Blit(paintContext.sourceRenderTexture, paintContext.destinationRenderTexture, mat, (int)TerrainPaintUtility.GetBlitMaterial());

            TerrainPaintUtility.EndPaintHeightmap(paintContext, "Terrain Paint - MyPaintHeightTool");
            return false;
        }
    }
