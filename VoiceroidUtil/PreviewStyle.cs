using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Windows;
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
        /// TRToys'拡張：字幕テキストに対して改行or分割処理を適用するか否かを取得または設定する。
        /// </summary>
        [DataMember]
        public bool IsTextSplitting
        {
            get => this.textSplitting;
            set => this.SetProperty(ref this.textSplitting, value);
        }
        private bool textSplitting = false;

        [DataMember]
        public string LineFeedString
        {
            get => this.lineFeedString;
            set => this.SetProperty(ref this.lineFeedString, value);
        }
        private string lineFeedString = "/-";

        [DataMember]
        public string FileSplitString
        {
            get => this.fileSplitString;
            set => this.SetProperty(ref this.fileSplitString, value);
        }
        private string fileSplitString = "/--";

        [DataMember]
        public bool IsPreviewReplacingToComma
        {
            get => this.isPreviewReplacingToComma;
            set => this.SetProperty(ref this.isPreviewReplacingToComma, value);
        }
        private bool isPreviewReplacingToComma = false;


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
            set
            {
                this.SetPropertyWithPropertyChangedChain(
                    ref this.text,
                    value ?? new TextComponent());
            }
        }
        private TextComponent text = null;

        [DataMember]
        public int AviUtlWindowWidth
        {
            get => this.aviUtlWindowWidth;
            set
            {
                this.SetProperty(ref this.aviUtlWindowWidth, value);
                this.setPreviewLeftMargin();
                this.setPreviewRightMargin();
            }
        }
        private int aviUtlWindowWidth = 1920;

        //private void setLeftMargin()
        //    => this.LeftMargin = this.PreviewWindowWidth * this.LeftMarginRatio;
        
        //private void setRightMargin()
        //    => this.RightMargin = this.PreviewWindowWidth * this.RightMarginRatio;
        private void setPreviewLeftMargin()
            => this.PreviewLeftMargin = this.AviUtlLeftMargin * ((double)this.PreviewWindowWidth / (double)this.AviUtlWindowWidth);

        private void setPreviewRightMargin()
            => this.PreviewRightMargin = this.AviUtlRightMargin * ((double)this.PreviewWindowWidth / (double)this.AviUtlWindowWidth);


        [DataMember]
        public int PreviewWindowWidth
        {
            get => this.previewWindowWidth;
            set
            {
                this.SetProperty(ref this.previewWindowWidth, value);
                //this.setLeftMargin();
                //this.setRightMargin();
                this.setPreviewLeftMargin();
                this.setPreviewRightMargin();
            }
        }
        private int previewWindowWidth = 200;

        //[DataMember]
        //public double LeftMarginRatio
        //{
        //    get => this.leftMarginRatio;
        //    set
        //    {
        //        Debug.WriteLine(value);
        //        this.SetProperty(ref this.leftMarginRatio, value);
        //        this.setLeftMargin();
        //    }
        //}
        //private double leftMarginRatio = 0.1;

        //public double LeftMargin
        //{
        //    get => this.leftMargin;
        //    set => this.SetProperty(ref this.leftMargin, value);
        //}
        //private double leftMargin = 20;

        [DataMember]
        public double AviUtlLeftMargin
        {
            get => this.aviUtlLeftMargin;
            set
            {
                this.SetProperty(ref this.aviUtlLeftMargin, value);
                this.setPreviewLeftMargin();
            }
        }
        private double aviUtlLeftMargin = 300;

        public double PreviewLeftMargin
        {
            get => this.previewLeftMargin;
            set => this.SetProperty(ref this.previewLeftMargin, value);
        }
        private double previewLeftMargin = 300.0 * 200.0 / 1920.0;

        //[DataMember]
        //public double RightMarginRatio
        //{
        //    get => this.rightMarginRatio;
        //    set
        //    {
        //        this.SetProperty(ref this.rightMarginRatio, value);
        //        this.setRightMargin();
        //    }
        //}
        //private double rightMarginRatio = 0.1;

        //public double RightMargin
        //{
        //    get => this.rightMargin;
        //    set => this.SetProperty(ref this.rightMargin, value);
        //}
        //private double rightMargin = 20;

        [DataMember]
        public double AviUtlRightMargin
        {
            get => this.aviUtlRightMargin;
            set
            {
                this.SetProperty(ref this.aviUtlRightMargin, value);
                this.setPreviewRightMargin();
            }
        }
        private double aviUtlRightMargin = 300;

        public double PreviewRightMargin
        {
            get => this.previewRightMargin;
            set => this.SetProperty(ref this.previewRightMargin, value);
        }
        private double previewRightMargin = 300.0 * 200.0 / 1920.0;

        /// <summary>
        /// デシリアライズの直前に呼び出される。
        /// </summary>
        [OnDeserializing]
        private void OnDeserializing(StreamingContext context) =>
            this.ResetDataMembers(VoiceroidId.YukariEx);
    }
}
