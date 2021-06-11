using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;
using RucheHome.AviUtl.ExEdit;
using RucheHome.Text;
using RucheHome.Util.Extensions.String;
using WaterTrans.TypeLoader;
//using TextAlignment = RucheHome.AviUtl.ExEdit.TextAlignment;
using PreviewTextAlignment = VoiceroidUtil.TRToys.PreviewTextAlignment;

namespace VoiceroidUtil.TRToys
{
    [DataContract(Namespace = "")]
    public class PreviewTextComponent : ComponentBase, ICloneable
    {
        #region アイテム名定数群

        /// <summary>
        /// フォントサイズを保持する拡張編集オブジェクトファイルアイテムの名前。
        /// </summary>
        public const string ExoFileItemNameOfFontSize = @"サイズ";

        /// <summary>
        /// 表示速度を保持する拡張編集オブジェクトファイルアイテムの名前。
        /// </summary>
        public const string ExoFileItemNameOfTextSpeed = @"表示速度";

        /// <summary>
        /// 自動スクロールフラグを保持する拡張編集オブジェクトファイルアイテムの名前。
        /// </summary>
        public const string ExoFileItemNameOfIsAutoScrolling = @"自動スクロール";

        /// <summary>
        /// 個別オブジェクト化フラグを保持する拡張編集オブジェクトファイルアイテムの名前。
        /// </summary>
        public const string ExoFileItemNameOfIsIndividualizing =
            @"文字毎に個別オブジェクト";

        /// <summary>
        /// 移動座標上表示フラグを保持する拡張編集オブジェクトファイルアイテムの名前。
        /// </summary>
        public const string ExoFileItemNameOfIsAligningOnMotion = @"移動座標上に表示する";

        /// <summary>
        /// 高さ自動調整フラグを保持する拡張編集オブジェクトファイルアイテムの名前。
        /// </summary>
        public const string ExoFileItemNameOfIsAutoAdjusting = @"autoadjust";

        /// <summary>
        /// フォント色を保持する拡張編集オブジェクトファイルアイテムの名前。
        /// </summary>
        public const string ExoFileItemNameOfFontColor = @"color";

        /// <summary>
        /// フォント装飾色を保持する拡張編集オブジェクトファイルアイテムの名前。
        /// </summary>
        public const string ExoFileItemNameOfFontDecorationColor = @"color2";

        /// <summary>
        /// フォントファミリ名を保持する拡張編集オブジェクトファイルアイテムの名前。
        /// </summary>
        public const string ExoFileItemNameOfFontFamilyName = @"font";

        /// <summary>
        /// フォント装飾種別を保持する拡張編集オブジェクトファイルアイテムの名前。
        /// </summary>
        public const string ExoFileItemNameOfFontDecoration = @"type";

        /// <summary>
        /// テキスト配置種別を保持する拡張編集オブジェクトファイルアイテムの名前。
        /// </summary>
        public const string ExoFileItemNameOfTextAlignment = @"align";

        /// <summary>
        /// 太字フラグを保持する拡張編集オブジェクトファイルアイテムの名前。
        /// </summary>
        public const string ExoFileItemNameOfIsBold = @"B";

        /// <summary>
        /// イタリック体フラグを保持する拡張編集オブジェクトファイルアイテムの名前。
        /// </summary>
        public const string ExoFileItemNameOfIsItalic = @"I";

        /// <summary>
        /// 字間幅を保持する拡張編集オブジェクトファイルアイテムの名前。
        /// </summary>
        public const string ExoFileItemNameOfLetterSpace = @"spacing_x";

        /// <summary>
        /// 行間幅を保持する拡張編集オブジェクトファイルアイテムの名前。
        /// </summary>
        public const string ExoFileItemNameOfLineSpace = @"spacing_y";

        /// <summary>
        /// 高精細モード有効フラグを保持する拡張編集オブジェクトファイルアイテムの名前。
        /// </summary>
        public const string ExoFileItemNameOfIsHighDefinition = @"precision";

        /// <summary>
        /// 滑らかフラグを保持する拡張編集オブジェクトファイルアイテムの名前。
        /// </summary>
        public const string ExoFileItemNameOfIsSoft = @"soft";

