using RucheHome.AviUtl.ExEdit;
using RucheHome.Util;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace VoiceroidUtil.TRToys
{
    [DataContract(Namespace = "")]
    public class PreviewRenderComponent : BindableConfigBase
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public PreviewRenderComponent() : base()
        {
            // イベントハンドラ追加のためにプロパティ経由で設定
            this.Scale = new PreviewMovableValue<ScaleConst>();
        }

        public PreviewRenderComponent(Action action) : base()
        {
            // イベントハンドラ追加のためにプロパティ経由で設定
            this.Scale = new PreviewMovableValue<ScaleConst>(action);
        }

        /// <summary>
        /// 拡大率を取得または設定する。
        /// </summary>
        [DataMember]
        public PreviewMovableValue<ScaleConst> Scale
        {
            get => this.scale;
            set =>
                this.SetPropertyWithPropertyChangedChain(
                    ref this.scale,
                    value ?? new PreviewMovableValue<ScaleConst>());
        }
        private PreviewMovableValue<ScaleConst> scale = null;

        /// <summary>
        /// デシリアライズの直前に呼び出される。
        /// </summary>
        [OnDeserializing]
        private void OnDeserializing(StreamingContext context) => this.ResetDataMembers();

        #region PreviewMovableValue{TConstants} ジェネリッククラス用の定数情報構造体群

        /// <summary>
        /// 拡大率用の定数情報クラス。
        /// </summary>
        [SuppressMessage("Design", "CA1034")]
        [SuppressMessage("Performance", "CA1815")]
        public struct ScaleConst : IMovableValueConstants
        {
            public int Digits => 2;
            public decimal DefaultValue => 100;
            public decimal MinValue => 0;
            public decimal MaxValue => 5000;
            public decimal MinSliderValue => 0;
            public decimal MaxSliderValue => 800;
        }

        #endregion
    }
}
