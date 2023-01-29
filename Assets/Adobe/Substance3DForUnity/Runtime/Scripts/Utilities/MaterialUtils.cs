using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Adobe.Substance
{
    public static class MaterialUtils
    {
        private const int URP_WORKFLOW_METALLIC = 1;
        private const int URP_WORKFLOW_SPECULAR = 0;

        /// <summary>
        /// Default shader for the HDRP pipeline.
        /// </summary>
        private const string HDRPShaderName = "HDRP/Lit";

        /// <summary>
        /// Default shader for the URP pipeline.
        /// </summary>
        private const string URPShaderName = "Universal Render Pipeline/Lit";

        /// <summary>
        /// Default shader for the Standard unity render pipeline.
        /// </summary>
        private const string StandardShaderName = "Standard";

        /// <summary>
        /// Table for converting substance output names to textures inputs in HDRP shaders.
        /// </summary>
        private static readonly IReadOnlyDictionary<string, string> HDRPOutputTable = new Dictionary<string, string>()
        {
            { "mask", "_MaskMap" },
            { "normal", "_NormalMap" },
            { "height", "_HeightMap" },
            { "emissive", "_EmissiveColorMap" },
            { "specular", "_SpecularColorMap" },
            { "detailMask", "_DetailMask" },
            { "opacity", "_OpacityMap" },
            { "glossiness", "_GlossinessMap" },
            { "ambientocclusion", "_OcclusionMap" },
            { "metallic", "_MetallicGlossMap" },
            { "roughness", "_RoughnessMap" }
        };

        /// <summary>
        /// Table for converting substance output names to textures inputs in URP shaders.
        /// </summary>
        private static readonly IReadOnlyDictionary<string, string> URPOutputTable = new Dictionary<string, string>()
        {
            { "normal" , "_BumpMap" },
            { "height" ,"_ParallaxMap" },
            { "emissive" , "_EmissionMap" },
            { "specular" , "_SpecGlossMap" },
            { "ambientocclusion" , "_OcclusionMap" },
            { "metallic" , "_MetallicGlossMap" },
            { "mask" , "_MaskMap" },
            { "detailMask" , "_DetailMask" },
            { "opacity" ,"_OpacityMap" },
            { "glossiness" ,"_GlossinessMap" },
            { "roughness" ,"_RoughnessMap" }
        };

        /// <summary>
        /// Table for converting substance output names to textures inputs in unity Standard pipeline shaders.
        /// </summary>
        private static readonly IReadOnlyDictionary<string, string> StandardOutputTable = new Dictionary<string, string>()
        {
            { "normal" , "_BumpMap" },
            { "height" ,"_ParallaxMap" },
            { "emissive" ,"_EmissionMap" },
            { "specular" ,"_SpecGlossMap" },
            { "specularlevel" ,"_SpecularLevelMap" },
            { "opacity", "_OpacityMap" },
            { "glossiness" ,"_GlossinessMap" },
            { "ambientocclusion" ,"_OcclusionMap" },
            { "detailmask" ,"_DetailMask" },
            { "metallic" ,"_MetallicGlossMap" },
            { "roughness" ,"_RoughnessMap" }
        };

        public static void AssignOutputTexturesToMaterial(SubstanceGraphSO graph)
        {
            foreach (var output in graph.Output)
            {
                if (output.OutputTexture == null)
                    continue;

                Texture2D texture = output.OutputTexture;

                var shaderTextureName = GetUnityTextureName(graph, output);

                EnableShaderKeywords(graph.OutputMaterial, shaderTextureName);
                graph.OutputMaterial.SetTexture(shaderTextureName, texture);
            }

            var smoothnessChannel = GetSmoothnessChannelAssignment(graph);
            graph.OutputMaterial.SetInt("_SmoothnessTextureChannel", smoothnessChannel);
            graph.OutputMaterial.SetFloat("_Glossiness", 1.0f);
            graph.OutputMaterial.SetFloat("_Smoothness", 1.0f);
            graph.OutputMaterial.SetFloat("_OcclusionStrenght", 1.0f);

            var opacityOutput = graph.Output.FirstOrDefault(a => a.IsOpacity());
        }

        /// <summary>
        /// Translates the substance channel name to the texture name in the current unity shader.
        /// </summary>
        /// <param name="graph">Substance graph.</param>
        /// <param name="outputName">Substance output name.</param>
        /// <param name="material">Target unity material.</param>
        /// <returns>Name for the target texture in the Unity shader.</returns>
        private static string GetUnityTextureName(SubstanceGraphSO graph, SubstanceOutputTexture outputTexture)
        {
            if (PluginPipelines.IsHDRP())
                return GetHDRPTexturePropertyName(graph, outputTexture);

            if (PluginPipelines.IsURP())
                return GetURPTexturePropertyName(graph, outputTexture); // for now

            return GetStandardTexturePropertyName(graph, outputTexture);
        }

        private static string GetStandardTexturePropertyName(SubstanceGraphSO graph, SubstanceOutputTexture outputTexture)
        {
            string texturePropertyName = "UnknownMap"; //null;
            string shaderName = graph.OutputMaterial.shader.name;
            var channelName = outputTexture.Description.Channel;

            if (outputTexture.IsBaseColor() || outputTexture.IsDiffuse())
            {
                string mainTexture = "invalid";

                if ((shaderName == "Standard") || (shaderName == "Standard (Roughness setup)"))
                    mainTexture = "baseColor";
                else
                    mainTexture = "diffuse";

                bool bBasecolorExists = graph.Output.Find(stuff => stuff.IsBaseColor()) != null;
                bool bDiffuseExists = graph.Output.Find(stuff => stuff.IsDiffuse()) != null;

                if (outputTexture.IsBaseColor())
                {
                    if ((channelName == mainTexture) || (bDiffuseExists == false))
                        texturePropertyName = "_MainTex";
                    else
                        texturePropertyName = "_NA_" + channelName;
                }
                else if (outputTexture.IsDiffuse())
                {
                    if ((channelName == mainTexture) || (bBasecolorExists == false))
                        texturePropertyName = "_MainTex";
                    else
                        texturePropertyName = "_NA_" + channelName;
                }

                if (texturePropertyName != null)
                    return texturePropertyName;
            }

            if (shaderName == "Standard (Roughness setup)")
            {
                if (outputTexture.IsRoughness())
                    texturePropertyName = "_SpecGlossMap";
                else if (outputTexture.IsSpecular())
                    texturePropertyName = "_SpecTex";

                if (texturePropertyName != null)
                    return texturePropertyName;
            }

            if (StandardOutputTable.TryGetValue(channelName.ToLower(), out string shaderTextureName))
                return shaderTextureName;

            return "_Unknown_" + channelName;
        }

        private static string GetHDRPTexturePropertyName(SubstanceGraphSO pGraph, SubstanceOutputTexture outputTexture)
        {
            var channelName = outputTexture.Description.Channel;

            if (string.IsNullOrEmpty(channelName))
            {
                channelName = outputTexture.Description.Label;
            }

            if (outputTexture.IsBaseColor() || outputTexture.IsDiffuse())
            {
                string texturePropertyName = "UnknownMap"; //null;

                string mainTexture = "baseColor"; // assume baseColor is always the desired main texture
                bool bBasecolorExists = pGraph.Output.Find(stuff => stuff.Description.Channel == "baseColor") != null;
                bool bDiffuseExists = pGraph.Output.Find(stuff => stuff.Description.Channel == "diffuse") != null;

                if (outputTexture.IsBaseColor())
                {
                    if ((channelName == mainTexture) || (bDiffuseExists == false))
                        texturePropertyName = "_BaseColorMap";
                    else
                        texturePropertyName = "_NA_" + channelName;
                }
                else if (outputTexture.IsDiffuse())
                {
                    if ((channelName == mainTexture) || (bBasecolorExists == false))
                        texturePropertyName = "_BaseColorMap";
                    else
                        texturePropertyName = "_NA_" + channelName;
                }

                if (texturePropertyName != null)
                    return texturePropertyName;
            }

            if (HDRPOutputTable.TryGetValue(channelName.ToLower(), out string shaderTextureName))
                return shaderTextureName;

            return "_Unknown_" + channelName;
        }

        private static string GetURPTexturePropertyName(SubstanceGraphSO pGraph, SubstanceOutputTexture outputTexture)
        {
            var channelName = outputTexture.Description.Channel;
            var material = pGraph.OutputMaterial;

            if (outputTexture.IsBaseColor() || outputTexture.IsDiffuse())
            {
                string texturePropertyName = "UnknownMap"; //null;

                string mainTexture = "invalid"; // assume, for now that basecolor is the desired main texture.

                if (material.shader.name == URPShaderName)
                {
                    int workFlow = (int)material.GetFloat("_WorkflowMode");
                    if (workFlow == URP_WORKFLOW_METALLIC)
                        mainTexture = "baseColor";
                    else // workflow == URP_WORKFLOW_SPECULAR
                        mainTexture = "diffuse";
                }
                else
                    mainTexture = "baseColor";

                bool bBasecolorExists = pGraph.Output.Find(stuff => stuff.IsBaseColor()) != null;
                bool bDiffuseExists = pGraph.Output.Find(stuff => stuff.IsDiffuse()) != null;

                if (outputTexture.IsBaseColor())
                {
                    if ((channelName == mainTexture) || (bDiffuseExists == false))
                        texturePropertyName = "_BaseMap";
                    else
                        texturePropertyName = "_NA_" + channelName;
                }
                else if (outputTexture.IsDiffuse())
                {
                    if ((channelName == mainTexture) || (bBasecolorExists == false))
                        texturePropertyName = "_BaseMap";
                    else
                        texturePropertyName = "_NA_" + channelName;
                }

                if (texturePropertyName != null)
                    return texturePropertyName;
            }

            if (URPOutputTable.TryGetValue(channelName.ToLower(), out string shaderTextureName))
                return shaderTextureName;

            return "_NA_" + channelName;
        }

        private static void EnableShaderKeywords(Material material, string shaderTextureName)
        {
            if (shaderTextureName == "_BumpMap")
            {
                material.EnableKeyword("_NORMALMAP");
            }
            else if (shaderTextureName == "_EmissionMap")
            {
                // Enables emission for the material, and make the material use
                // realtime emission.
                material.EnableKeyword("_EMISSION");
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

                // Update the emission color and intensity of the material.
                material.SetColor("_EmissionColor", Color.white);

                // Inform Unity's GI system to recalculate GI based on the new emission map.
                DynamicGI.UpdateEnvironment();
            }
            else if (shaderTextureName == "_ParallaxMap")
            {
                material.EnableKeyword("_PARALLAXMAP");
            }
            else if (shaderTextureName == "_MetallicGlossMap")
            {
                if (PluginPipelines.IsURP())
                    material.EnableKeyword("_METALLICSPECGLOSSMAP");
                else
                    material.EnableKeyword("_METALLICGLOSSMAP");
            }
        }

        /// <summary>
        /// Returns 1 if smoothness is assigned to _MainTex alpha channel and 0 if it is assigned to metallic map alpha channel.
        /// </summary>
        /// <param name="graph">Target substance graph.</param>
        /// <returns>0 or 1 depending on the smoothness channel assignment. </returns>
        private static int GetSmoothnessChannelAssignment(SubstanceGraphSO graph)
        {
            var baseColorOutput = graph.Output.FirstOrDefault(a => a.IsBaseColor());

            //Check if smoothness is assigned to baseColor.
            if (baseColorOutput != null)
                if (baseColorOutput.AlphaChannel == "roughness" || baseColorOutput.AlphaChannel == "glossiness")
                    return 1;

            //Check if smoothness is assigned to diffuse.
            var diffuseOutput = graph.Output.FirstOrDefault(a => a.IsDiffuse());

            if (diffuseOutput != null)
                if (diffuseOutput.AlphaChannel == "roughness" || diffuseOutput.AlphaChannel == "glossiness")
                    return 1;

            //Assumes it is assinged to metallic map.
            return 0;
        }

        public static bool CheckIfMaterialTexture(SubstanceOutputTexture textureOutput, SubstanceGraphSO graph)
        {
            var textureName = GetUnityTextureName(graph, textureOutput);
            return graph.OutputMaterial.HasProperty(textureName);
        }

        public static bool CheckIfStandardOutput(SubstanceOutputDescription description)
        {
            if (PluginPipelines.IsHDRP())
            {
                return CheckIfHRPStandardOutput(description);
            }
            else if (PluginPipelines.IsURP())
            {
                return CheckIfURPStandardOutput(description);
            }

            //Unity Standard render pipeline.
            return CheckIfStandardPipelineOutput(description);
        }

        public static bool CheckIfBGRA(SubstanceOutputDescription description)
        {
            if (Application.platform == RuntimePlatform.WindowsEditor
                    || Application.platform == RuntimePlatform.WindowsPlayer)
            {
                return false;
            }

            switch (description.Channel)
            {
                case "baseColor":
                case "diffuse":
                case "emissive":
                case "normal":
                    return true;

                default:
                    return false;
            }
        }

        private static bool CheckIfURPStandardOutput(SubstanceOutputDescription description)
        {
            if (string.Equals(description.Channel, "baseColor", StringComparison.OrdinalIgnoreCase)
                || string.Equals(description.Channel, "diffuse", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (!URPOutputTable.TryGetValue(description.Channel, out string shaderValue))
                return false;

            var material = new Material(GetStandardShader());
            return material.HasProperty(shaderValue);
        }

        private static bool CheckIfHRPStandardOutput(SubstanceOutputDescription description)
        {
            switch (description.Channel)
            {
                case "baseColor":
                    return true;

                case "normal":
                    return true;

                case "mask":
                    return true;

                case "height":
                    return true;

                case "emissive":
                    return true;

                default:
                    return false;
            }
        }

        private static bool CheckIfStandardPipelineOutput(SubstanceOutputDescription description)
        {
            var channel = description.Channel.ToLower();

            if ("basecolor" == channel)
                return true;

            if (!StandardOutputTable.TryGetValue(channel, out string shaderValue))
                return false;

            var material = new Material(GetStandardShader());
            return material.HasProperty(shaderValue);
        }

        public static Shader GetStandardShader()
        {
            if (PluginPipelines.IsHDRP())
                return Shader.Find(HDRPShaderName);
            else if (PluginPipelines.IsURP())
                return Shader.Find(URPShaderName);

            return Shader.Find(StandardShaderName);
        }
    }
}