        /// <summary>
        /// 等間隔モード有効フラグを保持する拡張編集オブジェクトファイルアイテムの名前。
        /// </summary>
        public const string ExoFileItemNameOfIsMonospacing = @"monospace";

        /// <summary>
        /// テキストを保持する拡張編集オブジェクトファイルアイテムの名前。
        /// </summary>
        public const string ExoFileItemNameOfText = @"text";

        #endregion

        /// <summary>
        /// コンポーネント名。
        /// </summary>
        public static readonly string ThisComponentName = @"テキスト";

        /// <summary>
        /// 規定のフォントファミリ名。
        /// </summary>
        public static readonly string DefaultFontFamilyName = @"MS Gothic";

        /// <summary>
        /// テキストの最大許容文字数。
        /// </summary>
        public static readonly int TextLengthLimit = 1024 - 1;

        /// <summary>
        /// 拡張編集オブジェクトファイルのアイテムコレクションに
        /// コンポーネント名が含まれているか否かを取得する。
        /// </summary>
        /// <param name="items">アイテムコレクション。</param>
        /// <returns>含まれているならば true 。そうでなければ false 。</returns>
        public static bool HasComponentName(IniFileItemCollection items) =>
            HasComponentNameCore(items, ThisComponentName);

        /// <summary>
        /// 拡張編集オブジェクトファイルのアイテムコレクションから
        /// コンポーネントを作成する。
        /// </summary>
        /// <param name="items">アイテムコレクション。</param>
        /// <returns>コンポーネント。</returns>
        //public static PreviewTextComponent FromExoFileItems(IniFileItemCollection items) =>
        //    FromExoFileItemsCore(items, () => new PreviewTextComponent());

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public PreviewTextComponent(Action action) : base()
        {
            // イベントハンドラ追加のためにプロパティ経由で設定
            this.FontSize = new PreviewMovableValue<FontSizeConst>(action);
            this.TextSpeed = new PreviewMovableValue<TextSpeedConst>();

            //this.SetPreviewFontSize = setPreviewFontSize;
        }

        /// <summary>
        /// コピーコンストラクタ。
        /// </summary>
        /// <param name="src">コピー元。</param>
        public PreviewTextComponent(PreviewTextComponent src) : base()
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
        /// 表示速度を取得または設定する。
        /// </summary>
        [ExoFileItem(ExoFileItemNameOfTextSpeed, Order = 2)]
        [DataMember]
        public PreviewMovableValue<TextSpeedConst> TextSpeed
        {
            get => this.textSpeed;
            set =>
                this.SetPropertyWithPropertyChangedChain(
                    ref this.textSpeed,
                    value ?? new PreviewMovableValue<TextSpeedConst>());
        }
        private PreviewMovableValue<TextSpeedConst> textSpeed = null;

        /// <summary>
        /// 自動スクロールするか否かを取得または設定する。
        /// </summary>
        [ExoFileItem(ExoFileItemNameOfIsAutoScrolling, Order = 5)]
        [DataMember]
        public bool IsAutoScrolling
        {
            get => this.autoScrolling;
            set => this.SetProperty(ref this.autoScrolling, value);
        }
        private bool autoScrolling = false;

        /// <summary>
        /// 文字毎に個別オブジェクトとするか否かを取得または設定する。
        /// </summary>
        [ExoFileItem(ExoFileItemNameOfIsIndividualizing, Order = 3)]
        [DataMember]
        public bool IsIndividualizing
        {
            get => this.individualizing;
            set => this.SetProperty(ref this.individualizing, value);
        }
        private bool individualizing = false;

        /// <summary>
        /// 各文字を移動座標上に表示するか否かを取得する。
        /// </summary>
        [ExoFileItem(ExoFileItemNameOfIsAligningOnMotion, Order = 4)]
        [DataMember]
        public bool IsAligningOnMotion
        {
            get => this.aligningOnMotion;
            set => this.SetProperty(ref this.aligningOnMotion, value);
        }
        private bool aligningOnMotion = false;

        /// <summary>
        /// 高さを自動調整するか否かを取得する。
        /// </summary>
        [ExoFileItem(ExoFileItemNameOfIsAutoAdjusting, Order = 9)]
        [DataMember]
        public bool IsAutoAdjusting
        {
            get => this.autoAdjusting;
            set => this.SetProperty(ref this.autoAdjusting, value);
        }
        private bool autoAdjusting = false;

