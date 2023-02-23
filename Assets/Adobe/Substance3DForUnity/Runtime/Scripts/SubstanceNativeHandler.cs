//! @file substancearchive.cs
//! @brief Substance archive interface
//! @author Galen Helfter - Adobe
//! @date 20210609
//! @copyright Adobe. All rights reserved.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

using Adobe.Substance.Input;
using Adobe.Substance.Input.Description;

namespace Adobe.Substance
{
    /// <summary>
    /// Object that represent a native substance instance currently in use by the native engine. Use this object to get or set data to the native substance object.
    /// </summary>
    public sealed class SubstanceNativeHandler
        : IDisposable
    {
        private IntPtr _handler;
        private bool _disposedValue;
        private bool _inRenderWork;

        public bool InRenderWork
        {
            get
            {
                lock (this)
                {
                    return _inRenderWork;
                }
            }
            set
            {
                lock (this)
                {
                    _inRenderWork = value;
                }
            }
        }

        internal SubstanceNativeHandler(string filePath)
        {
            _handler = NativeMethods.sbsario_sbsar_open(filePath);

            if (_handler == default)
                throw new ArgumentException();
        }

        internal SubstanceNativeHandler(byte[] fileContent)
        {
            int size = Marshal.SizeOf(fileContent[0]) * fileContent.Length;
            var nativeMemory = Marshal.AllocHGlobal(size);
            Marshal.Copy(fileContent, 0, nativeMemory, size);

            try
            {
                _handler = NativeMethods.sbsario_sbsar_load_from_memory(nativeMemory, (IntPtr)size);

                if (_handler == default)
                    throw new ArgumentException();
            }
            finally
            {
                if (nativeMemory != default)
                    Marshal.FreeHGlobal(nativeMemory);
            }
        }

        /// <summary>
        /// Get the total graph count for this substance object.
        /// </summary>
        /// <returns>Total graph count.</returns>
        public int GetGraphCount()
        {
            return (int)NativeMethods.sbsario_sbsar_get_graph_count(_handler);
        }

        public IntPtr Render(int graphID)
        {
            ErrorCode errorCode = (ErrorCode)NativeMethods.sbsario_sbsar_render(_handler, (IntPtr)graphID, out IntPtr result);

            if (errorCode != ErrorCode.SBSARIO_ERROR_OK)
                throw new SubstanceException(errorCode);

            return result;
        }

        public byte[] GetThumbnail(int graphID)
        {
            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_get_graph_thumbnail(_handler, (IntPtr)graphID, out NativeThumbnail thumbnail);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
                throw new SubstanceException(result);

            if (thumbnail.Size == IntPtr.Zero)
                return null;

            var thumbnailData = new byte[(int)thumbnail.Size];
            Marshal.Copy(thumbnail.Data, thumbnailData, 0, thumbnailData.Length);
            return thumbnailData;
        }

        #region Presets

        public string CreatePresetFromCurrentState(int graphID)
        {
            //Alocate 1Mb for the preset file text.
            NativePreset preset = new NativePreset
            {
                XMLString = Marshal.AllocHGlobal(1024 * 1024)
            };

            try
            {
                ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_make_preset_from_current_state(_handler, (IntPtr)graphID, ref preset);

                if (result != ErrorCode.SBSARIO_ERROR_OK)
                    throw new SubstanceException(result);

                var stringResult = Marshal.PtrToStringAnsi(preset.XMLString);
                return stringResult;
            }
            finally
            {
                //Free native allocated memory.
                Marshal.FreeHGlobal(preset.XMLString);
            }
        }

        public void ApplyPreset(int graphID, string presetXML)
        {
            NativePreset preset = new NativePreset();
            preset.XMLString = Marshal.StringToHGlobalAnsi(presetXML);

            try
            {
                ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_apply_preset(_handler, (IntPtr)graphID, ref preset);

                if (result != ErrorCode.SBSARIO_ERROR_OK)
                    throw new SubstanceException(result);
            }
            finally
            {
                // Always free the unmanaged string.
                Marshal.FreeHGlobal(preset.XMLString);
            }
        }

        #endregion Presets

        #region Output

        /// <summary>
        /// Get the total output count for a given graph in the substance object.
        /// </summary>
        /// <param name="graphID">Index of the target graph.</param>
        /// <returns>Total graph output count.</returns>
        public int GetGraphOutputCount(int graphID)
        {
            return (int)NativeMethods.sbsario_sbsar_get_output_count(_handler, (IntPtr)graphID);
        }

        public SubstanceOutputDescription GetOutputDescription(int graphID, int outputID)
        {
            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_get_output_desc(_handler, (IntPtr)graphID, (IntPtr)outputID, out NativeOutputDesc inputDesc);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
                throw new SubstanceException(result);

            var identifier = Marshal.PtrToStringAnsi(inputDesc.mIdentifier);
            var label = Marshal.PtrToStringAnsi(inputDesc.mIdentifier);
            var channel = Marshal.PtrToStringAnsi(inputDesc.mChannelUsage);

            return new SubstanceOutputDescription()
            {
                Identifier = identifier,
                Label = label,
                Index = (int)inputDesc.mIndex,
                Type = inputDesc.mValueType.ToUnity(),
                Channel = channel
            };
        }

        public SubstanceOutputDescription CreateVirtualOutput(int graphIndex, SubstanceVirtualOutputCreateInfo info)
        {
            var description = info.CreateOutputDesc();
            var format = info.CreateOutputFormat();

            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_create_virtual_output(_handler, (IntPtr)graphIndex, ref description, ref format);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
                throw new SubstanceException(result);

            var identifier = Marshal.PtrToStringAnsi(description.mIdentifier);
            var label = Marshal.PtrToStringAnsi(description.mIdentifier);
            var channel = Marshal.PtrToStringAnsi(description.mChannelUsage);

            return new SubstanceOutputDescription()
            {
                Identifier = identifier,
                Label = label,
                Index = (int)description.mIndex,
                Type = description.mValueType.ToUnity(),
                Channel = channel,
            };
        }

        public SubstanceOutputDescription CreateOutputCopy(int graphID, int outputID)
        {
            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_create_output_copy(_handler, (IntPtr)graphID, (IntPtr)outputID, out NativeOutputDesc inputDesc);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
                throw new SubstanceException(result);

            var identifier = Marshal.PtrToStringAnsi(inputDesc.mIdentifier);
            var label = Marshal.PtrToStringAnsi(inputDesc.mIdentifier);
            var channel = Marshal.PtrToStringAnsi(inputDesc.mChannelUsage);

            return new SubstanceOutputDescription()
            {
                Identifier = identifier,
                Label = label,
                Index = (int)inputDesc.mIndex,
                Type = inputDesc.mValueType.ToUnity(),
                Channel = channel
            };
        }

        public uint GetOutputUID(int graphID, int outputIndex)
        {
            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_get_output_uid(_handler, (IntPtr)graphID, (IntPtr)outputIndex, out IntPtr outputUID);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
                throw new SubstanceException(result);

            return (uint)outputUID;
        }

        public void AssignOutputToAlphaChannel(int graphID, int targetOutputID, int alphaChannelID, bool invert = false)
        {
            float minValue = invert ? 1f : 0f;
            float maxValue = invert ? 0f : 1f;

            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_assign_as_alpha_channel(_handler, (IntPtr)graphID, (IntPtr)targetOutputID, (IntPtr)alphaChannelID, minValue, maxValue);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
                throw new SubstanceException(result);
        }

        public void ResetAlphaChannelAssignment(int graphID, int targetOutputID)
        {
            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_get_output_desc(_handler, (IntPtr)graphID, (IntPtr)targetOutputID, out NativeOutputDesc outputDesc);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
                throw new SubstanceException(result);

            result = (ErrorCode)NativeMethods.sbsario_sbsar_get_output_format_override(_handler, (IntPtr)graphID, (IntPtr)targetOutputID, out IntPtr isFormatOverridenPtr, out NativeOutputFormat oldFormat);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
                throw new SubstanceException(result);

            bool isFormatOverriden = (int)isFormatOverridenPtr != 0;

            if (!isFormatOverriden)
                return;

            if (oldFormat.format == NativeConsts.UseDefault)
                return;

            NativeOutputFormat newFormat = oldFormat;
            newFormat.format = NativeConsts.UseDefault;

            result = (ErrorCode)NativeMethods.sbsario_sbsar_set_output_format_override(_handler, (IntPtr)graphID, (IntPtr)targetOutputID, ref newFormat);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
                throw new SubstanceException(result);
        }

        public void ChangeOutputRBChannels(int graphID, int targetOutputID)
        {
            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_get_output_format_override(_handler, (IntPtr)graphID, (IntPtr)targetOutputID, out IntPtr isFormatOverridenPtr, out NativeOutputFormat oldFormat);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
                throw new SubstanceException(result);

            bool isFormatOverriden = (int)isFormatOverridenPtr != 0;

            NativeOutputFormat newFormat = NativeOutputFormat.CreateDefault();

            if (isFormatOverriden)
                newFormat = oldFormat;

            var redChannel = newFormat.ChannelComponent0;
            var blueChannel = newFormat.ChannelComponent2;

            if (blueChannel.outputIndex == NativeConsts.UseDefault || redChannel.outputIndex == NativeConsts.UseDefault)
                return;

            if (newFormat.ChannelComponent0.ShuffleIndex == ShuffleIndex.Blue &&
            newFormat.ChannelComponent2.ShuffleIndex == ShuffleIndex.Red)
            {
                return;
            }

            newFormat.ChannelComponent0.ShuffleIndex = ShuffleIndex.Blue;
            newFormat.ChannelComponent2.ShuffleIndex = ShuffleIndex.Red;

            result = (ErrorCode)NativeMethods.sbsario_sbsar_set_output_format_override(_handler, (IntPtr)graphID, (IntPtr)targetOutputID, ref newFormat);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
                throw new SubstanceException(result);
        }

        #endregion Output

        #region Input

        /// <summary>
        /// Get the total input count for a given graph in the substance object.
        /// </summary>
        /// <param name="graphID">Index of the target graph.</param>
        /// <returns>Total graph input count.</returns>
        public int GetInputCount(int graphID)
        {
            return (int)NativeMethods.sbsario_sbsar_get_input_count(_handler, (IntPtr)graphID);
        }

        /// <summary>
        /// Get the input object for a given graph in the substance object.
        /// </summary>
        /// <param name="graphID">Index of the target graph.</param>
        /// <param name="inputID">Input index.</param>
        /// <returns>Substance input object.</returns>
        public SubstanceInputBase GetInputObject(int graphID, int inputID)
        {
            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_get_input(_handler, (IntPtr)graphID, (IntPtr)inputID, out NativeData inputDesc);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
            {
                if (result == ErrorCode.SBSARIO_ERROR_FAILURE)
                {
                    Debug.LogWarning($"Unable to load input from graphID: {graphID} and inputID:{inputID}. The input type is not supported.");
                    return SubstanceInputFactory.CreateInvalidInput(inputID, graphID);
                }

                throw new SubstanceException(result);
            }

            var input = SubstanceInputFactory.CreateInput(inputDesc, graphID);
            AssignInputDescription(graphID, inputID, input);
            return input;
        }

        /// <summary>
        /// Returns true if the target input should be visible in the editor.
        /// </summary>
        /// <param name="graphID">Target graph ID.</param>
        /// <param name="inputID">Target input ID.</param>
        /// <returns>True if the target input should be visible in the editor.</returns>
        public bool IsInputVisible(int graphID, int inputID)
        {
            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_get_input_visibility(_handler, (IntPtr)graphID, (IntPtr)inputID, out NativeInputVisibility visibility);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
                throw new SubstanceException(result);

            return (int)visibility.IsVisible == 1;
        }

        public void SetInputFloat(float value, int inputID, int graphID)
        {
            NativeData inputData = new NativeData();
            inputData.DataType = DataType.SBSARIO_DATA_INPUT;
            inputData.ValueType = ValueType.SBSARIO_VALUE_FLOAT;
            inputData.Index = (IntPtr)inputID;
            inputData.Data = new DataInternalNumeric();
            inputData.Data.mFloatData0 = value;

            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_set_input(_handler, (IntPtr)graphID, ref inputData);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
            {
                Debug.LogError($"Fail to update Substance input {inputID} for graph {graphID}");
                throw new SubstanceException(result);
            }
        }

        public void SetInputFloat2(Vector2 value, int inputID, int graphID)
        {
            NativeData inputData = new NativeData();
            inputData.DataType = DataType.SBSARIO_DATA_INPUT;
            inputData.ValueType = ValueType.SBSARIO_VALUE_FLOAT2;
            inputData.Index = (IntPtr)inputID;
            inputData.Data = new DataInternalNumeric();
            inputData.Data.mFloatData0 = value.x;
            inputData.Data.mFloatData1 = value.y;

            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_set_input(_handler, (IntPtr)graphID, ref inputData);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
            {
                Debug.LogError($"Fail to update Substance input {inputID} for graph {graphID}");
                throw new SubstanceException(result);
            }
        }

        public void SetInputFloat3(Vector3 value, int inputID, int graphID)
        {
            NativeData inputData = new NativeData();
            inputData.DataType = DataType.SBSARIO_DATA_INPUT;
            inputData.ValueType = ValueType.SBSARIO_VALUE_FLOAT3;
            inputData.Index = (IntPtr)inputID;
            inputData.Data = new DataInternalNumeric();
            inputData.Data.mFloatData0 = value.x;
            inputData.Data.mFloatData1 = value.y;
            inputData.Data.mFloatData2 = value.z;

            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_set_input(_handler, (IntPtr)graphID, ref inputData);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
            {
                Debug.LogError($"Fail to update Substance input {inputID} for graph {graphID}");
                throw new SubstanceException(result);
            }
        }

        public void SetInputFloat4(Vector4 value, int inputID, int graphID)
        {
            NativeData inputData = new NativeData();
            inputData.DataType = DataType.SBSARIO_DATA_INPUT;
            inputData.ValueType = ValueType.SBSARIO_VALUE_FLOAT4;
            inputData.Index = (IntPtr)inputID;
            inputData.Data = new DataInternalNumeric();
            inputData.Data.mFloatData0 = value.x;
            inputData.Data.mFloatData1 = value.y;
            inputData.Data.mFloatData2 = value.z;
            inputData.Data.mFloatData3 = value.w;

            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_set_input(_handler, (IntPtr)graphID, ref inputData);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
            {
                Debug.LogError($"Fail to update Substance input {inputID} for graph {graphID}");
                throw new SubstanceException(result);
            }
        }

        public void SetInputInt(int value, int inputID, int graphID)
        {
            NativeData inputData = new NativeData();
            inputData.DataType = DataType.SBSARIO_DATA_INPUT;
            inputData.ValueType = ValueType.SBSARIO_VALUE_INT;
            inputData.Index = (IntPtr)inputID;
            inputData.Data = new DataInternalNumeric();
            inputData.Data.mIntData0 = value;

            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_set_input(_handler, (IntPtr)graphID, ref inputData);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
            {
                Debug.LogError($"Fail to update Substance input {inputID} for graph {graphID}");
                throw new SubstanceException(result);
            }
        }

        public void SetInputInt2(Vector2Int value, int inputID, int graphID)
        {
            NativeData inputData = new NativeData();
            inputData.DataType = DataType.SBSARIO_DATA_INPUT;
            inputData.ValueType = ValueType.SBSARIO_VALUE_INT2;
            inputData.Index = (IntPtr)inputID;
            inputData.Data = new DataInternalNumeric();
            inputData.Data.mIntData0 = value.x;
            inputData.Data.mIntData1 = value.y;

            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_set_input(_handler, (IntPtr)graphID, ref inputData);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
            {
                Debug.LogError($"Fail to update Substance input {inputID} for graph {graphID}");
                throw new SubstanceException(result);
            }
        }

        public void SetInputInt3(Vector3Int value, int inputID, int graphID)
        {
            NativeData inputData = new NativeData();
            inputData.DataType = DataType.SBSARIO_DATA_INPUT;
            inputData.ValueType = ValueType.SBSARIO_VALUE_INT3;
            inputData.Index = (IntPtr)inputID;
            inputData.Data = new DataInternalNumeric();
            inputData.Data.mIntData0 = value.x;
            inputData.Data.mIntData1 = value.y;
            inputData.Data.mIntData2 = value.z;

            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_set_input(_handler, (IntPtr)graphID, ref inputData);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
            {
                Debug.LogError($"Fail to update Substance input {inputID} for graph {graphID}");
                throw new SubstanceException(result);
            }
        }

        public void SetInputInt4(int x, int y, int z, int w, int inputID, int graphID)
        {
            NativeData inputData = new NativeData();
            inputData.DataType = DataType.SBSARIO_DATA_INPUT;
            inputData.ValueType = ValueType.SBSARIO_VALUE_INT4;
            inputData.Index = (IntPtr)inputID;
            inputData.Data = new DataInternalNumeric();
            inputData.Data.mIntData0 = x;
            inputData.Data.mIntData1 = y;
            inputData.Data.mIntData2 = z;
            inputData.Data.mIntData2 = w;

            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_set_input(_handler, (IntPtr)graphID, ref inputData);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
            {
                Debug.LogError($"Fail to update Substance input {inputID} for graph {graphID}");
                throw new SubstanceException(result);
            }
        }

        public void SetInputString(string value, int inputID, int graphID)
        {
            NativeData inputData = new NativeData();
            inputData.DataType = DataType.SBSARIO_DATA_INPUT;
            inputData.ValueType = ValueType.SBSARIO_VALUE_STRING;
            inputData.Index = (IntPtr)inputID;
            inputData.Data = new DataInternalNumeric();
            inputData.Data.mPtr = Marshal.StringToHGlobalAnsi(value);

            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_set_input(_handler, (IntPtr)graphID, ref inputData);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
            {
                Debug.LogError($"Fail to update Substance input {inputID} for graph {graphID}");
                throw new SubstanceException(result);
            }
        }

        public void SetInputTexture2DNull(int inputID, int graphID)
        {
            var imageData = new NativeDataImage
            {
                channel_order = ChannelOrder.SBSARIO_CHANNEL_ORDER_RGBA,
                height = IntPtr.Zero,
                width = IntPtr.Zero
            };

            var numericData = new DataInternalNumeric
            {
                ImageData = imageData
            };

            NativeData inputData = new NativeData
            {
                DataType = DataType.SBSARIO_DATA_INPUT,
                ValueType = ValueType.SBSARIO_VALUE_IMAGE,
                Index = (IntPtr)inputID,
                Data = numericData
            };

            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_set_input(_handler, (IntPtr)graphID, ref inputData);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
            {
                Debug.LogError($"Fail to update Substance input {inputID} for graph {graphID}");
                throw new SubstanceException(result);
            }
        }

        public void SetInputTexture2D(Color32[] pixelData, int width, int height, int inputID, int graphID)
        {
            if (pixelData == null)
                return;

            Color32[] flippedData = TextureExtensions.FlipY(pixelData, width, height);
            byte[] textureBytes = TextureExtensions.Color32ArrayToByteArray(flippedData);
            int textureSize = Marshal.SizeOf(textureBytes[0]) * textureBytes.Length;
            IntPtr tempNativeMemory = Marshal.AllocHGlobal(textureSize);
            Marshal.Copy(textureBytes, 0, tempNativeMemory, textureBytes.Length);

            var imageData = new NativeDataImage
            {
                channel_order = ChannelOrder.SBSARIO_CHANNEL_ORDER_RGBA,
                height = (IntPtr)width,
                width = (IntPtr)height,
                mipmaps = (IntPtr)0,
                image_format = TextureFormat.RGBA32.ToSubstance(),
                data = tempNativeMemory
            };

            var numericData = new DataInternalNumeric
            {
                ImageData = imageData
            };

            NativeData inputData = new NativeData
            {
                DataType = DataType.SBSARIO_DATA_INPUT,
                ValueType = ValueType.SBSARIO_VALUE_IMAGE,
                Index = (IntPtr)inputID,
                Data = numericData
            };

            try
            {
                ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_set_input(_handler, (IntPtr)graphID, ref inputData);

                if (result != ErrorCode.SBSARIO_ERROR_OK)
                {
                    Debug.LogError($"Fail to update Substance input {inputID} for graph {graphID}");
                    throw new SubstanceException(result);
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                Marshal.FreeHGlobal(tempNativeMemory);
            }
        }

        public float GetInputFloat(int inputID, int graphID)
        {
            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_get_input(_handler, (IntPtr)graphID, (IntPtr)inputID, out NativeData inputDesc);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
            {
                Debug.LogError($"Fail to get Substance input {inputID} for graph {graphID}");
                throw new SubstanceException(result);
            }

            return inputDesc.Data.mFloatData0;
        }

        public Vector2 GetInputFloat2(int inputID, int graphID)
        {
            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_get_input(_handler, (IntPtr)graphID, (IntPtr)inputID, out NativeData inputDesc);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
            {
                Debug.LogError($"Fail to get Substance input {inputID} for graph {graphID}");
                throw new SubstanceException(result);
            }

            return new Vector2(inputDesc.Data.mFloatData0, inputDesc.Data.mFloatData1);
        }

        public Vector3 GetInputFloat3(int inputID, int graphID)
        {
            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_get_input(_handler, (IntPtr)graphID, (IntPtr)inputID, out NativeData inputDesc);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
            {
                Debug.LogError($"Fail to get Substance input {inputID} for graph {graphID}");
                throw new SubstanceException(result);
            }

            return new Vector3(inputDesc.Data.mFloatData0, inputDesc.Data.mFloatData1, inputDesc.Data.mFloatData2);
        }

        public Vector4 GetInputFloat4(int inputID, int graphID)
        {
            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_get_input(_handler, (IntPtr)graphID, (IntPtr)inputID, out NativeData inputDesc);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
            {
                Debug.LogError($"Fail to get Substance input {inputID} for graph {graphID}");
                throw new SubstanceException(result);
            }

            return new Vector4(inputDesc.Data.mFloatData0, inputDesc.Data.mFloatData1, inputDesc.Data.mFloatData2, inputDesc.Data.mFloatData2);
        }

        public int GetInputInt(int inputID, int graphID)
        {
            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_get_input(_handler, (IntPtr)graphID, (IntPtr)inputID, out NativeData inputDesc);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
            {
                Debug.LogError($"Fail to get Substance input {inputID} for graph {graphID}");
                throw new SubstanceException(result);
            }

            return inputDesc.Data.mIntData0;
        }

        public Vector2Int GetInputInt2(int inputID, int graphID)
        {
            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_get_input(_handler, (IntPtr)graphID, (IntPtr)inputID, out NativeData inputDesc);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
            {
                Debug.LogError($"Fail to get Substance input {inputID} for graph {graphID}");
                throw new SubstanceException(result);
            }

            return new Vector2Int(inputDesc.Data.mIntData0, inputDesc.Data.mIntData1);
        }

        public Vector3Int GetInputInt3(int inputID, int graphID)
        {
            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_get_input(_handler, (IntPtr)graphID, (IntPtr)inputID, out NativeData inputDesc);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
            {
                Debug.LogError($"Fail to get Substance input {inputID} for graph {graphID}");
                throw new SubstanceException(result);
            }

            return new Vector3Int(inputDesc.Data.mIntData0, inputDesc.Data.mIntData1, inputDesc.Data.mIntData2);
        }

        public int[] GetInputInt4(int inputID, int graphID)
        {
            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_get_input(_handler, (IntPtr)graphID, (IntPtr)inputID, out NativeData inputDesc);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
            {
                Debug.LogError($"Fail to get Substance input {inputID} for graph {graphID}");
                throw new SubstanceException(result);
            }

            return new int[] { inputDesc.Data.mIntData0, inputDesc.Data.mIntData1, inputDesc.Data.mIntData2, inputDesc.Data.mIntData2 };
        }

        public string GetInputString(int inputID, int graphID)
        {
            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_get_input(_handler, (IntPtr)graphID, (IntPtr)inputID, out NativeData inputDesc);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
            {
                Debug.LogError($"Fail to get Substance input {inputID} for graph {graphID}");
                throw new SubstanceException(result);
            }

            return Marshal.PtrToStringAnsi(inputDesc.Data.mPtr);
        }

        #endregion Input

        #region IDisposable

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (_handler != default)
                {
                    NativeMethods.sbsario_sbsar_close(_handler);
                    _handler = default;
                }

                _disposedValue = true;
            }
        }

