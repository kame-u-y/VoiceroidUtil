using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;
using RucheHome.AviUtl.ExEdit;
using RucheHome.Util;
using RucheHome.Util.Extensions.String;
using WaterTrans.TypeLoader;

namespace VoiceroidUtil.TRToys
{
    [DataContract(Namespace = "")]
    public class PreviewTextComponent : BindableConfigBase
    {
        #region アイテム名定数群

        /// <summary>
        /// フォントサイズを保持する拡張編集オブジェクトファイルアイテムの名前。
        /// </summary>
        public const string ExoFileItemNameOfFontSize = @"サイズ";

        #endregion

        /// <summary>
        /// 規定のフォントファミリ名。
        /// </summary>
        public static readonly string DefaultFontFamilyName = @"MS Gothic";

        /// <summary>
        /// テキストの最大許容文字数。
        /// </summary>
        public static readonly int TextLengthLimit = 1024 - 1;

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public PreviewTextComponent() : base()
        {
            this.FontSize = new PreviewMovableValue<FontSizeConst>();
        }

        public PreviewTextComponent(Action action) : base()
        {
            // イベントハンドラ追加のためにプロパティ経由で設定
            this.FontSize = new PreviewMovableValue<FontSizeConst>(action);
        }

        /// <summary>
        /// フォントサイズを取得または設定する。
        /// </summary>
        [ExoFileItem(ExoFileItemNameOfFontSize, Order = 1)]
        [DataMember]
        public PreviewMovableValue<FontSizeConst> FontSize
        {
            get => this.fontSize;
            set
            {
                this.SetPropertyWithPropertyChangedChain(
                    ref this.fontSize,
                    value ?? new PreviewMovableValue<FontSizeConst>());
            }
        }
        private PreviewMovableValue<FontSizeConst> fontSize = null;

        /// <summary>
        /// フォント色を取得または設定する。
        /// </summary>
        [DataMember]
        public Color FontColor
        {
            get => this.fontColor;
            set
            {
                this.SetProperty(
                    ref this.fontColor,
                    Color.FromRgb(value.R, value.G, value.B));
            }
        }
        private Color fontColor = Colors.Black;

        public SolidColorBrush PreviewFontColor
        {
            get => new SolidColorBrush(this.FontColor);
        }

        /// <summary>
        /// フォントファミリ名を取得または設定する。
        /// </summary>
        [DataMember]
        public string FontFamilyName
        {
            get => this.fontFamilyName;
            set
            {
                this.SetProperty(ref this.fontFamilyName, value ?? DefaultFontFamilyName);
            }
        }
        private string fontFamilyName = DefaultFontFamilyName;