        /// <summary>
        /// フォント色を取得または設定する。
        /// </summary>
        [ExoFileItem(ExoFileItemNameOfFontColor, Order = 16)]
        [DataMember]
        public Color FontColor
        {
            get => this.fontColor;
            set
            {
                this.SetProperty(
                    ref this.fontColor,
                    Color.FromRgb(value.R, value.G, value.B));
                this.PreviewFontColor = new SolidColorBrush(this.fontColor);
            }
        }
        private Color fontColor = Colors.Black;

        [DataMember]
        public SolidColorBrush PreviewFontColor
        {
            get => this.previewFontColor;
            private set => this.SetProperty(ref this.previewFontColor, value);
        }
        private SolidColorBrush previewFontColor = new SolidColorBrush(Colors.Black);

        /// <summary>
        /// フォント装飾色を取得または設定する。
        /// </summary>
        [ExoFileItem(ExoFileItemNameOfFontDecorationColor, Order = 17)]
        [DataMember]
        public Color FontDecorationColor
        {
            get => this.fontDecorationColor;
            set =>
                this.SetProperty(
                    ref this.fontDecorationColor,
                    Color.FromRgb(value.R, value.G, value.B));
        }
        private Color fontDecorationColor = Colors.Black;

        /// <summary>
        /// フォントファミリ名を取得または設定する。
        /// </summary>
        [ExoFileItem(ExoFileItemNameOfFontFamilyName, Order = 18)]
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
        static IDictionary<string, Uri> fontPathDictionary =
            SearchFontNamePathPair(new[] { CultureInfo.CurrentUICulture });
        static IDictionary<string, Uri> SearchFontNamePathPair(IEnumerable<CultureInfo> cultures)
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


            IDictionary<string, Dictionary<string, Uri>> list = new Dictionary<string, Dictionary<string, Uri>>(); 
            foreach (Uri uri in uris)
            {
                try
                {
                    GlyphTypeface gtf = new GlyphTypeface(uri);
                    var cultureEnUS = new CultureInfo("en-US");

                    //Console.WriteLine("===");
                    //Console.WriteLine(uri);
                    //Console.WriteLine(gtf.FaceNames[cultureEnUS]);

                    foreach (string FamilyName in gtf.FamilyNames.Values)
                    {
                        //Console.WriteLine(FamilyName);
                        var faceName = gtf.FaceNames[cultureEnUS]
                                .Replace("Regular", "")
                                .Replace(" Bold", "")
                                .Replace(" Italic", "");

                        if (faceName == "Bold")
                        {
                            faceName = faceName.Replace("Bold", "");
                        }
                        else if (faceName == "Italic")
                        {
                            faceName = faceName.Replace("Italic", "");
                        }
                        else if (faceName == "Bold Italic")
                        {
                            faceName = faceName.Replace("Bold Italic", "");
                        }

                        var fontName = faceName != ""
                            ? $"{FamilyName} {faceName}"
                            : $"{FamilyName}";
                        if (!list.ContainsKey(fontName))
                        {
                            list.Add(fontName, new Dictionary<string, Uri>());
                        }
                        list[fontName].Add(gtf.FaceNames[cultureEnUS], uri);
                    }
                }
                catch
                {

                }
            }

            foreach (string k in list.Keys)
            {
                //Console.WriteLine("======");
                //Console.WriteLine("=="+k+"==");
                //foreach (string s in list[k].Keys)
                //{
                //    Console.WriteLine(s);
                    
                //}

                if (list[k].Keys.Count==1 && list[k].ContainsKey("Bold"))
                {
                    dic.Add(k, list[k]["Bold"]);
                } 
                else if (list[k].Keys.Count == 1 && list[k].ContainsKey("Italic"))
                {
                    dic.Add(k, list[k]["Italic"]);
                }
                else if (list[k].Keys.Count == 1 && list[k].ContainsKey("Bold Italic"))
                {
                    dic.Add(k, list[k]["Bold Italic"]);
                }
                else if (list[k].ContainsKey("Regular"))
                {
                    dic.Add(k, list[k]["Regular"]);
                }
                else
                {
                    var faceName = k.Substring(k.LastIndexOf(" ")+1);
                    dic.Add(k, list[k][faceName]);
                }

            }

