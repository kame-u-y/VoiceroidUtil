using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace VoiceroidUtil.TRToys
{
    [DataContract(Namespace = "")]
    public enum PreviewTextAlignment
    {
        /// <summary>
        /// 左上。
        /// </summary>
        [Display(Name = @"左寄せ[上]")]
        [EnumMember]
        TopLeft = 0,

        /// <summary>
        /// 中央上。
        /// </summary>
        [Display(Name = @"中央揃え[上]")]
        [EnumMember]
        TopCenter = 1,

        /// <summary>
        /// 右上。
        /// </summary>
        [Display(Name = @"右寄せ[上]")]
        [EnumMember]
        TopRight = 2,

        /// <summary>
        /// 左中央。
        /// </summary>
        [Display(Name = @"左寄せ[中]")]
        [EnumMember]
        MiddleLeft = 3,

        /// <summary>
        /// 中央。
        /// </summary>
        [Display(Name = @"中央揃え[中]")]
        [EnumMember]
        MiddleCenter = 4,

        /// <summary>
        /// 右中央。
        /// </summary>
        [Display(Name = @"右寄せ[中]")]
        [EnumMember]
        MiddleRight = 5,

        /// <summary>
        /// 左下。
        /// </summary>
        [Display(Name = @"左寄せ[下]")]
        [EnumMember]
        BottomLeft = 6,

        /// <summary>
        /// 中央下。
        /// </summary>
        [Display(Name = @"中央揃え[下]")]
        [EnumMember]
        BottomCenter = 7,

        /// <summary>
        /// 右下。
        /// </summary>
        [Display(Name = @"右寄せ[下]")]
        [EnumMember]
        BottomRight = 8,
    }
}