        ~SubstanceNativeHandler()
        {
            Dispose(disposing: false);
        }

        #endregion IDisposable

        private void AssignInputDescription(int graphID, int inputID, SubstanceInputBase input)
        {
            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_get_input_desc(_handler, (IntPtr)graphID, (IntPtr)inputID, out NativeInputDesc inputDesc);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
                throw new SubstanceException(result);

            var identifier = inputDesc.mIdentifier == default ? null : Marshal.PtrToStringAnsi(inputDesc.mIdentifier);
            var label = inputDesc.mLabel == default ? null : Marshal.PtrToStringAnsi(inputDesc.mLabel);
            var guiGroup = inputDesc.GuiGroup == default ? null : Marshal.PtrToStringAnsi(inputDesc.GuiGroup);
            var guiDescription = inputDesc.GuiDescription == default ? null : Marshal.PtrToStringAnsi(inputDesc.GuiDescription);

            input.Description = new SubstanceInputDescription()
            {
                Identifier = identifier,
                Label = label,
                GuiGroup = guiGroup,
                GuiDescription = guiDescription,
                Type = ((ValueType)inputDesc.mValueType).ToUnity(),
                WidgetType = ((WidgetType)inputDesc.inputWidgetType).ToUnity()
            };

            if (input.IsNumeric)
                AssignNumericInputDescription(graphID, inputID, input);
        }