            foreach(string name in dic.Keys)
            {
                Console.WriteLine(name + ":" + dic[name]);
            }


            //foreach (Uri uri in uris)
            //{
            //    try
            //    {
            //        GlyphTypeface gtf = new GlyphTypeface(uri);
            //        var cultureEnUS = new CultureInfo("en-US");


            //        if ((gtf.FaceNames[cultureEnUS].Contains("Bold")
            //                && !gtf.FamilyNames.Values.Contains("Unispace")
            //                && !gtf.FamilyNames.Values.Contains("UD デジタル 教科書体 N-B")
            //                && !gtf.FamilyNames.Values.Contains("UD デジタル 教科書体 NP-B")
            //                && !gtf.FamilyNames.Values.Contains("UD デジタル 教科書体 NK-B")
            //                )
            //            || gtf.FaceNames[cultureEnUS].Contains("Italic") 
            //            || gtf.FaceNames[cultureEnUS].Contains("Bold Italic"))
            //        {
            //            continue;
            //        }
            //        foreach (string FamilyName in gtf.FamilyNames.Values)
            //        {
            //            // 「*** Bold」「*** Italic」というFaceNameを「***」のみに
            //            var faceName = gtf.FaceNames[cultureEnUS]
            //                .Replace("Regular", "")
            //                .Replace(" Bold", "")
            //                .Replace(" Italic", "");
                        
            //            var fontName = faceName != "" 
            //                ? $"{FamilyName} {faceName}"
            //                : $"{FamilyName}";
            //            if (!dic.ContainsKey(fontName))
            //            {
            //                dic.Add(fontName, uri);
            //            }
            //        }
            //    }
            //    catch (NullReferenceException) { }
            //}

            return dic;
        }

        public Uri PreviewFontUri
        {
            get
            {
                try
                {
                    return FontPathDictionary[this.FontFamilyName];
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return FontPathDictionary["MS Gothic"];
                }
            }
        }

        /// <summary>
        /// フォント装飾種別を取得または設定する。
        /// </summary>
        [ExoFileItem(
            ExoFileItemNameOfFontDecoration,
            typeof(FontDecorationConverter),
            Order = 8)]
        public FontDecoration FontDecoration
        {
            get => this.fontDecoration;
            set =>
                this.SetProperty(
                    ref this.fontDecoration,
                    Enum.IsDefined(value.GetType(), value) ? value : FontDecoration.None);
        }
        private FontDecoration fontDecoration = FontDecoration.None;

        /// <summary>
        /// FontDecoration プロパティのシリアライズ用ラッパプロパティ。
        /// </summary>
        [DataMember(Name = nameof(FontDecoration))]
        [SuppressMessage("CodeQuality", "IDE0051")]
        private string FontDecorationString
        {
            get => this.FontDecoration.ToString();
            set =>
                this.FontDecoration =
                    Enum.TryParse(value, out FontDecoration deco) ?
                        deco : FontDecoration.None;
        }

        /// <summary>
        /// テキスト配置種別を取得または設定する。
        /// </summary>
        [ExoFileItem(ExoFileItemNameOfTextAlignment, Order = 12)]
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
        /// 太字にするか否かを取得する。
        /// </summary>
        [ExoFileItem(ExoFileItemNameOfIsBold, Order = 6)]
        [DataMember]
        public bool IsBold
        {
            get => this.bold;
            set => this.SetProperty(ref this.bold, value);
        }
        private bool bold = false;

        /// <summary>
        /// イタリック体にするか否かを取得する。
        /// </summary>
        [ExoFileItem(ExoFileItemNameOfIsItalic, Order = 7)]
        [DataMember]
        public bool IsItalic
        {
            get => this.italic;
            set => this.SetProperty(ref this.italic, value);
        }
        private bool italic = false;

