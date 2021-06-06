using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Windows;
using RucheHome.AviUtl.ExEdit;
using RucheHome.Util;
using RucheHome.Voiceroid;
using VoiceroidUtil.TRToys;

namespace VoiceroidUtil
{
    /// <summary>
    /// TRToys'拡張：プレビュー用の設定・スタイルを保持するクラス。
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
            this.Render = new PreviewRenderComponent(() => this.SetPreviewFontSize());
            this.Text = new PreviewTextComponent(() => this.SetPreviewFontSize());
        }

        /// <summary>
        /// TRToys'拡張：字幕テキストに対して改行or分割処理を適用するか否かを取得または設定する。
        /// </summary>
        [DataMember]
        public bool IsTextSplitting
        {
            get => this.textSplitting;
            set
            {
                this.SetProperty(ref this.textSplitting, value);
                this.SetPreviewFontSize();
            }
        }
        private bool textSplitting = false;

        /// <summary>
        /// 改行用の文字列を取得または設定する。
        /// </summary>
        [DataMember]
        public string LineFeedString
        {
            get => this.lineFeedString;
            set => this.SetProperty(ref this.lineFeedString, value);
        }
        private string lineFeedString = "/-";


        /// <summary>
        /// プレビューシーン・テキストファイル分割用の文字列を取得または設定する。
        /// </summary>
        [DataMember]
        public string FileSplitString
        {
            get => this.fileSplitString;
            set => this.SetProperty(ref this.fileSplitString, value);
        }
        private string fileSplitString = "/--";

        public string RemovePreviewStrings(string text)
        {
            if (this.LineFeedString.Contains(this.FileSplitString))
            {
                return text.Replace(this.LineFeedString, "").Replace(FileSplitString, "");
            }
            else
            {
                return text.Replace(this.FileSplitString, "").Replace(LineFeedString, "");
            }
        }

        /// <summary>
        /// 改行・分割用文字列の文字列を音声再生時に「、」へ変換する
        /// </summary>
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
        public PreviewRenderComponent Render
        {
            get => this.render;
            set =>
                this.SetPropertyWithPropertyChangedChain(
                    ref this.render,
                    value ?? new PreviewRenderComponent());
        }
        private PreviewRenderComponent render = null;

        /// <summary>
        /// テキストコンポーネントを取得または設定する。
        /// </summary>
        [DataMember]
        public PreviewTextComponent Text
        {
            get => this.text;
            set
            {
                this.SetPropertyWithPropertyChangedChain(
                    ref this.text,
                    value ?? new PreviewTextComponent(()=> Debug.WriteLine("はっか")));
            }
        }
        private PreviewTextComponent text = null;

        /// <summary>
        /// 編集中のAviUtlプロジェクトの横幅を設定または取得する。
        /// </summary>
        [DataMember]
        public int AviUtlWindowWidth
        {
            get => this.aviUtlWindowWidth;
            set
            {
                this.SetProperty(ref this.aviUtlWindowWidth, value);
                this.SetPreviewLeftMargin();
                this.SetPreviewRightMargin();
                this.SetPreviewFontSize();
            }
        }
        private int aviUtlWindowWidth = 1920;

        /// <summary>
        /// プレビューウィンドウの横幅を設定または取得する。
        /// </summary>
        [DataMember]
        public int PreviewWindowWidth
        {
            get => this.previewWindowWidth;
            set
            {
                this.SetProperty(ref this.previewWindowWidth, value);
                this.SetPreviewLeftMargin();
                this.SetPreviewRightMargin();
                this.SetPreviewFontSize();
            }
        }
        private int previewWindowWidth = 200;
        
        /// <summary>
        /// AviUtl上の字幕の左余白幅を設定または取得する。
        /// </summary>
        [DataMember]
        public double AviUtlLeftMargin
        {
            get => this.aviUtlLeftMargin;
            set
            {
                this.SetProperty(ref this.aviUtlLeftMargin, value);
                this.SetPreviewLeftMargin();
            }
        }
        private double aviUtlLeftMargin = 300;
        
        /// <summary>
        /// プレビュー上の字幕の左余白幅を設定または取得する。
        /// </summary>
        public double PreviewLeftMargin
        {
            get => this.previewLeftMargin;
            private set => this.SetProperty(ref this.previewLeftMargin, value);
        }
        private double previewLeftMargin = 300.0 * 200.0 / 1920.0;
        
        /// <summary>
        /// AviUtlに対するプレビューの横幅倍率をもとに、プレビュー上の字幕の左余白幅を設定する。
        /// </summary>
        private void SetPreviewLeftMargin()
            => this.PreviewLeftMargin = 
                this.AviUtlLeftMargin * ((double)this.PreviewWindowWidth / (double)this.AviUtlWindowWidth);

        /// <summary>
        /// AviUtl上の字幕の右余白幅を設定または取得する。
        /// </summary>
        [DataMember]
        public double AviUtlRightMargin
        {
            get => this.aviUtlRightMargin;
            set
            {
                this.SetProperty(ref this.aviUtlRightMargin, value);
                this.SetPreviewRightMargin();
            }
        }
        private double aviUtlRightMargin = 300;

        /// <summary>
        /// プレビュー上の字幕の右余白幅を設定または取得する。
        /// </summary>
        public double PreviewRightMargin
        {
            get => this.previewRightMargin;
            set => this.SetProperty(ref this.previewRightMargin, value);
        }
        private double previewRightMargin = 300.0 * 200.0 / 1920.0;

        /// <summary>
        /// AviUtlに対するプレビューの横幅倍率をもとに、プレビュー上の字幕の右余白幅を設定する。
        /// </summary>
        private void SetPreviewRightMargin()
            => this.PreviewRightMargin
                = this.AviUtlRightMargin * ((double)this.PreviewWindowWidth / (double)this.AviUtlWindowWidth);

        private void SetPreviewFontSize()
        {
            this.PreviewFontSize = this.Text.FontSize.Begin
                * ((decimal)this.PreviewWindowWidth / (decimal)this.AviUtlWindowWidth)
                * Render.Scale.Begin / (decimal)100.0;
        }
        public decimal PreviewFontSize
        {
            get => this.previewFontSize;
            set => this.SetProperty(ref this.previewFontSize, value);
        }
        private decimal previewFontSize = 100;

    /// <summary>
    /// デシリアライズの直前に呼び出される。
    /// </summary>
    [OnDeserializing]
        private void OnDeserializing(StreamingContext context) =>
            this.ResetDataMembers(VoiceroidId.YukariEx);
    }
}
