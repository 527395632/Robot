using System;

namespace Robot.Forms
{

/// <summary>
/// 阴影效果配置: 阴影偏移 (X/Y) 与模糊半径, 并据此计算阴影尺寸。
/// </summary>
internal class ShadowEffectConfiguration
{
    /// <summary>
    /// 阴影模糊半径。
    /// </summary>
    int _shadowBlur = 0;

    /// <summary>
    /// 阴影 X 偏移。
    /// </summary>
    int _shadowOffsetX = 0;

    /// <summary>
    /// 阴影 Y 偏移。
    /// </summary>
    int _shadowOffsetY = 0;

        /// <summary>
        /// 阴影 X 偏移, 取值范围 -20 到 20。
        /// </summary>
        public int OffsetX
        {
            get => _shadowOffsetX;
            set
            {
                if (value >= -20 && value <= 20)
                {
                    _shadowOffsetX = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException($"The value of {nameof(OffsetX)} should be -20 to 20.");

                }
            }
        }



        /// <summary>
        /// 阴影 Y 偏移, 取值范围 -20 到 20。
        /// </summary>
        public int OffsetY
        {
            get => _shadowOffsetY;
            set
            {
                if (value >= -20 && value <= 20)
                {
                    _shadowOffsetY = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException($"The value of {nameof(OffsetY)} should be -20 to 20.");

                }
            }
        }

        /// <summary>
        /// 阴影模糊半径, 取值范围 -25 到 25。
        /// </summary>
        public int Blur
        {
            get => _shadowBlur;
            set
            {
                if (value >= -25 && value <= 25)
                {
                    _shadowBlur = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException($"The value of {nameof(Blur)} should be -20 to 25.");
                }
            }
        }




        /// <summary>
        /// 阴影最大偏移量 (X/Y 偏移绝对值的较大者)。
        /// </summary>
        public int Offset => Math.Max(Math.Abs(OffsetX), Math.Abs(OffsetY));

        /// <summary>
        /// 阴影位图尺寸, 由模糊半径与偏移量共同决定。
        /// </summary>
        public int Size => Blur <= 0 && Offset == 0 ? 5 + Math.Abs(Blur) : (Blur + Math.Abs(Offset)) * 2;

        /// <summary>
        /// 阴影内偏移量 (固定 10)。
        /// </summary>
        public int InsideOffset => 10;
    }
}