        /// <summary>
        /// 字間幅を取得または設定する。
        /// </summary>
        [ExoFileItem(ExoFileItemNameOfLetterSpace, typeof(SpaceConverter), Order = 13)]
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
        [ExoFileItem(ExoFileItemNameOfLineSpace, typeof(SpaceConverter), Order = 14)]
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
        /// 高精細モードを有効にするか否かを取得または設定する。
        /// </summary>
        [ExoFileItem(ExoFileItemNameOfIsHighDefinition, Order = 15)]
        [DataMember]
        public bool IsHighDefinition
        {
            get => this.highDefinition;
            set => this.SetProperty(ref this.highDefinition, value);
        }
        private bool highDefinition = true;

        /// <summary>
        /// 文字を滑らかにするか否かを取得または設定する。
        /// </summary>
        [ExoFileItem(ExoFileItemNameOfIsSoft, Order = 10)]
        [DataMember]
        public bool IsSoft
        {
            get => this.soft;
            set => this.SetProperty(ref this.soft, value);
        }
        private bool soft = true;

        /// <summary>
        /// 等間隔モードを有効にするか否かを取得または設定する。
        /// </summary>
        [ExoFileItem(ExoFileItemNameOfIsMonospacing, Order = 11)]
        [DataMember]
        public bool IsMonospacing
        {
            get => this.monospacing;
            set => this.SetProperty(ref this.monospacing, value);
        }
        private bool monospacing = false;

        /// <summary>
        /// テキストを取得または設定する。
        /// </summary>
        [ExoFileItem(ExoFileItemNameOfText, typeof(TextConverter), Order = 19)]
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

        private bool isTopAlignment(PreviewTextAlignment alignment)
            => alignment == PreviewTextAlignment.TopLeft
            || alignment == PreviewTextAlignment.TopCenter
            || alignment == PreviewTextAlignment.TopRight;
        private bool isMiddleAlignment(PreviewTextAlignment alignment)
            => alignment == PreviewTextAlignment.MiddleLeft
            || alignment == PreviewTextAlignment.MiddleCenter
            || alignment == PreviewTextAlignment.MiddleRight;
        private bool isBottomAlignment(PreviewTextAlignment alignment)
            => alignment == PreviewTextAlignment.BottomLeft
            || alignment == PreviewTextAlignment.BottomCenter
            || alignment == PreviewTextAlignment.BottomRight;
        
        private Thickness getPreviewLineSpace()
        {
            var val = 2;
            if (isTopAlignment(this.TextAlignment))
                return new Thickness(0, 0, 0, val);
            else if (isMiddleAlignment(this.TextAlignment))
                return new Thickness(0, val / 2.0, 0, val / 2.0);
            else if (isBottomAlignment(this.TextAlignment))
                return new Thickness(0, val, 0, 0);
            else
                return new Thickness(0);
        }

        public Thickness PreviewLineSpace
        {
            get => getPreviewLineSpace();
        }
        
        private VerticalAlignment getVertical()
        {
            if (isTopAlignment(this.TextAlignment))
                return VerticalAlignment.Top;
            else if (isMiddleAlignment(this.TextAlignment))
                return VerticalAlignment.Center;
            else if (isBottomAlignment(this.TextAlignment))
                return VerticalAlignment.Bottom;
            else
                return VerticalAlignment.Center;
        }

        public VerticalAlignment Vertical
        {
            get => getVertical();
        }

        private bool isLeftAlignment(PreviewTextAlignment alignment)
                => alignment == PreviewTextAlignment.TopLeft
                || alignment == PreviewTextAlignment.MiddleLeft
                || alignment == PreviewTextAlignment.BottomLeft;
        private bool isCenterAlignment(PreviewTextAlignment alignment)
            => alignment == PreviewTextAlignment.TopCenter
            || alignment == PreviewTextAlignment.MiddleCenter
            || alignment == PreviewTextAlignment.BottomCenter;
        private bool isRightAlignment(PreviewTextAlignment alignment)
            => alignment == PreviewTextAlignment.TopRight
            || alignment == PreviewTextAlignment.MiddleRight
            || alignment == PreviewTextAlignment.BottomRight;

        private HorizontalAlignment getHorizontal()
        {
            if (isLeftAlignment(this.TextAlignment))
                return HorizontalAlignment.Left;
            else if (isCenterAlignment(this.TextAlignment))
                return HorizontalAlignment.Center;
            else if (isRightAlignment(this.TextAlignment))
                return HorizontalAlignment.Right;
            else
                return HorizontalAlignment.Center;
        }