        private void AssignNumericInputDescription(int graphID, int inputID, SubstanceInputBase substanceInput)
        {
            ErrorCode result = (ErrorCode)NativeMethods.sbsario_sbsar_get_numeric_input_desc(_handler, (IntPtr)graphID, (IntPtr)inputID, out NativeNumericInputDesc inputDesc);

            if (result != ErrorCode.SBSARIO_ERROR_OK)
                throw new SubstanceException(result);

            substanceInput.SetNumericDescription(inputDesc);

            if ((int)(inputDesc.enumValueCount) != 0)
            {
                var count = (int)inputDesc.enumValueCount;

                NativeEnumInputDesc[] buffer = new NativeEnumInputDesc[count];
                GCHandle gcHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                IntPtr pointer = gcHandle.AddrOfPinnedObject();

                try
                {
                    result = (ErrorCode)NativeMethods.sbsario_sbsar_get_enum_input_desc(_handler, (IntPtr)graphID, (IntPtr)inputID, pointer, inputDesc.enumValueCount);
                    substanceInput.SetEnumOptions(buffer);

                    if (result != ErrorCode.SBSARIO_ERROR_OK)
                    {
                        Debug.LogError("Unable to get data for enum.");
                        return;
                    }
                }
                finally
                {
                    gcHandle.Free();
                }
            }
        }
    }
} // namespace Adobe.Substance