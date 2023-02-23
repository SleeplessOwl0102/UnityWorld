# ﻿Include  Path

#include "Assets/@RenURP/ShaderLib/Core.hlsl"
#include "Assets/@RenURP/ShaderLib/Input.hlsl"
#include "Assets/@RenURP/ShaderLib/Math.hlsl"

## 

# BlendMode

紀錄常見的BlendMode模式

- Add
  Blend One One

- Darken
  BlendOp Min
  Blend One One

- Lighten
  BlendOp Max
  Blend One One

- Linear Burn
  BlendOp RevSub
  Blend One One

- Multiply
  Blend DstColor OneMinusSrcAlpha