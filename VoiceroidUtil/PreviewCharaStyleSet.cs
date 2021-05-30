using System;
using System.Runtime.Serialization;

namespace VoiceroidUtil
{
    /// <summary>
    /// ExoCharaStyle インスタンスセットクラス。
    /// </summary>
    [DataContract(Namespace = "")]
    public class PreviewCharaStyleSet : VoiceroidItemSetBase<PreviewCharaStyle>
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public PreviewCharaStyleSet() : base()
        {
        }
    }
}
