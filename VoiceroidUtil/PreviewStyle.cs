using System;
using System.Runtime.Serialization;
using RucheHome.AviUtl.ExEdit;
using RucheHome.Util;
using RucheHome.Voiceroid;

namespace VoiceroidUtil
{
    /// <summary>
    /// AviUtl拡張編集ファイル用のキャラ別スタイルを保持するクラス。
    /// </summary>
    [DataContract(Namespace = "")]
    public class PreviewStyle : BindableConfigBase
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="voiceroidId">VOICEROID識別ID。</param>
        public PreviewStyle()
        {
            // イベントハンドラ追加のためにプロパティ経由で設定
            this.Render = new RenderComponent();
            this.Text = new TextComponent();
        }

        /// <summary>
        /// 標準描画コンポーネントを取得または設定する。
        /// </summary>
        [DataMember]
        public RenderComponent Render
        {
            get => this.render;
            set =>
                this.SetPropertyWithPropertyChangedChain(
                    ref this.render,
                    value ?? new RenderComponent());
        }
        private RenderComponent render = null;

        /// <summary>
        /// テキストコンポーネントを取得または設定する。
        /// </summary>
        [DataMember]
        public TextComponent Text
        {
            get => this.text;
            set =>
                this.SetPropertyWithPropertyChangedChain(
                    ref this.text,
                    value ?? new TextComponent());
        }
        private TextComponent text = null;

        [DataMember]
        public int MarginLeft
        {
            get => this.marginLeft;
            set => this.SetProperty(ref this.marginLeft, value);
        }
        private int marginLeft = 20;

        [DataMember]
        public int MarginRight
        {
            get => this.marginRight;
            set => this.SetProperty(ref this.marginRight, value);
        }
        private int marginRight = 20;

        
        /// <summary>
        /// デシリアライズの直前に呼び出される。
        /// </summary>
        [OnDeserializing]
        private void OnDeserializing(StreamingContext context) =>
            this.ResetDataMembers(VoiceroidId.YukariEx);
    }
}
