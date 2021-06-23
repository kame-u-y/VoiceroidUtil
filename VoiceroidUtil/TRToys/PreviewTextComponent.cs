﻿using System;
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
    /// <summary>
    /// TextComponentをもとに作成、プレビューのテキスト関連の設定値を管理する
    /// </summary>
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
        public static readonly string DefaultFontFamilyName = @"MS UI Gothic";

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
        public static IDictionary<string, Uri> FontPathDictionary
        {
            get
            {
                return fontPathDictionary;
            }
        }
        private static IDictionary<string, Uri> fontPathDictionary =
            SearchFontNamePathPair();

        /// <summary>
        /// 辞書の作成
        /// </summary>
        /// <returns></returns>
        static IDictionary<string, Uri> SearchFontNamePathPair()
        {
            IDictionary<string, Uri> dic = new SortedDictionary<string, Uri>();

            // ユーザ共通のフォントフォルダと、ユーザ個別のフォントフォルダのパスを取得
            string FontDir = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
            string UserFontDir = 
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
                + @"\AppData\Local\Microsoft\Windows\Fonts\";

            // .ttf, .otf, .ttcを取得し、Glyphsで読み込めるUriを生成
            var uris = Directory.GetFiles(FontDir, "*.ttf")
                .Concat(Directory.GetFiles(FontDir, "*.otf"))
                    .Select(p => new Uri(p))
                .Concat(Directory.GetFiles(FontDir, "*.ttc")
                    .SelectMany(p =>
                    {
                        using (var fs = new FileStream(p, FileMode.Open, FileAccess.Read))
                        {
                            return Enumerable.Range(0, TypefaceInfo.GetCollectionCount(fs))
                                .Select(i => new UriBuilder("file", "", -1, p, "#" + i).Uri);
                        }
                    }));
            if (Directory.Exists(UserFontDir))
            {
                var userUris = Directory.GetFiles(UserFontDir, "*.ttf")
                    .Concat(Directory.GetFiles(UserFontDir, "*.otf"))
                        .Select(p => new Uri(p))
                    .Concat(Directory.GetFiles(UserFontDir, "*.ttc")
                        .SelectMany(p =>
                        {
                            using (var fs = new FileStream(p, FileMode.Open, FileAccess.Read))
                            {
                                return Enumerable.Range(0, TypefaceInfo.GetCollectionCount(fs))
                                    .Select(i => new UriBuilder("file", "", -1, p, "#" + i).Uri);
                            }
                        }));
                uris = uris.Concat(userUris);
            }

            // 一時的な辞書を作成（FontName->FontFace->Uri）
            //     FaceNameについて、Boldは通常、Regular（もしくはフォント特有のFaceName）の太字モードとして扱われるが、
            //     RegularがなくBoldのみのフォント（例：Unispace、UD デジタル 教科書体 N-B）が存在するため、
            //     BoldのみであるかチェックするためにFaceNameを一覧する辞書を作成する
            IDictionary<string, Dictionary<string, Uri>> nameFaceToPath = new Dictionary<string, Dictionary<string, Uri>>();
            var generalCultureName = "en-US";
            var exceptionalCultureName = "ja-JP";
            var exceptionalFaceName = "exceptionalFaceName";
            foreach (Uri uri in uris)
            {
                try
                {
                    GlyphTypeface gtf = new GlyphTypeface(uri);
                    foreach (string familyName in gtf.FamilyNames.Values)
                    {
                        var cultureName =
                            gtf.FaceNames.ContainsKey(new CultureInfo(generalCultureName))
                                ? generalCultureName
                                : exceptionalCultureName;
                        var culture = new CultureInfo(cultureName);

                        // "○○ Bold"と"○○ Italic"を"○○"に
                        var faceName = gtf.FaceNames[culture]
                                .Replace(" Bold", string.Empty)
                                .Replace(" Italic", string.Empty);
                        if (cultureName == exceptionalCultureName)
                        {
                            faceName = string.Empty;
                        }
                        else
                        {
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
                        }

                        var fontName = (faceName != "")
                            ? $"{familyName} {faceName}"
                            : $"{familyName}";

                        // 同一のfontNameが存在しなければAdd
                        if (!nameFaceToPath.ContainsKey(fontName))
                        {
                            nameFaceToPath.Add(fontName, new Dictionary<string, Uri>());
                        }

                        // fontName内に同一のfontFaceが存在しなければAdd
                        if (cultureName == generalCultureName)
                        {
                            if (!nameFaceToPath[fontName].ContainsKey(gtf.FaceNames[culture]))
                            {
                                nameFaceToPath[fontName].Add(gtf.FaceNames[culture], uri);
                            }
                        }
                        else
                        {
                            if (!nameFaceToPath[fontName].ContainsKey(exceptionalFaceName))
                            {
                                nameFaceToPath[fontName].Add(exceptionalFaceName, uri);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(uri);
                    Console.WriteLine(e);
                    Console.WriteLine();
                    continue;
                }
            }

            // 最終的な辞書の作成
            foreach (string fontName in nameFaceToPath.Keys)
            {
                try
                {
                    var faceName = "";
                    if (nameFaceToPath[fontName].Keys.Count == 1 && nameFaceToPath[fontName].ContainsKey("Bold"))
                    {
                        faceName = "Bold";
                    }
                    else if (nameFaceToPath[fontName].Keys.Count == 1 && nameFaceToPath[fontName].ContainsKey("Italic"))
                    {
                        faceName = "Italic";
                    }
                    else if (nameFaceToPath[fontName].Keys.Count == 1 && nameFaceToPath[fontName].ContainsKey("Bold Italic"))
                    {
                        faceName = "Bold Italic";
                    }
                    else if (nameFaceToPath[fontName].ContainsKey("Regular"))
                    {
                        faceName = "Regular";
                    }
                    else if (fontName.LastIndexOf(" ") != -1)
                    {
                        faceName = fontName.Substring(fontName.LastIndexOf(" ") + 1);
                    }
                    else
                    {
                        faceName = exceptionalFaceName;
                    }
                    dic.Add(fontName, nameFaceToPath[fontName][faceName]);
                }
                catch (Exception e)
                {
                    Console.WriteLine(fontName);
                    Console.WriteLine(e);
                    Console.WriteLine();
                    continue;
                }
            }

            return dic;
        }

        public bool IsExistFontNamePathPair(string familyName, Uri fontUri)
            =>FontPathDictionary.Contains(new KeyValuePair<string, Uri>(familyName, fontUri));

        /// <summary>
        /// フォントファミリー名からフォントUriを取得
        /// </summary>
        [DataMember]
        public Uri PreviewFontUri
        {
            get => this.previewFontUri;
            set
            {
                this.SetProperty(ref this.previewFontUri, value);
            }
        }
        private Uri previewFontUri = FontPathDictionary[DefaultFontFamilyName];

        public void SetPreviewFontUri(string familyName)
        {
            if (FontPathDictionary.ContainsKey(familyName))
            {
                this.PreviewFontUri = FontPathDictionary[familyName];
            }
            else
            {
                this.PreviewFontUri = FontPathDictionary[DefaultFontFamilyName];
            }
        }


        /// <summary>
        /// テキスト配置種別を取得または設定する。
        /// </summary>
        public PreviewTextAlignment TextAlignment
        {
            get => this.textAlignment;
            set
            {
                this.SetProperty(
                    ref this.textAlignment,
                    Enum.IsDefined(value.GetType(), value) ?
                        value : PreviewTextAlignment.TopLeft);
                this.Horizontal = GetHorizontal();
            }
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

        /// <summary>
        /// 縦方向について、TextAlignmentの位置を判定する
        /// </summary>
        /// <param name="alignment"></param>
        /// <returns></returns>
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
        
        /// <summary>
        /// 上中下それぞれでの行間を取得
        /// </summary>
        /// <returns></returns>
        private Thickness GetPreviewLineSpace()
        {
            var val = 0;
            if (IsTopAlignment(this.TextAlignment))
                return new Thickness(0, 0, 0, val);
            else if (IsMiddleAlignment(this.TextAlignment))
                return new Thickness(0, val / 2.0, 0, val / 2.0);
            else if (IsBottomAlignment(this.TextAlignment))
                return new Thickness(0, val, 0, 0);
            else
                return new Thickness(0);
        }

        /// <summary>
        /// プレビューの行間を取得する
        /// 行間は設定項目には含まない
        /// </summary>
        public Thickness PreviewLineSpace
        {
            get => GetPreviewLineSpace();
        }
        
        /// <summary>
        /// TextAlignmentの位置からVerticalAlignmentを取得する
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// View用のVerticalAlignmentを取得する
        /// </summary>
        public VerticalAlignment Vertical
        {
            get => GetVertical();
        }

        /// <summary>
        /// 横方向について、TextAlignmentの位置を判定する
        /// </summary>
        /// <param name="alignment"></param>
        /// <returns></returns>
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

        /// <summary>
        /// TextAlignmentの位置からHorizontalAlignmentを取得する
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// View用のHorizontalAlignmentを取得する
        /// </summary>
        public HorizontalAlignment Horizontal
        {
            get => this.horizontal;
            set => SetProperty(ref horizontal, value);
        }
        private HorizontalAlignment horizontal = HorizontalAlignment.Center;

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
