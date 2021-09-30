using System;
using System.Runtime.Serialization;
using RucheHome.Util;

namespace VoiceroidUtil.TRToys
{
    /// <summary>
    /// TRT's拡張：プレビュー用の設定・スタイルを保持するクラス。
    /// </summary>
    [DataContract(Namespace = "")]
    public class PreviewStyle : BindableConfigBase
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public PreviewStyle()
        {
            // イベントハンドラ追加のためにプロパティ経由で設定
            this.Render = new PreviewRenderComponent();
            this.Text = new PreviewTextComponent();
        }

        /// <summary>
        /// 字幕テキストに対して改行・分割処理を適用するか否かを取得または設定する。
        /// </summary>
        [DataMember]
        public bool IsTextSplitting
        {
            get => this.textSplitting;
            set
            {
                this.SetProperty(ref this.textSplitting, value);
            }
        }
        private bool textSplitting = false;

        /// <summary>
        /// テキストボックスの折り返しを行うか否かを取得または設定する。
        /// </summary>
        [DataMember]
        public bool IsTextWrapping
        {
            get => this.isTextWrapping;
            set => this.SetProperty(ref this.isTextWrapping, value);
        }
        private bool isTextWrapping = true;

        /// <summary>
        /// 分割された複数のテキストファイルを保存するか否かを取得または設定する。
        /// 保存先に "(保存名)_0.txt" "(保存名)_1.txt" "(保存名)_2.txt" ... のように保存される。
        /// </summary>
        [DataMember]
        public bool IsSplitTextSaving
        {
            get => this.isSplitTextSaving;
            set => this.SetProperty(ref this.isSplitTextSaving, value);
        }
        private bool isSplitTextSaving = false;

        /// <summary>
        /// 改行・分割文字列を含むテキストファイルを保存するか否かを取得または設定する。
        /// 保存先下に「TextSplit_再編集用」フォルダがなければ作成され、その中に保存される
        /// </summary>
        [DataMember]
        public bool IsRawTextSaving
        {
            get => this.isRawTextSaving;
            set => this.SetProperty(ref this.isRawTextSaving, value);
        }
        private bool isRawTextSaving = false;

        /// <summary>
        /// 改行用の文字列を取得または設定する。
        /// 文字の設定は変更できないようにしている
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
        /// 文字の設定は変更できないようにしている
        /// </summary>
        [DataMember]
        public string FileSplitString
        {
            get => this.fileSplitString;
            set => this.SetProperty(ref this.fileSplitString, value);
        }
        private string fileSplitString = "/--";

        /// <summary>
        /// 音声合成のために改行・分割用文字列を置換する
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
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
                    value ?? new PreviewTextComponent());
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
            }
        }
        private int previewWindowWidth = 200;


        /// <summary>
        /// AviUtlに対するプレビューの横幅倍率をもとに、プレビュー上の字幕の左余白幅を設定する。
        /// </summary>
        private void SetPreviewLeftMargin()
            => this.PreviewLeftMargin =
                this.AviUtlLeftMargin * ((double)this.PreviewWindowWidth / (double)this.AviUtlWindowWidth);

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
            set => this.SetProperty(ref this.previewLeftMargin, value);
        }
        private double previewLeftMargin = 300.0 * 200.0 / 1920.0;


        /// <summary>
        /// AviUtlに対するプレビューの横幅倍率をもとに、プレビュー上の字幕の右余白幅を設定する。
        /// </summary>
        private void SetPreviewRightMargin()
            => this.PreviewRightMargin
                = this.AviUtlRightMargin * ((double)this.PreviewWindowWidth / (double)this.AviUtlWindowWidth);

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
        /// プレビューテキストのフォントサイズを設定する
        /// 依存するプロパティから呼び出される
        /// </summary>
        public void SetPreviewFontSize()
        {
            if (this.Text != null && this.Render != null)
            {
                Console.WriteLine("set PreviewFontSize");
                this.PreviewFontSize = 
                    this.Text.FontSize.Begin
                    * ((decimal)this.PreviewWindowWidth / (decimal)this.AviUtlWindowWidth)
                    * Render.Scale.Begin / (decimal)100.0;
            }
        }

        /// <summary>
        /// プレビューテキストのフォントサイズを取得または設定する。
        /// </summary>
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
            this.ResetDataMembers();
    }
}