        public HorizontalAlignment Horizontal
        {
            get => getHorizontal();
        }

        /// <summary>
        /// このコンポーネントのクローンを作成する。
        /// </summary>
        /// <returns>クローン。</returns>
        public PreviewTextComponent Clone() => new PreviewTextComponent(this);

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

        /// <summary>
        /// 表示速度用の定数情報クラス。
        /// </summary>
        [SuppressMessage("Design", "CA1034")]
        [SuppressMessage("Performance", "CA1815")]
        public struct TextSpeedConst : IMovableValueConstants
        {
            public int Digits => 1;
            public decimal DefaultValue => 0;
            public decimal MinValue => 0;
            public decimal MaxValue => 800;
            public decimal MinSliderValue => 0;
            public decimal MaxSliderValue => 100;
        }

        #endregion

        #region 特殊プロパティ用コンポーネントアイテムコンバータ

        /// <summary>
        /// フォント装飾種別用のコンバータクラス。
        /// </summary>
        /// <remarks>
        /// 『ゆっくりMovieMaker3』で「ソフトシャドー(濃)」を選択すると
        /// 範囲外の値が書き出されるため、その対処を行う。
        /// </remarks>
        [SuppressMessage("Design", "CA1034")]
        [SuppressMessage("Performance", "CA1815")]
        public class FontDecorationConverter : IExoFileValueConverter
        {
            /// <summary>
            /// コンストラクタ。
            /// </summary>
            public FontDecorationConverter()
            {
            }

            /// <summary>
            /// .NETオブジェクト値を拡張編集オブジェクトファイルの文字列値に変換する。
            /// </summary>
            /// <param name="value">.NETオブジェクト値。</param>
            /// <param name="objectType">.NETオブジェクトの型情報。</param>
            /// <returns>文字列値。変換できなかった場合は null 。</returns>
            public string ToExoFileValue(object value, Type objectType)
            {
                try
                {
                    return
                        Convert
                            .ChangeType(value, Enum.GetUnderlyingType(objectType))
                            .ToString();
                }
                catch { }
                return null;
            }

            /// <summary>
            /// 拡張編集オブジェクトファイルの文字列値を.NETオブジェクト値に変換する。
            /// </summary>
            /// <param name="value">文字列値。</param>
            /// <param name="objectType">.NETオブジェクトの型情報。</param>
            /// <returns>
            /// .NETオブジェクト値を持つタプル。変換できなかったならば null 。
            /// </returns>
            public Tuple<object> FromExoFileValue(string value, Type objectType)
            {
                try
                {
                    var v =
                        Convert.ChangeType(value, Enum.GetUnderlyingType(objectType));
                    return
                        Tuple.Create(
                            Enum.IsDefined(objectType, v) ?
                                Enum.ToObject(objectType, v) : FontDecoration.None);
                }
                catch { }
                return null;
            }
        }

        /// <summary>
        /// 字間幅および行間幅用のコンバータクラス。
        /// </summary>
        /// <remarks>
        /// AviUtl拡張編集が byte (0 ～ 255) で扱っているようなのでそれに合わせる。
        /// </remarks>
        [SuppressMessage("Design", "CA1034")]
        [SuppressMessage("Performance", "CA1815")]
        public class SpaceConverter : IExoFileValueConverter
        {
            /// <summary>
            /// コンストラクタ。
            /// </summary>
            public SpaceConverter()
            {
            }

            /// <summary>
            /// .NETオブジェクト値を拡張編集オブジェクトファイルの文字列値に変換する。
            /// </summary>
            /// <param name="value">.NETオブジェクト値。</param>
            /// <param name="objectType">.NETオブジェクトの型情報。</param>
            /// <returns>文字列値。変換できなかった場合は null 。</returns>
            public string ToExoFileValue(object value, Type objectType)
            {
                if (objectType == null)
                {
                    throw new ArgumentNullException(nameof(objectType));
                }

                if (value == null)
                {
                    return null;
                }

                try
                {
                    return ((byte)Convert.ToSByte(value)).ToString();
                }
                catch { }
                return null;
            }

