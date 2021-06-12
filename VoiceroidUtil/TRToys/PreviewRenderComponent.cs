using RucheHome.AviUtl.ExEdit;
using RucheHome.Text;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace VoiceroidUtil.TRToys
{
    [DataContract(Namespace = "")]
    public class PreviewRenderComponent : ComponentBase, ICloneable
    {
       
        /// <summary>
        /// コンポーネント名。
        /// </summary>
        public static readonly string ThisComponentName = @"標準描画";

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
        /// コピーコンストラクタ。
        /// </summary>
        /// <param name="src">コピー元。</param>
        public PreviewRenderComponent(PreviewRenderComponent src) : base()
        {
            if (src == null)
            {
                throw new ArgumentNullException(nameof(src));
            }

            src.CopyToCore(this);
        }

        /// <summary>
        /// コンポーネント名を取得する。
        /// </summary>
        public override string ComponentName => ThisComponentName;


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
        /// このコンポーネントのクローンを作成する。
        /// </summary>
        /// <returns>クローン。</returns>
        public PreviewRenderComponent Clone() => new PreviewRenderComponent(this);

        

        /// <summary>
        /// デシリアライズの直前に呼び出される。
        /// </summary>
        [OnDeserializing]
        private void OnDeserializing(StreamingContext context) => this.ResetDataMembers();

        #region ICloneable の明示的実装

        /// <summary>
        /// このオブジェクトのクローンを作成する。
        /// </summary>
        /// <returns>クローン。</returns>
        object ICloneable.Clone() => this.Clone();

        #endregion

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