        /// <summary>
        /// フォントファミリー名とフォントUriとが対応する辞書
        /// </summary>
        static IDictionary<string, Uri> FontPathDictionary
        {
            get
            {
                return fontPathDictionary;
            }
        }
        static readonly IDictionary<string, Uri> fontPathDictionary =
            SearchFontNamePathPair();
        static IDictionary<string, Uri> SearchFontNamePathPair()
        {
            IDictionary<string, Uri> dic = new SortedDictionary<string, Uri>();

            string FontDir = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
            string UserFontDir = 
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
                + @"\AppData\Local\Microsoft\Windows\Fonts\";

            var uris = 
                Directory.GetFiles(FontDir, "*.ttf")
                    .Concat(Directory.GetFiles(FontDir, "*.otf"))
                    .Concat(Directory.GetFiles(UserFontDir, "*.ttf"))
                    .Concat(Directory.GetFiles(UserFontDir, "*.otf")).Select(p => new Uri(p))
                    .Concat(
                        Directory.GetFiles(FontDir, "*.ttc")
                            .Concat(Directory.GetFiles(UserFontDir, "*.ttc")

                    ).SelectMany(p => 
                    {
                        using (var fs = new FileStream(p, FileMode.Open, FileAccess.Read))
                        {
                            return Enumerable.Range(0, TypefaceInfo.GetCollectionCount(fs))
                                .Select(i => new UriBuilder("file", "", -1, p, "#" + i).Uri);
                        }
                    }));

            // FaceNameについて、Boldは通常、Regular（もしくはフォント特有のFaceName）の太字モードとして扱われるが、
            // RegularがなくBoldのみのフォント（例：Unispace、UD デジタル 教科書体 N-B）が存在するため、
            // BoldのみであるかチェックするためにFaceNameを一覧する辞書を作成する
            IDictionary<string, Dictionary<string, Uri>> nameFaceToPath = new Dictionary<string, Dictionary<string, Uri>>(); 
            foreach (Uri uri in uris)
            {
                try
                {
                    GlyphTypeface gtf = new GlyphTypeface(uri);
                    var cultureEnUS = new CultureInfo("en-US");
                    foreach (string familyName in gtf.FamilyNames.Values)
                    {
                        var faceName = gtf.FaceNames[cultureEnUS]
                                .Replace(" Bold", string.Empty)
                                .Replace(" Italic", string.Empty);
                        if (faceName == "Regular")
                        {
                            faceName = faceName.Replace("Regular", string.Empty);
                        }
                        else if (faceName == "Bold")
                        {
                            faceName = faceName.Replace("Bold", string.Empty);
                        }
                        else if (faceName == "Italic")
                        {
                            faceName = faceName.Replace("Italic", string.Empty);
                        }

                        var fontName = (faceName != "")
                            ? $"{familyName} {faceName}"
                            : $"{familyName}";
                        if (!nameFaceToPath.ContainsKey(fontName))
                        {
                            nameFaceToPath.Add(fontName, new Dictionary<string, Uri>());
                        }
                        if (!nameFaceToPath[fontName].ContainsKey(gtf.FaceNames[cultureEnUS]))
                        {
                            nameFaceToPath[fontName].Add(gtf.FaceNames[cultureEnUS], uri);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            // 最終的な辞書の作成
            foreach (string fontName in nameFaceToPath.Keys)
            {
                if (nameFaceToPath[fontName].Keys.Count==1 && nameFaceToPath[fontName].ContainsKey("Bold"))
                {
                    dic.Add(fontName, nameFaceToPath[fontName]["Bold"]);
                } 
                else if (nameFaceToPath[fontName].Keys.Count == 1 && nameFaceToPath[fontName].ContainsKey("Italic"))
                {
                    dic.Add(fontName, nameFaceToPath[fontName]["Italic"]);
                }
                else if (nameFaceToPath[fontName].Keys.Count == 1 && nameFaceToPath[fontName].ContainsKey("Bold Italic"))
                {
                    dic.Add(fontName, nameFaceToPath[fontName]["Bold Italic"]);
                }
                else if (nameFaceToPath[fontName].ContainsKey("Regular"))
                {
                    dic.Add(fontName, nameFaceToPath[fontName]["Regular"]);
                }
                else
                {
                    var faceName = fontName.Substring(fontName.LastIndexOf(" ")+1);
                    dic.Add(fontName, nameFaceToPath[fontName][faceName]);
                }
            }

            return dic;
        }

        public Uri PreviewFontUri
        {
            get
            {
                try
                {
                    return FontPathDictionary.ContainsKey(this.FontFamilyName)
                        ? FontPathDictionary[this.FontFamilyName]
                        : FontPathDictionary["MS Gothic"];
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return FontPathDictionary["MS Gothic"];
                }
            }
        }


        /// <summary>
        /// テキスト配置種別を取得または設定する。
        /// </summary>
        public PreviewTextAlignment TextAlignment
        {
            get => this.textAlignment;
            set =>
                this.SetProperty(
                    ref this.textAlignment,
                    Enum.IsDefined(value.GetType(), value) ?
                        value : PreviewTextAlignment.TopLeft);
        }
        private PreviewTextAlignment textAlignment = PreviewTextAlignment.TopLeft;

        /// <summary>
        /// TextAlignment プロパティのシリアライズ用ラッパプロパティ。
        /// </summary>
        [DataMember(Name = nameof(PreviewTextAlignment))]
        [SuppressMessage("CodeQuality", "IDE0051")]
        private string TextAlignmentString
        {
            get => this.TextAlignment.ToString();
            set =>
                this.TextAlignment =
                    Enum.TryParse(value, out PreviewTextAlignment deco) ?
                        deco : PreviewTextAlignment.TopLeft;
        }


        /// <summary>
        /// 字間幅を取得または設定する。
        /// </summary>
        [DataMember]
        public int LetterSpace
        {
            get => this.letterSpace;
            set =>
                this.SetProperty(
                    ref this.letterSpace,
                    Math.Min(Math.Max(-100, value), 100));
        }
        private int letterSpace = 0;

        /// <summary>
        /// 行間幅を取得または設定する。
        /// </summary>
        [DataMember]
        public int LineSpace
        {
            get => this.lineSpace;
            set =>
                this.SetProperty(
                    ref this.lineSpace,
                    Math.Min(Math.Max(-100, value), 100));
        }
        private int lineSpace = 0;


        /// <summary>
        /// テキストを取得または設定する。
        /// </summary>
        [DataMember]
        public string Text
        {
            get => this.text;
            set
            {
                var v = value ?? "";
                if (v.Length > TextLengthLimit)
                {
                    v = v.RemoveSurrogateSafe(TextLengthLimit);
                }

                this.SetProperty(ref this.text, v);
            }
        }
        private string text = "";

        private bool IsTopAlignment(PreviewTextAlignment alignment)
            => alignment == PreviewTextAlignment.TopLeft
            || alignment == PreviewTextAlignment.TopCenter
            || alignment == PreviewTextAlignment.TopRight;
        private bool IsMiddleAlignment(PreviewTextAlignment alignment)
            => alignment == PreviewTextAlignment.MiddleLeft
            || alignment == PreviewTextAlignment.MiddleCenter
            || alignment == PreviewTextAlignment.MiddleRight;
        private bool IsBottomAlignment(PreviewTextAlignment alignment)
            => alignment == PreviewTextAlignment.BottomLeft
            || alignment == PreviewTextAlignment.BottomCenter
            || alignment == PreviewTextAlignment.BottomRight;
        
        private Thickness GetPreviewLineSpace()
        {
            var val = 2;
            if (IsTopAlignment(this.TextAlignment))
                return new Thickness(0, 0, 0, val);
            else if (IsMiddleAlignment(this.TextAlignment))
                return new Thickness(0, val / 2.0, 0, val / 2.0);
            else if (IsBottomAlignment(this.TextAlignment))
                return new Thickness(0, val, 0, 0);
            else
                return new Thickness(0);
        }

        public Thickness PreviewLineSpace
        {
            get => GetPreviewLineSpace();
        }
        
        private VerticalAlignment GetVertical()
        {
            if (IsTopAlignment(this.TextAlignment))
                return VerticalAlignment.Top;
            else if (IsMiddleAlignment(this.TextAlignment))
                return VerticalAlignment.Center;
            else if (IsBottomAlignment(this.TextAlignment))
                return VerticalAlignment.Bottom;
            else
                return VerticalAlignment.Center;
        }

        public VerticalAlignment Vertical
        {
            get => GetVertical();
        }

        private bool IsLeftAlignment(PreviewTextAlignment alignment)
                => alignment == PreviewTextAlignment.TopLeft
                || alignment == PreviewTextAlignment.MiddleLeft
                || alignment == PreviewTextAlignment.BottomLeft;
        private bool IsCenterAlignment(PreviewTextAlignment alignment)
            => alignment == PreviewTextAlignment.TopCenter
            || alignment == PreviewTextAlignment.MiddleCenter
            || alignment == PreviewTextAlignment.BottomCenter;
        private bool IsRightAlignment(PreviewTextAlignment alignment)
            => alignment == PreviewTextAlignment.TopRight
            || alignment == PreviewTextAlignment.MiddleRight
            || alignment == PreviewTextAlignment.BottomRight;

        private HorizontalAlignment GetHorizontal()
        {
            if (IsLeftAlignment(this.TextAlignment))
                return HorizontalAlignment.Left;
            else if (IsCenterAlignment(this.TextAlignment))
                return HorizontalAlignment.Center;
            else if (IsRightAlignment(this.TextAlignment))
                return HorizontalAlignment.Right;
            else
                return HorizontalAlignment.Center;
        }

        public HorizontalAlignment Horizontal
        {
            get => GetHorizontal();
        }

        /// <summary>
        /// デシリアライズの直前に呼び出される。
        /// </summary>
        [OnDeserializing]
        private void OnDeserializing(StreamingContext context) => this.ResetDataMembers();

        #region PreviewMovableValue{TConstants} ジェネリッククラス用の定数情報構造体群

        /// <summary>
        /// フォントサイズ用の定数情報クラス。
        /// </summary>
        [SuppressMessage("Design", "CA1034")]
        [SuppressMessage("Performance", "CA1815")]
        public struct FontSizeConst : IMovableValueConstants
        {
            public int Digits => 0;
            public decimal DefaultValue => 100;
            public decimal MinValue => 0;
            public decimal MaxValue => 1000;
            public decimal MinSliderValue => 0;
            public decimal MaxSliderValue => 256;
        }

        #endregion
    }
}
