using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Adobe.Substance
{
    public static class RenderingUtils
    {
        public static void ConfigureOutputTextures(SubstanceNativeHandler fileHandler, SubstanceGraphSO graph, bool runtimeUsage = false)
        {
            if (PluginPipelines.IsDEFAULT() || PluginPipelines.IsURP())
            {
                AssignSmoothnessToAlpha(fileHandler, graph);
            }
            if (PluginPipelines.IsHDRP())
            {
                CreateHDRPMaskMap(fileHandler, graph);
            }

            AssignOpacityToAlpha(fileHandler, graph);
            CreateOutputVirtualCopies(fileHandler, graph);

            if (!runtimeUsage)
                ChangeRBChannel(fileHandler, graph);

            //For some reason we have to call it twice. (Bug in the substance engine?)
            UpdateAlphaChannelsAssignment(fileHandler, graph);
            UpdateAlphaChannelsAssignment(fileHandler, graph);
        }

        public static void UpdateAlphaChannelsAssignment(SubstanceNativeHandler fileHandler, SubstanceGraphSO graph)
        {
            var graphID = graph.Index;

            foreach (var output in graph.Output)
            {
                var outputID = output.Index;
                var targetIndex = output.Description.Index;
                var invert = output.InvertAssignedAlpha;
                var outputCopy = output.VirtualOutputIndex;

                if (output.IsAlphaAssignable && string.IsNullOrEmpty(output.AlphaChannel))
                {
                    fileHandler.ResetAlphaChannelAssignment(graphID, outputCopy);
                    continue;
                }

                var alphaTarget = graph.Output.FirstOrDefault(a => a.Description.Label == output.AlphaChannel);

                if (alphaTarget != null)
                {
                    var alphaSourceIndex = alphaTarget.Description.Index;
                    fileHandler.AssignOutputToAlphaChannel(graphID, outputCopy, alphaSourceIndex, invert);
                }
            }
        }

        private static void AssignOpacityToAlpha(SubstanceNativeHandler fileHandler, SubstanceGraphSO graph)
        {
            var opacityOutput = graph.Output.FirstOrDefault(a => a.IsOpacity());

            if (opacityOutput == null)
                return;

            var baseColorOutput = graph.Output.FirstOrDefault(a => a.IsBaseColor());
            var diffuseOutput = graph.Output.FirstOrDefault(a => a.IsDiffuse());

            if (baseColorOutput != null)
            {
                if (string.IsNullOrEmpty(baseColorOutput.AlphaChannel))
                {
                    baseColorOutput.AlphaChannel = opacityOutput.Description.Label;
                    baseColorOutput.InvertAssignedAlpha = false;
                }
            }

            if (diffuseOutput != null)
            {
                if (string.IsNullOrEmpty(diffuseOutput.AlphaChannel))
                {
                    diffuseOutput.AlphaChannel = opacityOutput.Description.Label;
                    diffuseOutput.InvertAssignedAlpha = false;
                }
            }
        }

        private static void AssignSmoothnessToAlpha(SubstanceNativeHandler fileHandler, SubstanceGraphSO graph)
        {
            var roughtnessOutput = graph.Output.FirstOrDefault(a => a.IsRoughness());
            var metallicOutput = graph.Output.FirstOrDefault(a => a.IsMetallicness());
            var diffuseOutput = graph.Output.FirstOrDefault(a => a.IsDiffuse());
            var baseColorOutput = graph.Output.FirstOrDefault(a => a.IsBaseColor());
            var specularOutput = graph.Output.FirstOrDefault(a => a.IsSpecular());

            if (roughtnessOutput != null && (metallicOutput != null || specularOutput != null))
            {
                if (metallicOutput != null)
                {
                    if (string.IsNullOrEmpty(metallicOutput.AlphaChannel))
                    {
                        metallicOutput.AlphaChannel = roughtnessOutput.Description.Label;
                        metallicOutput.InvertAssignedAlpha = true;
                    }
                }

                if (specularOutput != null)
                {
                    if (string.IsNullOrEmpty(specularOutput.AlphaChannel))
                    {
                        specularOutput.AlphaChannel = roughtnessOutput.Description.Label;
                        specularOutput.InvertAssignedAlpha = true;
                    }
                }
            }
            else if (roughtnessOutput != null && baseColorOutput != null)
            {
                if (string.IsNullOrEmpty(baseColorOutput.AlphaChannel))
                {
                    baseColorOutput.AlphaChannel = roughtnessOutput.Description.Label;
                    baseColorOutput.InvertAssignedAlpha = true;
                }
            }
            else if (roughtnessOutput != null && diffuseOutput != null)
            {
                if (string.IsNullOrEmpty(diffuseOutput.AlphaChannel))
                {
                    diffuseOutput.AlphaChannel = roughtnessOutput.Description.Label;
                    diffuseOutput.InvertAssignedAlpha = true;
                }
            }
        }

        private static void CreateOutputVirtualCopies(SubstanceNativeHandler fileHandler, SubstanceGraphSO graph)
        {
            var graphIndex = graph.Index;

            foreach (var output in graph.Output)
            {
                if (output.IsVirtual)
                    continue;

                var outputIndex = output.Index;
                var desc = fileHandler.CreateOutputCopy(graphIndex, outputIndex);
                output.VirtualOutputIndex = desc.Index;
            }
        }

        private static void CreateHDRPMaskMap(SubstanceNativeHandler fileHandler, SubstanceGraphSO graph)
        {
            var graphIndex = graph.Index;

            uint flags = 0;

            //Red = Metallic
            var metallicnessOutput = graph.Output.FirstOrDefault(a => a.IsMetallicness());

            var outputChannel0Info = SubstanceVirtualOutputChannelInfo.Black;
            var outputChannel1Info = SubstanceVirtualOutputChannelInfo.Black;
            var outputChannel2Info = SubstanceVirtualOutputChannelInfo.Black;
            var outputChannel3Info = SubstanceVirtualOutputChannelInfo.Black;

            if (metallicnessOutput != null)
            {
                var mettalicnessOutputUID = fileHandler.GetOutputUID(graphIndex, metallicnessOutput.Index);
                outputChannel0Info = new SubstanceVirtualOutputChannelInfo(mettalicnessOutputUID);
            }

            //Green = Occlusion
            var occlusionOutput = graph.Output.FirstOrDefault(a => a.IsOcclusion());

            if (occlusionOutput != null)
            {
                var occlusionOutputUID = fileHandler.GetOutputUID(graphIndex, occlusionOutput.Index);
                outputChannel1Info = new SubstanceVirtualOutputChannelInfo(occlusionOutputUID);
            }
            else
            {
                flags += 2;
            }

            //Blue = Detail mask
            var detailOutput = graph.Output.FirstOrDefault(a => a.IsDetail());

            if (detailOutput != null)
            {
                var detailOutputUID = fileHandler.GetOutputUID(graphIndex, detailOutput.Index);
                outputChannel2Info = new SubstanceVirtualOutputChannelInfo(detailOutputUID);
            }
            else
            {
                flags += 4;
            }

            //Alpha = Smoothness (1 - roughness)
            var roughnessOutput = graph.Output.FirstOrDefault(a => a.IsRoughness());

            if (roughnessOutput != null)
            {
                var roughnessOutputUID = fileHandler.GetOutputUID(graphIndex, roughnessOutput.Index);
                outputChannel3Info = new SubstanceVirtualOutputChannelInfo(roughnessOutputUID, ShuffleIndex.Red, true);
            }

            var outputCreateInfo = new SubstanceVirtualOutputCreateInfo(TextureFormat.RGBA32,
                                                                  "mask",
                                                                  TextureFlip.Vertical,
                                                                  outputChannel0Info,
                                                                  outputChannel1Info,
                                                                  outputChannel2Info,
                                                                  outputChannel3Info);

            var hdrpMaskOutputDescription = fileHandler.CreateVirtualOutput(graphIndex, outputCreateInfo);

            var hdrpMaskOutput = graph.Output.FirstOrDefault(a => a.IsHDRPMask());

            if (hdrpMaskOutput == null)
            {
                hdrpMaskOutput = new SubstanceOutputTexture(hdrpMaskOutputDescription, graphIndex, true)
                {
                    IsVirtual = true,
                    IsAlphaAssignable = false,
                    VirtualOutputIndex = hdrpMaskOutputDescription.Index
                };

                graph.Output.Add(hdrpMaskOutput);
            }
            else
            {
                hdrpMaskOutput.Description = hdrpMaskOutputDescription;
            }

            hdrpMaskOutput.Description.Channel = "mask";
            hdrpMaskOutput.Flags = flags;
        }

        private static void ChangeRBChannel(SubstanceNativeHandler fileHandler, SubstanceGraphSO graph)
        {
            foreach (var output in graph.Output)
            {
                if (MaterialUtils.CheckIfBGRA(output.Description))
                    fileHandler.ChangeOutputRBChannels(graph.Index, output.VirtualOutputIndex);
            }
        }
    }
}