            /// <summary>
            /// 拡張編集オブジェクトファイルの文字列値を.NETオブジェクト値に変換する。
            /// </summary>
            /// <param name="value">文字列値。</param>
            /// <param name="objectType">.NETオブジェクトの型情報。</param>
            /// <returns>
            /// .NETオブジェクト値を持つタプル。変換できなかったならば null 。
            /// </returns>
            public Tuple<object> FromExoFileValue(string value, Type objectType)
            {
                if (objectType == null)
                {
                    throw new ArgumentNullException(nameof(objectType));
                }

                if (!byte.TryParse(value, out var exoValue))
                {
                    return null;
                }

                try
                {
                    return Tuple.Create(Convert.ChangeType((sbyte)exoValue, objectType));
                }
                catch { }
                return null;
            }
        }

        /// <summary>
        /// テキスト用のコンバータクラス。
        /// </summary>
        [SuppressMessage("Design", "CA1034")]
        [SuppressMessage("Performance", "CA1815")]
        public class TextConverter : IExoFileValueConverter
        {
            /// <summary>
            /// コンストラクタ。
            /// </summary>
            public TextConverter()
            {
            }

            /// <summary>
            /// .NETオブジェクト値を拡張編集オブジェクトファイルの文字列値に変換する。
            /// </summary>
            /// <param name="value">.NETオブジェクト値。</param>
            /// <param name="objectType">.NETオブジェクトの型情報。</param>
            /// <returns>文字列値。変換できなかった場合は null 。</returns>
            public string ToExoFileValue(object value, Type objectType) =>
                (objectType == null) ?
                    throw new ArgumentNullException(nameof(objectType)) :
                    !(value is string propValue) ?
                        null :
                        string.Join(
                            null,
                            propValue
                                .PadRight(TextLengthLimit + 1, '\0')
                                .Select(c => Convert(c).ToString(@"x4")));

            /// <summary>
            /// 拡張編集オブジェクトファイルの文字列値を.NETオブジェクト値に変換する。
            /// </summary>
            /// <param name="value">文字列値。</param>
            /// <param name="objectType">.NETオブジェクトの型情報。</param>
            /// <returns>
            /// .NETオブジェクト値を持つタプル。変換できなかったならば null 。
            /// </returns>
            public Tuple<object> FromExoFileValue(string value, Type objectType)
            {
                if (objectType == null)
                {
                    throw new ArgumentNullException(nameof(objectType));
                }

                if (objectType != typeof(string) || string.IsNullOrWhiteSpace(value))
                {
                    return null;
                }

                // 文字数が4の倍数でなければ不可
                var exoValue = value.Trim();
                if (exoValue.Length % 4 != 0)
                {
                    return null;
                }

                // 4文字ずつ切り取り char 値を表す int 配列にする
                var charInts =
                    Enumerable
                        .Range(0, exoValue.Length / 4)
                        .Select(
                            i =>
                            {
                                // 16進数文字列から int に変換
                                bool ok =
                                    int.TryParse(
                                        exoValue.Substring(i * 4, 4),
                                        NumberStyles.AllowHexSpecifier,
                                        CultureInfo.InvariantCulture,
                                        out var c);

                                // 変換失敗時は -1 を返す
                                return ok ? c : -1;
                            });

                // 範囲外の値が含まれるなら変換失敗
                if (charInts.Any(c => c < ushort.MinValue || c > ushort.MaxValue))
                {
                    return null;
                }

                // '\0' の手前までを文字列化
                var result =
                    new string(
                        charInts
                            .TakeWhile(c => c != 0)
                            .Select(c => Convert((ushort)c))
                            .ToArray());
                return Tuple.Create<object>(result);
            }

            /// <summary>
            /// char 値をネットワークバイトオーダーの ushort 値に変換する。
            /// </summary>
            /// <param name="value">char 値。</param>
            /// <returns>ネットワークバイトオーダーの ushort 値。</returns>
            private static ushort Convert(char value) =>
                (ushort)IPAddress.HostToNetworkOrder((short)value);

            /// <summary>
            /// ネットワークバイトオーダーの ushort 値を char 値に変換する。
            /// </summary>
            /// <param name="value">ネットワークバイトオーダーの ushort 値。</param>
            /// <returns>char 値。</returns>
            private static char Convert(ushort value) =>
                (char)IPAddress.NetworkToHostOrder((short)value);
        }

        #endregion
    }
}
